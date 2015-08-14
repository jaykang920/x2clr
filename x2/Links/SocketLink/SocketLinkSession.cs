// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using x2.Events;
using x2.Flows;
using x2.Queues;

namespace x2.Links.SocketLink
{
    /// <summary>
    /// Common abstract base class for SocketLink sessions.
    /// </summary>
    public abstract class SocketLinkSession : LinkSession
    {
        protected object syncRoot = new Object();
        //protected object syncRx = new Object();
        protected object syncTx = new Object();

        protected SocketLink link;  // associated link
        protected Socket socket;    // underlying socket

        protected Queue<Event> sendQueue;

        protected Buffer recvBuffer;
        protected Buffer sendBuffer;

        protected IList<ArraySegment<byte>> recvBufferList;
        protected IList<ArraySegment<byte>> sendBufferList;

        // Operation context details
        protected int lengthToReceive;                // rx
        protected int lengthToSend;                   // tx
        protected byte[] headerBytes = new byte[5];   // tx

        protected int failureCount;

        // Boolean flags
        protected volatile bool sending;
        protected volatile bool rxBeginning;
        protected volatile bool rxTransformed;
        protected volatile bool rxTransformReady;
        protected volatile bool txTransformReady;
#if SESSION_KEEPALIVE
        protected volatile bool hasReceived;
        protected volatile bool hasSent;
        protected volatile bool ignoreKeepaliveFailure;
#endif
#if SESSION_RECOVERY
        protected volatile bool closing;
        public volatile bool recovered;
#endif

        public object SyncRoot { get { return syncRoot; } }

#if SESSION_KEEPALIVE
        public bool HasReceived
        {
            get { return hasReceived; }
            set { hasReceived = value; }
        }

        public bool HasSent
        {
            get { return hasSent; }
            set { hasSent = value; }
        }
        public bool IgnoreKeepaliveFailure
        {
            get { return ignoreKeepaliveFailure; }
            set { ignoreKeepaliveFailure = value; }
        }
#endif

        public SocketLink Link { get { return link; } }

        /// <summary>
        /// Gets the underlying socket object.
        /// </summary>
        public Socket Socket { get { return socket; } }

        /// <summary>
        /// Gets a boolean value indicating whether this session is an active
        /// (client) session. A passive (server) session will return false.
        /// </summary>
        public bool Polarity { get; set; }

#if SESSION_RECOVERY
        public string Token { get; set; }
        public x2.Flows.Timer.Token TimeoutToken;
#endif

        public string RemoteAddress
        {
            get
            {
                if (!socket.Connected)
                {
                    return "(Closed)";
                }
                var endpoint = socket.RemoteEndPoint as IPEndPoint;
                return endpoint.Address.ToString();
            }
        }

        protected SocketLinkSession(SocketLink link, Socket socket)
        {
            this.link = link;
            this.socket = socket;

            sendQueue = new Queue<Event>();

            recvBuffer = new Buffer(12);
            sendBuffer = new Buffer(12);

            recvBufferList = new List<ArraySegment<byte>>();
            sendBufferList = new List<ArraySegment<byte>>();

            Diag = new Diagnostics(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            lock (syncRoot)
            {
                if (socket == null)
                {
                    return;
                }
#if SESSION_RECOVERY
                closing = disposing;
#endif
                DisposeInternal();
            }

            Log.Info("{0} {1} closed", link.Name, Handle);

            link.OnDisconnect(this);
        }

        protected virtual bool DisposeInternal()
        {
            if (socket == null)
            {
                return false;
            }
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                socket.Close();
                return true;
            }
            catch (SocketException se)
            {
                Log.Warn("{0} {1} Error closing socket: {2}",
                    link.Name, Handle, se.ToString());
                return false;
            }
            finally
            {
                socket = null;
            }
        }

        public int IncrementFailureCount()
        {
            return Interlocked.Increment(ref failureCount);
        }

        public void ResetFailureCount()
        {
            Interlocked.Exchange(ref failureCount, 0);
        }

        /// <summary>
        /// Sends the specified event through this session.
        /// </summary>
        public override void Send(Event e)
        {
            lock (syncTx)
            {
                if (sending)
                {
                    sendQueue.Enqueue(e);
                    return;
                }

                sending = true;

                BeginSend(e);
            }
        }

        internal void BeginReceive(bool beginning)
        {
            rxBeginning = beginning;

            recvBufferList.Clear();
            recvBuffer.ListAvailableSegments(recvBufferList);

            ReceiveImpl();
        }

        protected abstract void ReceiveImpl();
        protected abstract void SendImpl();

