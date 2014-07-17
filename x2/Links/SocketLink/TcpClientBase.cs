// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using x2.Events;
using x2.Flows;

namespace x2.Links.SocketLink
{
    /// <summary>
    /// Common abstract base class for TCP/IP client links.
    /// </summary>
    public abstract class TcpClientBase : SocketLink
    {
        protected volatile SocketLinkSession session;  // current link session
#if CONNECTION_RECOVERY
        protected SocketLinkSession tempSession;  // temporary link session
#endif

        protected volatile string remoteHost;
        protected volatile int remotePort;

        private Stopwatch stopwatch;
        private int retryCount;

        public int MaxRetryCount { get; set; }  // 0 for unlimited
        public long RetryInterval { get; set; }  // in millisec

        public bool AutoReconnect { get; set; }
        public int ReconnectDelay { get; set; }  // in millisec

        public bool Connected { get { return (session != null); } }

        public SocketLinkSession Session {
            get { return session; }
            set
            {
                session = value;
            }
        }

        public string RemoteHost
        {
            get { return remoteHost; }
            set { remoteHost = value; }
        }
        public int RemotePort
        {
            get { return remotePort; }
            set { remotePort = value; }
        }

        public TcpClientBase(string name)
            : base(name)
        {
            stopwatch = new Stopwatch();
        }

        public override void Close()
        {
            lock (syncRoot)
            {
                if (session == null)
                {
                    return;
                }

                session.Close();

                session = null;
                socket = null;
            }
        }

        public void CloseInternal()
        {
            session.CloseInternal();
            socket = null;
        }

        public void Connect(string host, int port)
        {
            remoteHost = host;
            remotePort = port;

            IPAddress ip = null;
            try
            {
                ip = Dns.GetHostAddresses(host)[0];
            }
            catch (Exception e)
            {
                Log.Error("{0} error resolving target host {1} : {2}",
                    Name, host, e.Message);
                throw;
            }

            Connect(ip, port);
        }

        public void Send(Event e)
        {
            lock (syncRoot)
            {
                if (session == null)
                {
                    Log.Warn("{0} dropped event {1}", Name, e);
                    return;
                }
                session.Send(e);
            }
        }

#if CONNECTION_RECOVERY
        public void SendSessionReq(SocketLinkSession session)
        {
            string sessionToken = null;
            lock (syncRoot)
            {
                var prevSession = this.session;
                if (prevSession != null)
                {
                    sessionToken = prevSession.Token;
                }
            }

            var req = new SessionReq();
            if (!String.IsNullOrEmpty(sessionToken))
            {
                req.Value = sessionToken;
            }
            session.Send(req);
        }

        public void OnSessionResp(SocketLinkSession session, SessionResp e)
        {
            string sessionToken = null;
            var prevSession = this.session;
            if (prevSession != null)
            {
                sessionToken = prevSession.Token;
            }

            if (sessionToken == null)
            {
                session.Token = e.Value;

                if (BufferTransform == null)
                {
                    this.session = session;
                }

                OnSessionSetUp(session);
            }
            else
            {
                if (sessionToken.Equals(e.Value))
                {
                    var oldSession = this.session;
                    session.HandOver(oldSession);
                    this.session = session;
                    tempSession = null;

                    Hub.Post(new LinkSessionRecovered {
                        LinkName = Name,
                        OldHandle = oldSession.Handle,
                        Context = session
                    });
                }
                else
                {
                    Close();
                }
            }
        }
#endif

        public override void OnDisconnect(SocketLinkSession session)
        {
            this.session = null;

            base.OnDisconnect(session);
        }

        protected override void OnKeepaliveTick()
        {
            if (!Connected)
            {
                return;
            }

            if (!Keepalive(session))
            {
                Close();
            }
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Close();

            if (AutoReconnect)
            {
                Thread.Sleep(ReconnectDelay);

                Connect(remoteHost, remotePort);
            }
        }

        private void Connect(IPAddress ip, int port)
        {
            lock (syncRoot)
            {
                try
                {
                    if (socket == null)
                    {
                        socket = new Socket(
                            ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                    BeginConnect(new IPEndPoint(ip, port));

                    Log.Info("{0} connecting to {1}:{2}", Name, ip, port);
                }
                catch (Exception)
                {
                    socket = null;
                    throw;
                }
            }
        }

        private void BeginConnect(EndPoint endpoint)
        {
            stopwatch.Reset();
            stopwatch.Start();

            ConnectImpl(endpoint);
        }

        protected abstract void ConnectImpl(EndPoint endpoint);

        protected void ConnectInternal(SocketLinkSession session)
        {
            // Adjust socket options.
            socket.NoDelay = NoDelay;

            // Reset the retry counter.
            retryCount = 0;

            session.Polarity = true;

#if CONNECTION_RECOVERY
            tempSession = session;

            SendSessionReq(session);
#else
            if (BufferTransform == null)
            {
                this.session = session;
            }
            OnSessionSetUp(session);
#endif

            session.BeginReceive(true);
        }

        protected void RetryInternal(EndPoint endpoint)
        {
            new LinkSessionConnected {
                LinkName = Name,
                Result = false,
                Context = endpoint
            }.Post();

            if (MaxRetryCount <= 0 ||
                (MaxRetryCount > 0 && retryCount < MaxRetryCount))
            {
                ++retryCount;

                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds < RetryInterval)
                {
                    Thread.Sleep((int)(RetryInterval - stopwatch.ElapsedMilliseconds));
                }

                BeginConnect(endpoint);
            }
            else
            {
                socket.Close();
                socket = null;
            }
        }
    }
}
