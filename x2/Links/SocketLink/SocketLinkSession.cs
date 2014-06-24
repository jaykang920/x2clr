// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

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

        protected SocketLink link;  // associated Link
        protected Socket socket;    // underlying socket

        protected Queue<Event> sendQueue;

        protected Buffer recvBuffer;
        protected Buffer sendBuffer;

        protected IList<ArraySegment<byte>> recvBufferList;
        protected IList<ArraySegment<byte>> sendBufferList;

        // Operation context details
        protected int lengthToReceive;                // rx
        protected int lengthToSend;                   // tx
        protected bool beginning;                     // rx
        protected bool sending;                       // tx
        protected bool transformed;                   // rx
        protected byte[] headerBytes = new byte[5];   // tx

        protected volatile bool hasReceived;
        protected volatile bool hasSent;
        protected int failureCount;

        protected volatile bool rxTransformReady;
        protected volatile bool txTransformReady;

        public SocketLink Link { get { return link; } }

        /// <summary>
        /// Gets the underlying socket object.
        /// </summary>
        public Socket Socket { get { return socket; } }

        /// <summary>
        /// Gets a boolean value indicating whether this session is an active
        /// (client) session. A passive (server-side) session will return false.
        /// </summary>
        public bool Polarity { get; set; }

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

        public bool RxTransformReady
        {
            get { return rxTransformReady; }
            set { rxTransformReady = value; }
        }
        public bool TxTransformReady
        {
            get { return txTransformReady; }
            set { txTransformReady = value; }
        }

        protected SocketLinkSession(SocketLink link, Socket socket)
            : base(socket.Handle)
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

        /// <summary>
        /// Closes the session.
        /// </summary>
        public override void Close()
        {
            if (socket == null)
            {
                return;
            }
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            socket.Close();
            socket = null;

            Log.Info("{0} {1} closed", link.Name, Handle);
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
            lock (sendQueue)
            {
                if (sending)
                {
                    sendQueue.Enqueue(e);
                    return;
                }

                sending = true;
            }

            BeginSend(e);
        }

        internal void BeginReceive(bool beginning)
        {
            this.beginning = beginning;

            recvBufferList.Clear();
            recvBuffer.ListAvailableSegments(recvBufferList);

            ReceiveImpl();
        }

        protected abstract void ReceiveImpl();
        protected abstract void SendImpl();

        protected void ReceiveInternal(int bytesTransferred)
        {
            hasReceived = true;

            Diag.AddBytesReceived(bytesTransferred);

            Log.Trace("{0} {1} received {2} byte(s)",
                link.Name, Handle, bytesTransferred);

            recvBuffer.Stretch(bytesTransferred);

            if (beginning)
            {
                recvBuffer.Rewind();
                uint header;
                int headerLength = recvBuffer.ReadVariable(out header);
                recvBuffer.Shrink(headerLength);
                lengthToReceive = (int)(header >> 1);
                transformed = ((header & 1) != 0);
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

                if (BufferTransform != null && RxTransformReady && transformed)
                {
                    try
                    {
                        BufferTransform.InverseTransform(recvBuffer, lengthToReceive);
                        recvBuffer.Rewind();
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} {1} buffer transform error: {2}", link.Name, Handle, e.Message);
                        goto next;
                    }
                }

                int typeId;
                recvBuffer.Read(out typeId);

                var retrieved = Event.Create(typeId);
                if (retrieved == null)
                {
                    Log.Error("{0} {1} unknown event type id {2}", link.Name, Handle, typeId);
                }
                else
                {
                    try
                    {
                        retrieved.Load(recvBuffer);
                        retrieved._Handle = Handle;
                        if (link.Preprocessor != null)
                        {
                            link.Preprocessor(retrieved, this);
                        }

                        Log.Debug("{0} {1} received event {2}", link.Name, Handle, retrieved);

                        switch (typeId)
                        {
                            case (int)SocketLinkEventType.KeepaliveEvent:
                                break;
                            case (int)SocketLinkEventType.HandshakeReq:
                                {
                                    var e = (HandshakeReq)retrieved;
                                    byte[] response = BufferTransform.Handshake(e.Data);
                                    Send(new HandshakeResp {
                                        _Transform = false,
                                        Data = response
                                    });
                                }
                                break;
                            case (int)SocketLinkEventType.HandshakeResp:
                                {
                                    var ack = new HandshakeAck { _Transform = false };
                                    var e = (HandshakeResp)retrieved;
                                    if (BufferTransform.FinalizeHandshake(e.Data))
                                    {
                                        RxTransformReady = true;
                                        ack.Result = true;
                                    }
                                    else
                                    {
                                        //
                                    }
                                    Send(ack);
                                }
                                break;
                            case (int)SocketLinkEventType.HandshakeAck:
                                {
                                    var e = (HandshakeAck)retrieved;

                                    if (e.Result)
                                    {
                                        TxTransformReady = true;
                                    }

                                    link.Flow.Publish(new LinkSessionConnected {
                                        LinkName = link.Name,
                                        Result = e.Result,
                                        Context = this
                                    });
                                }
                                break;
                            default:
                                link.Flow.Publish(retrieved);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} {1} error loading event {2}: {3}", link.Name, Handle, typeId, e.ToString());
                    }
                }
            next:
                recvBuffer.Trim();
                if (recvBuffer.IsEmpty)
                {
                    break;
                }

                uint header;
                int headerLength = recvBuffer.ReadVariable(out header);
                recvBuffer.Shrink(headerLength);
                lengthToReceive = (int)(header >> 1);
                transformed = ((header & 1) != 0);

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

        private void BeginSend(Event e)
        {
            if (e.GetTypeId() != (int)SocketLinkEventType.KeepaliveEvent)
            {
                hasSent = true;
            }

            e.Serialize(sendBuffer);

            uint header = 0;
            if (BufferTransform != null && TxTransformReady && e._Transform)
            {
                BufferTransform.Transform(sendBuffer, sendBuffer.Length);
                header = 1;
            }
            header |= ((uint)sendBuffer.Length << 1);

            int headerLength = Buffer.WriteVariable(headerBytes, header);
            lengthToSend = sendBuffer.Length + headerLength;

            sendBufferList.Clear();
            sendBufferList.Add(new ArraySegment<byte>(headerBytes, 0, headerLength));
            sendBuffer.ListOccupiedSegments(sendBufferList);

            SendImpl();

            Log.Debug("{0} {1} sent event {2}", link.Name, Handle, e);
        }

        private void TrySendNext()
        {
            Event e;
            lock (sendQueue)
            {
                if (sendQueue.Count == 0)
                {
                    sending = false;
                    return;
                }

                e = sendQueue.Dequeue();
            }

            BeginSend(e);
        }

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

                Interlocked.Add(ref totalBytesSent, bytesSent);
                Interlocked.Add(ref this.bytesSent, bytesSent);

                if (owner.Link.Diag != null)
                {
                    owner.Link.Diag.AddBytesSent(bytesSent);
                }
            }
        }

        #endregion  // Diagnostics
    }
}