        protected void ReceiveInternal(int bytesTransferred)
        {
#if SESSION_KEEPALIVE
            hasReceived = true;
#endif
            Diag.AddBytesReceived(bytesTransferred);

            Log.Trace("{0} {1} received {2} byte(s)",
                link.Name, Handle, bytesTransferred);

            recvBuffer.Stretch(bytesTransferred);

            if (rxBeginning)
            {
                recvBuffer.Position = 0;
                uint header;
                int headerLength;
                try
                {
                    headerLength = recvBuffer.ReadVariable(out header);
                }
                catch (System.IO.EndOfStreamException)
                {
                    // Need more to start parsing.
                    BeginReceive(true);
                    return;
                }
                recvBuffer.Shrink(headerLength);
                lengthToReceive = (int)(header >> 1);
                rxTransformed = ((header & 1) != 0);
            }

            // Handle split packets.
            if (recvBuffer.Length < lengthToReceive)
            {
                BeginReceive(false);
                return;
            }

            while (true)
            {
                recvBuffer.MarkToRead(lengthToReceive);

                Log.Trace("{0} {1} marked {2} byte(s) to read", link.Name, Handle, lengthToReceive);

                if (BufferTransform != null && rxTransformReady && rxTransformed)
                {
                    try
                    {
                        BufferTransform.InverseTransform(recvBuffer, lengthToReceive);
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} {1} buffer transform error: {2}", link.Name, Handle, e.Message);
                        goto next;
                    }
                }
                recvBuffer.Position = 0;

                var deserializer = new Deserializer(recvBuffer);
                Event retrieved = EventFactory.Create(deserializer);
                if (retrieved == null)
                {
                    goto next;
                }
                else
                {
                    try
                    {
                        retrieved.Deserialize(deserializer);
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} {1} error loading event {2}: {3}", link.Name, Handle, retrieved.GetTypeId(), e.ToString());
                        goto next;
                    }

                    Diag.IncrementEventsReceived();

                    retrieved._Handle = Handle;

                    if (link.Preprocessor != null)
                    {
                        link.Preprocessor(retrieved, this);
                    }

                    if (retrieved.GetTypeId() != SocketLinkEventType.KeepaliveEvent)
                    {
                        Log.Debug("{0} {1} received event {2}", link.Name, Handle, retrieved);
                    }
                    else
                    {
                        Log.Trace("{0} {1} received event {2}", link.Name, Handle, retrieved);
                    }

                    ProcessEvent(retrieved);
                }
            next:
                recvBuffer.Trim();
                if (recvBuffer.IsEmpty)
                {
                    break;
                }

                uint header;
                int headerLength;
                try
                {
                    headerLength = recvBuffer.ReadVariable(out header);
                }
                catch (System.IO.EndOfStreamException)
                {
                    BeginReceive(true);
                    return;
                }
                recvBuffer.Shrink(headerLength);
                lengthToReceive = (int)(header >> 1);
                rxTransformed = ((header & 1) != 0);

                if (recvBuffer.Length < lengthToReceive)
                {
                    BeginReceive(false);
                    return;
                }
            }

