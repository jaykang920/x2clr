// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Abstract base class for TCP/IP link sessions.
    /// </summary>
    public abstract class AbstractTcpSession : LinkSession
    {
        protected Socket socket;

        private AtomicInt keepaliveFailureCount;
        private volatile bool hasReceived;
        private volatile bool hasSent;

        public bool Connected
        {
            get { return (socket != null && socket.Connected); }
        }

        /// <summary>
        /// Gets the remote ip address string of this session, or null.
        /// </summary>
        public string RemoteAddress
        {
            get
            {
                IPEndPoint endpoint = RemoteEndPoint;
                if (endpoint != null)
                {
                    return endpoint.Address.ToString();
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the remote port number of this session, or zero.
        /// </summary>
        public int RemotePort
        {
            get
            {
                IPEndPoint endpoint = RemoteEndPoint;
                if (endpoint != null)
                {
                    return endpoint.Port;
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets the remote endpoint of this session, or null.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                EndPoint endpoint;
                try
                {
                    endpoint = socket.RemoteEndPoint;
                }
                catch (ObjectDisposedException)
                {
                    return null;
                }
                return endpoint as IPEndPoint;
            }
        }

        /// <summary>
        /// Gets the underlying Socket object.
        /// </summary>
        public Socket Socket { get { return socket; } }

        // Keepalive properties
        /// <summary>
        /// Gets or sets a boolean value indicating whether this link session
        /// ignores keepalive failures.
        /// </summary>
        public bool IgnoreKeepaliveFailure { get; set; }

        internal bool HasReceived
        {
            get { return hasReceived; }
            set { hasReceived = value; }
        }

        internal bool HasSent
        {
            get { return hasSent; }
            set { hasSent = value; }
        }

        /// <summary>
        /// Initializes a new instance of the AbstractTcpSession class.
        /// </summary>
        protected AbstractTcpSession(SessionBasedLink link, Socket socket)
            : base(link)
        {
            this.socket = socket;
            keepaliveFailureCount = new AtomicInt();
        }

        internal int Keepalive(bool checkIncoming, bool checkOutgoing)
        {
            int result = 0;

            if (checkIncoming)
            {
                if (hasReceived)
                {
                    hasReceived = false;
                    keepaliveFailureCount.Reset();
                }
                else
                {
                    if (!IgnoreKeepaliveFailure)
                    {
                        result = keepaliveFailureCount.Increment();
                    }
                }
            }

            if (checkOutgoing)
            {
                if (hasSent)
                {
                    hasSent = false;
                }
                else
                {
                    Log.Trace("{0} {1} sent keepalive event", link.Name, handle);

                    Send(new KeepaliveEvent { _Transform = false });
                }
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            if (socket != null)
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                socket.Close();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by a derived session class on send/receive error.
        /// </summary>
        protected void OnDisconnect()
        {
            IPEndPoint endpoint = RemoteEndPoint;
            if (endpoint != null)
            {
                OnDisconnect(endpoint);
            }
        }

        protected override bool Process(Event e)
        {
            switch (e.GetTypeId())
            {
                case (int)SocketLinkEventType.KeepaliveEvent:
                    break;
                default:
                    return base.Process(e);
            }
            return true;
        }

        protected override void LogEventReceived(Event e)
        {
            hasReceived = true;

            if (e.GetTypeId() != SocketLinkEventType.KeepaliveEvent)
            {
                Log.Debug("{0} {1} received event {2}", link.Name, Handle, e);
            }
            else
            {
                Log.Trace("{0} {1} received event {2}", link.Name, Handle, e);
            }

            base.LogEventReceived(e);
        }

        protected override void LogEventSent(Event e)
        {
            hasSent = true;

            if (e.GetTypeId() != SocketLinkEventType.KeepaliveEvent)
            {
                Log.Debug("{0} {1} sent event {2}", link.Name, Handle, e);
            }
            else
            {
                Log.Trace("{0} {1} sent event {2}", link.Name, Handle, e);
            }

            base.LogEventSent(e);
        }
    }
}
