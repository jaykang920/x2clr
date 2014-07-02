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

        protected SocketLink link;  // associated link
        protected Socket socket;    // underlying socket

        protected Queue<Event> sendQueue;

        protected Buffer recvBuffer;
        protected Buffer sendBuffer;

        protected IList<ArraySegment<byte>> recvBufferList;
        protected IList<ArraySegment<byte>> sendBufferList;

        public SessionStatus Status;

        // Operation context details
        protected int lengthToReceive;                // rx
        protected int lengthToSend;                   // tx
        protected byte[] headerBytes = new byte[5];   // tx

        protected int failureCount;

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

        //public string Token { get; set; }

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
            Status.Closing = true;

            CloseInternal();

            Log.Info("{0} {1} closed", link.Name, Handle);
        }

        internal void CloseInternal()
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
                if (Status.Sending)
                {
                    sendQueue.Enqueue(e);
                    return;
                }

                Status.Sending = true;
            }

            BeginSend(e);
        }

        internal void BeginReceive(bool beginning)
        {
            Status.RxBeginning = beginning;

            recvBufferList.Clear();
            recvBuffer.ListAvailableSegments(recvBufferList);

            ReceiveImpl();
        }

        protected abstract void ReceiveImpl();
        protected abstract void SendImpl();

        protected void ReceiveInternal(int bytesTransferred)
        {
            Status.HasReceived = true;

            Diag.AddBytesReceived(bytesTransferred);

            Log.Trace("{0} {1} received {2} byte(s)",
                link.Name, Handle, bytesTransferred);

            recvBuffer.Stretch(bytesTransferred);

            if (Status.RxBeginning)
            {
                recvBuffer.Rewind();
                uint header;
                int headerLength = recvBuffer.ReadVariable(out header);
                recvBuffer.Shrink(headerLength);
                lengthToReceive = (int)(header >> 1);
                Status.RxTransformed = ((header & 1) != 0);
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

                if (BufferTransform != null && Status.RxTransformReady && Status.RxTransformed)
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

                        ProcessEvent(retrieved);
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
                Status.RxTransformed = ((header & 1) != 0);

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

        protected void OnDisconnect()
        {
            //if (closing)
            //{
                link.OnDisconnect(this);
            //}
            /*
            else
            {
                CloseInternal();

                if (Polarity == true)
                {
                    //var client = (TcpClientBase)link;
                }
                else
                {
                    var server = (TcpServerBase)link;
                    server.OnInstantDisconnect(this);
                }
            }
            */
        }

        private void BeginSend(Event e)
        {
            if (e.GetTypeId() != (int)SocketLinkEventType.KeepaliveEvent)
            {
                Status.HasSent = true;
            }

            e.Serialize(sendBuffer);

            uint header = 0;
            if (BufferTransform != null && Status.TxTransformReady && e._Transform)
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
                    Status.Sending = false;
                    return;
                }

                e = sendQueue.Dequeue();
            }

            BeginSend(e);
        }

        private void ProcessEvent(Event e)
        {
            switch (e.GetTypeId())
            {
                case (int)SocketLinkEventType.KeepaliveEvent:
                    break;
                case (int)SocketLinkEventType.HandshakeReq:
                    {
                        var req = (HandshakeReq)e;
                        byte[] response = BufferTransform.Handshake(req.Data);
                        Send(new HandshakeResp {
                            _Transform = false,
                            Data = response
                        });
                    }
                    break;
                case (int)SocketLinkEventType.HandshakeResp:
                    {
                        var ack = new HandshakeAck { _Transform = false };
                        var resp = (HandshakeResp)e;
                        if (BufferTransform.FinalizeHandshake(resp.Data))
                        {
                            Status.RxTransformReady = true;
                            ack.Result = true;
                        }
                        else
                        {
                            // log
                        }
                        Send(ack);
                    }
                    break;
                case (int)SocketLinkEventType.HandshakeAck:
                    {
                        var ack = (HandshakeAck)e;

                        if (ack.Result)
                        {
                            Status.TxTransformReady = true;
                        }

                        if (Polarity == true)
                        {
                            var client = (TcpClientBase)link;
                            //client.SendSessionTokenReq(this);
                            client.Session = this;
                        }
                        //
                        Hub.Post(new LinkSessionConnected {
                            LinkName = link.Name,
                            Result = true,
                            Context = this
                        });
                    }
                    break;
                /*
                case (int)SocketLinkEventType.SessionTokenReq:
                    {
                        if (Polarity == false)
                        {
                            var server = (TcpServerBase)link;
                            server.OnSessionTokenReq(this, (SessionTokenReq)e);
                        }
                    }
                    break;
                case (int)SocketLinkEventType.SessionTokenResp:
                    {
                        if (Polarity == true)
                        {
                            var client = (TcpClientBase)link;
                            client.OnSessionTokenResp(this, (SessionTokenResp)e);
                        }
                    }
                    break;
                */
                default:
                    Hub.Post(e);
                    break;
            }
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

    // Status flags
    public struct SessionStatus
    {
        private enum BitIndex
        {
            Closing,
            Sending,
            RxBeginning,
            RxTransformed,
            RxTransformReady,
            TxTransformReady,
            HasReceived,
            HasSent
        }

        private volatile int status;

        public bool Closing
        {
            get { return Get(BitIndex.Closing); }
            set { Set(BitIndex.Closing, value); }
        }

        public bool Sending
        {
            get { return Get(BitIndex.Sending); }
            set { Set(BitIndex.Sending, value); }
        }

        public bool RxBeginning
        {
            get { return Get(BitIndex.RxBeginning); }
            set { Set(BitIndex.RxBeginning, value); }
        }

        public bool RxTransformed
        {
            get { return Get(BitIndex.RxTransformed); }
            set { Set(BitIndex.RxTransformed, value); }
        }

        public bool RxTransformReady
        {
            get { return Get(BitIndex.RxTransformReady); }
            set { Set(BitIndex.RxTransformReady, value); }
        }

        public bool TxTransformReady
        {
            get { return Get(BitIndex.TxTransformReady); }
            set { Set(BitIndex.TxTransformReady, value); }
        }

        public bool HasReceived
        {
            get { return Get(BitIndex.HasReceived); }
            set { Set(BitIndex.HasReceived, value); }
        }

        public bool HasSent
        {
            get { return Get(BitIndex.HasSent); }
            set { Set(BitIndex.HasSent, value); }
        }

        public void Reset()
        {
            status = 0;
        }

        private bool Get(BitIndex index)
        {
            return ((status & (1 << (int)index)) != 0);
        }

        private void Set(BitIndex index, bool value)
        {
            if (value)
            {
                status |= (1 << (int)index);
            }
            else
            {
                status &= ~(1 << (int)index);
            }
        }
    }
}