            BeginReceive(true);
        }

        protected void SendInternal(int bytesTransferred)
        {
            Diag.AddBytesSent(bytesTransferred);
            Diag.IncrementEventsSent();

            Log.Trace("{0} {1} sent {2}/{3} byte(s)",
                link.Name, Handle, bytesTransferred, lengthToSend);

            /* XXX TODO split send
             * Do we really have to consider this case?
            if (bytesTransferred < lengthToSend)
            {
                // Try to send the rest.
                sendBuffer.Shrink(bytesTransferred);
                lengthToSend = sendBuffer.Length;

                sendBufferList.Clear();
                sendBuffer.ListOccupiedSegments(sendBufferList);

                SendImpl();
                return;
            }
            */

            sendBuffer.Trim();

            TrySendNext();
        }

        protected void OnDisconnect()
        {
            Log.Trace("SocketLinkSession.OnDisconnect");

            lock (syncRoot)
            {
                if (!DisposeInternal())
                {
                    return;
                }
            }

#if SESSION_RECOVERY
            if (closing)
            {
#endif
                link.OnDisconnect(this);
#if SESSION_RECOVERY
            }
            else
            {
                if (Polarity == true)
                {
                    var client = (TcpClientBase)link;
                    client.Socket = null;
                    client.Connect(client.RemoteHost, client.RemotePort);
                }
                else
                {
                    var server = (TcpServerBase)link;
                    server.OnInstantDisconnect(this);
                }
            }
#endif
        }

        private void BeginSend(Event e)
        {
#if SESSION_KEEPALIVE
#endif
            e.Serialize(new Serializer(sendBuffer));

            uint header = 0;
            if (BufferTransform != null && txTransformReady && e._Transform)
            {
                BufferTransform.Transform(sendBuffer, (int)sendBuffer.Length);
                header = 1;
            }
            header |= ((uint)sendBuffer.Length << 1);

            int headerLength = Serializer.WriteVariable(headerBytes, header);
            lengthToSend = (int)sendBuffer.Length + headerLength;

            sendBufferList.Clear();
            sendBufferList.Add(new ArraySegment<byte>(headerBytes, 0, headerLength));
            sendBuffer.ListOccupiedSegments(sendBufferList);

            SendImpl();

            if (e.GetTypeId() != SocketLinkEventType.KeepaliveEvent)
            {
                Log.Debug("{0} {1} sent event {2}", link.Name, Handle, e);
            }
            else
            {
                Log.Trace("{0} {1} sent event {2}", link.Name, Handle, e);
            }
        }

        private void TrySendNext()
        {
            if (sendQueue.Count == 0)
            {
                sending = false;
                return;
            }

            Event e = sendQueue.Dequeue();

            BeginSend(e);
        }

        private void ProcessEvent(Event e)
        {
            switch (e.GetTypeId())
            {
                case (int)SocketLinkEventType.HandshakeReq:
                    {
                        var req = (HandshakeReq)e;
                        var resp = new HandshakeResp { _Transform = false };
                        byte[] response = null;
                        try
                        {
                            response = BufferTransform.Handshake(req.Data);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("{0} {1} error handshaking : {2}",
                                link.Name, Handle, ex.ToString());
                        }
                        if (response != null)
                        {
                            resp.Data = response;
                        }
                        Send(resp);
                    }
                    break;
                case (int)SocketLinkEventType.HandshakeResp:
                    {
                        var ack = new HandshakeAck { _Transform = false };
                        var resp = (HandshakeResp)e;
                        try
                        {
                            if (BufferTransform.FinalizeHandshake(resp.Data))
                            {
                                rxTransformReady = true;
                                ack.Result = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("{0} {1} error finishing handshake : {2}",
                                link.Name, Handle, ex.ToString());
                        }
                        Send(ack);
                    }
                    break;
                case (int)SocketLinkEventType.HandshakeAck:
                    {
                        var ack = (HandshakeAck)e;

                        if (ack.Result)
                        {
                            txTransformReady = true;
                        }

                        if (Polarity == true)
                        {
                            var client = (TcpClientBase)link;
                            client.Session = this;
                        }
                        //
                        Hub.Post(new LinkSessionConnected {
                            _Handle = Handle,
                            LinkName = link.Name,
                            Result = ack.Result,
                            Context = this
                        });
                    }
                    break;
#if SESSION_KEEPALIVE
                case (int)SocketLinkEventType.KeepaliveEvent:
                    break;
#endif
#if SESSION_RECOVERY
                case (int)SocketLinkEventType.SessionReq:
                    {
                        if (Polarity == false)
                        {
                            var server = (TcpServerBase)link;
                            server.OnSessionReq(this, (SessionReq)e);
                        }
                    }
                    break;
                case (int)SocketLinkEventType.SessionResp:
                    {
                        if (Polarity == true)
                        {
                            var client = (TcpClientBase)link;
                            client.OnSessionResp(this, (SessionResp)e);
                        }
                    }
                    break;
#endif
                default:
                    Hub.Post(e);
                    break;
            }
        }

#if SESSION_RECOVERY
        public void HandOver(SocketLinkSession oldSession)
        {
            lock (syncRoot)
            {
                BufferTransform = oldSession.BufferTransform;
                if (BufferTransform != null)
                {
                    rxTransformReady = true;
                    txTransformReady = true;
                }
                while (oldSession.sendQueue.Count != 0)
                {
                    sendQueue.Enqueue(oldSession.sendQueue.Dequeue());
                }
            }
        }
#endif

        #region Diagnostics

        /// <summary>
        /// Internal diagnostics helper class.
        /// </summary>
        public class Diagnostics : LinkDiagnostics
        {
            protected SocketLinkSession owner;

            public Diagnostics(SocketLinkSession owner)
            {
                this.owner = owner;
            }

            internal override void AddBytesReceived(long bytesReceived)
            {
                base.AddBytesReceived(bytesReceived);

                if (owner.Link.Diag != null)
                {
                    owner.Link.Diag.AddBytesReceived(bytesReceived);
                }
            }

            internal override void AddBytesSent(long bytesSent)
            {
                base.AddBytesSent(bytesSent);

                if (owner.Link.Diag != null)
                {
                    owner.Link.Diag.AddBytesSent(bytesSent);
                }
            }

            internal override void IncrementEventsReceived()
            {
                base.IncrementEventsReceived();

                if (owner.Link.Diag != null)
                {
                    owner.Link.Diag.IncrementEventsReceived();
                }
            }

            internal override void IncrementEventsSent()
            {
                base.IncrementEventsSent();

                if (owner.Link.Diag != null)
                {
                    owner.Link.Diag.IncrementEventsSent();
                }
            }
        }

        #endregion  // Diagnostics
    }
}
