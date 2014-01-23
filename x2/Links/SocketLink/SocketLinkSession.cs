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
        protected int length;                         // common
        protected bool beginning;                     // rx
        protected volatile bool sending;              // tx
        protected byte[] lengthBytes = new byte[4];   // tx

        /// <summary>
        /// Gets the underlying socket object.
        /// </summary>
        public Socket Socket { get { return socket; } }

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
        }

        /// <summary>
        /// Closes the session.
        /// </summary>
        public override void Close()
        {
            if (socket == null) { return; }

            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            socket.Close();

            socket = null;
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
            recvBuffer.Stretch(bytesTransferred);

            Log.Trace("{0} {1} received {2} byte(s)",
                link.Name, Handle, bytesTransferred);

            if (beginning)
            {
                recvBuffer.Rewind();
                int payloadLength;
                int numLengthBytes = recvBuffer.ReadUInt29(out payloadLength);
                recvBuffer.Shrink(numLengthBytes);
                length = payloadLength;
            }

            // Handle split packets.
            if (recvBuffer.Length < length)
            {
                BeginReceive(false);
                return;
            }

            while (true)
            {
                recvBuffer.MarkToRead(length);

                int typeId;
                recvBuffer.ReadUInt29(out typeId);

                Event retrieved = Event.Create(typeId);
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

                    Log.Info("{0} {1} received event {2}", link.Name, Handle, retrieved);

                    link.Flow.Publish(retrieved);
                }

                recvBuffer.Trim();
                if (recvBuffer.IsEmpty)
                {
                    break;
                }

                int payloadLength;
                int numLengthBytes = recvBuffer.ReadUInt29(out payloadLength);
                recvBuffer.Shrink(numLengthBytes);
                length = payloadLength;

                if (recvBuffer.Length < length)
                {
                    BeginReceive(false);
                    return;
                }
            }

            BeginReceive(true);
        }

        protected void SendInternal(int bytesTransferred)
        {
            Log.Trace("{0} {1} sent {2}/{3} byte(s)",
                link.Name, Handle, bytesTransferred, length);

            if (bytesTransferred < length)
            {
                // Try to send the rest.
                sendBuffer.Shrink(bytesTransferred);
                length = sendBuffer.Length;

                sendBufferList.Clear();
                sendBuffer.ListOccupiedSegments(sendBufferList);

                SendImpl();
                return;
            }

            sendBuffer.Trim();

            TrySendNext();
        }

        private void BeginSend(Event e)
        {
            e.Serialize(sendBuffer);
            int numLengthBytes = Buffer.WriteUInt29(lengthBytes, sendBuffer.Length);
            length = sendBuffer.Length + numLengthBytes;

            sendBufferList.Clear();
            sendBufferList.Add(new ArraySegment<byte>(lengthBytes, 0, numLengthBytes));
            sendBuffer.ListOccupiedSegments(sendBufferList);

            SendImpl();

            Log.Info("{0} {1} sent event {2}", link.Name, Handle, e);
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
