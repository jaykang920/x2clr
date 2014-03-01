// Copyright (c) 2013 Jae-jun Kang
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
        protected volatile bool sending;              // tx
        protected byte[] lengthBytes = new byte[4];   // tx

        /// <summary>
        /// Gets the underlying socket object.
        /// </summary>
        public Socket Socket { get { return socket; } }

        public bool Polarity { get; set; }

        public x2.Flows.Timer.Token HeartbeatTimeoutToken { get; set; }

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
            Diag.AddBytesReceived(bytesTransferred);

            Log.Trace("{0} {1} received {2} byte(s)",
                link.Name, Handle, bytesTransferred);

            recvBuffer.Stretch(bytesTransferred);

            if (beginning)
            {
                recvBuffer.Rewind();
                int payloadLength;
                int numLengthBytes = recvBuffer.ReadUInt29(out payloadLength);
                recvBuffer.Shrink(numLengthBytes);
                lengthToReceive = payloadLength;
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

                if (BufferTransform != null)
                {
                    try
                    {
                        BufferTransform.InverseTransform(recvBuffer, lengthToReceive);
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} {1} buffer transform error: {2}", link.Name, Handle, e.Message);
                        recvBuffer.Shrink(lengthToReceive);
                        break;
                    }
                    recvBuffer.Rewind();
                }

                int typeId;
                recvBuffer.ReadUInt29(out typeId);

                // Heartbeat
                if (typeId == HeartbeatEvent.TypeId)
                {
                    var heartbeat = new HeartbeatEvent();
                    heartbeat.Load(recvBuffer);

                    if (!Polarity)
                    {
                        // Heartbeat feedback
                        Send(heartbeat);

                        Log.Debug("{0} {1} heartbeat {2}", link.Name, Handle, heartbeat.Timestamp);
                    }

                    if (link.HeartbeatEventHandler != null)
                    {
                        link.HeartbeatEventHandler(this, heartbeat);
                    }
                }
                else
                {
                    var retrieved = Event.Create(typeId);
                    if (retrieved == null)
                    {
                        Log.Error("{0} {1} unknown event type id {2}", link.Name, Handle, typeId);
                    }
                    else
                    {
                        retrieved.Load(recvBuffer);
                        retrieved.SessionHandle = Handle;
                        if (link.Preprocessor != null)
                        {
                            link.Preprocessor(retrieved, this);
                        }

                        Log.Debug("{0} {1} received event {2}", link.Name, Handle, retrieved);

                        link.Flow.Publish(retrieved);
                    }
                }

                recvBuffer.Trim();
                if (recvBuffer.IsEmpty)
                {
                    break;
                }

                int payloadLength;
                int numLengthBytes = recvBuffer.ReadUInt29(out payloadLength);
                recvBuffer.Shrink(numLengthBytes);
                lengthToReceive = payloadLength;

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
            e.Serialize(sendBuffer);

            if (BufferTransform != null)
            {
                BufferTransform.Transform(sendBuffer, sendBuffer.Length);
            }

            int numLengthBytes = Buffer.WriteUInt29(lengthBytes, sendBuffer.Length);
            lengthToSend = sendBuffer.Length + numLengthBytes;

            sendBufferList.Clear();
            sendBufferList.Add(new ArraySegment<byte>(lengthBytes, 0, numLengthBytes));
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
    }
}
