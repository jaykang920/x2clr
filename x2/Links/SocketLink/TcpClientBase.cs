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
    public abstract class TcpClientBase : SocketLink
    {
        protected volatile SocketLinkSession session;

        protected volatile string remoteHost;
        protected volatile int remotePort;

        private Stopwatch stopwatch;
        private int retryCount;

        public int MaxRetryCount { get; set; }  // 0 for unlimited
        public long RetryInterval { get; set; }  // in millisec

        public bool AutoReconnect { get; set; }
        public int ReconnectDelay { get; set; }  // in millisec

        public bool Connected { get { return (session != null); } }

        public SocketLinkSession Session { get { return session; } }

        /// <summary>
        /// Gets or sets a value that indicates whether the underlying socket is
        /// not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

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

            // Default socket options
            NoDelay = true;
        }

        public override void Close()
        {
            if (session == null)
            {
                return;
            }

            session.Close();

            session = null;
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
                Log.Error("{0} error resolving target host {1} - {2}",
                    Name, host, e.Message);
                throw;
            }

            Connect(ip, port);
        }

        public void Send(Event e)
        {
            if (session == null)
            {
                Log.Warn("{0} dropped event {1}", Name, e);
                return;
            }
            session.Send(e);
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
            try
            {
                if (socket == null)
                {
                    socket = new Socket(
                        ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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

        private void BeginConnect(EndPoint endpoint)
        {
            stopwatch.Reset();
            stopwatch.Start();

            ConnectImpl(endpoint);
        }

        protected abstract void ConnectImpl(EndPoint endpoint);

        protected void ConnectInternal()
        {
            // Reset the retry counter.
            retryCount = 0;
        }

        protected void RetryInternal(EndPoint endpoint)
        {
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
        }
    }
}
