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

        private int keepaliveFailureCount;
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
                catch (Exception)
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
        }

        /// <summary>
        /// Called on send/receive error.
        /// </summary>
        public void OnDisconnect()
        {
            OnDisconnect(RemoteEndPoint);

            CloseInternal();
        }

        protected override void OnClose()
        {
            OnDisconnect(RemoteEndPoint);
        }

        internal int Keepalive(bool checkIncoming, bool checkOutgoing)
        {
            int result = 0;

            if (checkIncoming)
            {
                if (hasReceived)
                {
                    hasReceived = false;
                    Interlocked.Exchange(ref keepaliveFailureCount, 0);
                }
                else
                {
                    if (!IgnoreKeepaliveFailure)
                    {
                        result = Interlocked.Increment(ref keepaliveFailureCount);

                        Log.Warn("{0} {1} keepalive failure count {2}",
                            link.Name, handle, result);
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

                    Send(Hub.HeartbeatEvent);
                }
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            if (socket != null)
            {
                try
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    socket.Close();
                }
                catch (Exception e)
                {
                    Log.Warn("{0} {1} close : {2}",
                        Link.Name, handle, e.Message);
                }
            }

            base.Dispose(disposing);
        }

        protected override void BuildHeader(SendBuffer sendBuffer, bool transformed)
        {
            uint header = (uint)(transformed ? 1 : 0);
            header |= ((uint)sendBuffer.Buffer.Length << 1);

            sendBuffer.HeaderLength = Serializer.WriteVariable(sendBuffer.HeaderBytes, header);
        }

        protected override bool ParseHeader()
        {
            uint header;
            int headerLength;
            try
            {
                headerLength = rxBuffer.ReadVariable(out header);
            }
            catch (System.IO.EndOfStreamException)
            {
                // Need more to start parsing.
                return false;
            }
            rxBuffer.Shrink(headerLength);
            lengthToReceive = (int)(header >> 1);
            rxTransformed = ((header & 1) != 0);
            return true;
        }

        protected override bool Process(Event e)
        {
            switch (e.GetTypeId())
            {
                case BuiltinEventType.HeartbeatEvent:
                    // Do nothing
                    break;
                default:
                    return base.Process(e);
            }
            return true;
        }

        protected override void LogEventReceived(Event e)
        {
            hasReceived = true;

            if (e.GetTypeId() != BuiltinEventType.HeartbeatEvent)
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

            if (e.GetTypeId() != BuiltinEventType.HeartbeatEvent)
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
