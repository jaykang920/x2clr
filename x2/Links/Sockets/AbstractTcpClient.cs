// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Abstract base class for TCP/IP client links.
    /// </summary>
    public abstract class AbstractTcpClient : ClientLink
    {
        private int retryCount;
        private DateTime startTime;
        private EndPoint remoteEndPoint;

        private volatile bool incomingKeepaliveEnabled;
        private volatile bool outgoingKeepaliveEnabled;

        private volatile bool connecting;
        private volatile bool recovering;

        /// <summary>
        /// Gets whether this client link is currently connected or not.
        /// </summary>
        public bool Connected
        {
            get { return (session != null && ((AbstractTcpSession)session).Connected); }
        }

        // Socket option properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether the client sockets
        /// are not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

        // Connect properties

        /// <summary>
        /// Gets or sets the maximum number of connection retries before this
        /// link declares a connection failure.
        /// </summary>
        /// <remarks>
        /// Default value is 0 (no retry). A negative integer such as -1 means
        /// that the link should retry for unlimited times.
        /// </remarks>
        public int MaxRetryCount { get; set; }
        /// <summary>
        /// Gets or sets the connection retry interval time in milliseconds.
        /// </summary>
        public double RetryInterval { get; set; }

        // Reconnect properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether this link should
        /// start a new connection attempt automatically on disconnect, toward
        /// the previous remote endpoint.
        /// </summary>
        public bool AutoReconnect { get; set; }
        /// <summary>
        /// Gets or sets a delay before an automatic reconnect, in milliseconds.
        /// </summary>
        public int ReconnectDelay { get; set; }

        // Keepalive properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether this link checks
        /// for incomming keepalive events
        /// </summary>
        public bool IncomingKeepaliveEnabled
        {
            get { return incomingKeepaliveEnabled; }
            set { incomingKeepaliveEnabled = value; }
        }
        /// <summary>
        /// Gets or sets a boolean value indicating whether this link emits
        /// outgoing keepalive events.
        /// </summary>
        public bool OutgoingKeepaliveEnabled
        {
            get { return outgoingKeepaliveEnabled; }
            set { outgoingKeepaliveEnabled = value; }
        }
        /// <summary>
        /// Gets or sets the maximum number of successive keepalive failures
        /// before the link closes the session.
        /// </summary>
        public int MaxKeepaliveFailureCount { get; set; }
        /// <summary>
        /// Gets or sets a boolean value indicating whether this link ignores
        /// the keepalive failure limit instead of clsoing the session.
        /// </summary>
        public bool IgnoreKeepaliveFailure { get; set; }

        static AbstractTcpClient()
        {
            EventFactory.Register<HeartbeatEvent>();
        }

        /// <summary>
        /// Initializes a new instance of the AbstractTcpClient class.
        /// </summary>
        protected AbstractTcpClient(string name)
            : base(name)
        {
            // Default socket options
            NoDelay = true;
        }

        /// <summary>
        /// Connects to the specified host and port.
        /// </summary>
        public void Connect(string host, int port)
        {
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

        /// <summary>
        /// Connects to the specified IP address and port.
        /// </summary>
        public void Connect(IPAddress ip, int port)
        {
            if (connecting)
            {
                throw new InvalidOperationException();
            }
            connecting = true;

            using (new ReadLock(rwlock))
            {
                if (session != null &&
                    ((AbstractTcpSession)session).Connected)
                {
                    throw new InvalidOperationException();
                }
            }

            Connect(null, new IPEndPoint(ip, port));
        }

        /// <summary>
        /// Reconnects to the last successful remote address.
        /// </summary>
        public void Reconnect()
        {
            if (remoteEndPoint == null)
            {
                Log.Error("{0} no reconnect target", Name);
                return;
            }

            connecting = true;

            Connect(null, remoteEndPoint);
        }

        private void Connect(Socket socket, EndPoint endpoint)
        {
            Log.Info("{0} connecting to {1}", Name, endpoint);

            // Reset the retry counter.
            retryCount = 0;

            startTime = DateTime.UtcNow;

            ConnectInternal(socket, endpoint);
        }

        protected override void OnSessionConnectedInternal(bool result, object context)
        {
            base.OnSessionConnectedInternal(result, context);

            connecting = false;
            recovering = false;
        }

        protected override void OnSessionDisconnectedInternal(int handle, object context)
        {
            base.OnSessionDisconnectedInternal(handle, context);

            if (AutoReconnect)
            {
                Thread.Sleep(ReconnectDelay);

                Reconnect();
            }
        }

        internal override void OnInstantDisconnect(LinkSession session)
        {
            LinkSession currentSession = Session;
            if (!Object.ReferenceEquals(session, currentSession))
            {
                Log.Warn("{0} gave up session recovery {1}", Name, session.Handle);
                return;
            }

            recovering = true;

            Reconnect();
        }

        /// <summary>
        /// Provides an actual implementation of Connect.
        /// </summary>
        protected abstract void ConnectInternal(Socket socket, EndPoint endpoint);

        /// <summary>
        /// <see cref="ClientLink.OnConnectInternal"/>
        /// </summary>
        protected override void OnConnectInternal(LinkSession session)
        {
            var tcpSession = (AbstractTcpSession)session;
            Socket socket = tcpSession.Socket;

            // Adjust socket options.
            socket.NoDelay = NoDelay;

            // Save the remote endpoint to reconnect.
            remoteEndPoint = socket.RemoteEndPoint;

            tcpSession.BeginReceive(true);

            Log.Info("{0} {1} connected to {2}",
                Name, tcpSession.InternalHandle, socket.RemoteEndPoint);
            
            base.OnConnectInternal(session);
        }

        /// <summary>
        /// Called by a derived link class when a connection attempt fails.
        /// </summary>
        protected virtual void OnConnectError(Socket socket, EndPoint endpoint)
        {
            if (MaxRetryCount < 0 ||
                (MaxRetryCount > 0 && retryCount < MaxRetryCount))
            {
                if (MaxRetryCount > 0)
                {
                    ++retryCount;
                }

                double elapsedMillisecs =
                    (DateTime.UtcNow - startTime).TotalMilliseconds;
                if (elapsedMillisecs < RetryInterval)
                {
                    Thread.Sleep((int)(RetryInterval - elapsedMillisecs));
                }

                Connect(socket, endpoint);
            }
            else
            {
                socket.Close();

                connecting = false;

                if (recovering)
                {
                    recovering = false;

                    LinkSession deadSession = Session;

                    deadSession.Release();

                    OnLinkSessionDisconnectedInternal(deadSession.Handle, deadSession);
                }
                else
                {
                    OnLinkSessionConnectedInternal(false, endpoint);
                }
            }
        }

        /// <summary>
        /// Called when an existing link session is recovered.
        /// </summary>
        protected virtual void OnSessionRecovered(int handle, object context)
        {
        }

        protected override void Setup()
        {
            base.Setup();

            Bind(new LinkSessionRecovered { LinkName = Name },
                OnLinkSessionRecovered);

            Bind(Hub.HeartbeatEvent, OnHeartbeatEvent);
        }

        private void OnHeartbeatEvent(HeartbeatEvent e)
        {
            if (!IncomingKeepaliveEnabled && !OutgoingKeepaliveEnabled)
            {
                return;
            }

            var tcpSession = (AbstractTcpSession)Session;
            if (tcpSession == null || !tcpSession.Connected)
            {
                return;
            }

            int failureCount = tcpSession.Keepalive(
                incomingKeepaliveEnabled, outgoingKeepaliveEnabled);

            if (MaxKeepaliveFailureCount > 0 &&
                failureCount > MaxKeepaliveFailureCount)
            {
                Log.Error("{0} {1} closed due to the keepalive failure",
                    Name, tcpSession.Handle);

                tcpSession.OnDisconnect();
            }
        }

        // LinkSessionRecovered event handler
        private void OnLinkSessionRecovered(LinkSessionRecovered e)
        {
            OnSessionRecovered(e.Handle, e.Context);
        }
    }
}
