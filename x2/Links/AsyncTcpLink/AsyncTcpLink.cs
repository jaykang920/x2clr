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

namespace x2.Links.AsyncTcpLink
{
    public abstract class AsyncTcpLinkCase : LinkCase
    {
        protected Socket socket;

        protected AsyncTcpLinkCase(string name) : base(name) { }

        public void OnCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    OnReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    OnSend(e);
                    break;
                default:
                    throw new Exception("Invalid async operation completed");
            }
        }

        public void OnReceive(SocketAsyncEventArgs e)
        {
            var session = e.UserToken as AsyncTcpLinkSession;
            Buffer buffer = session.ReceiveBuffer;

            if (e.SocketError == SocketError.Success)
            {
                int bytesTransferred = e.BytesTransferred;

                if (bytesTransferred > 0)
                {
                    buffer.Stretch(bytesTransferred);

                    Log.Trace("{0} AsyncTcpLink.OnReceive buffer length {1} after stretching", session.Handle, buffer.Length);

                    if (session.Beginning)
                    {
                        buffer.Rewind();
                        int payloadLength;
                        int numLengthBytes = buffer.ReadUInt29(out payloadLength);
                        buffer.Shrink(numLengthBytes);
                        session.Length = payloadLength;

                        Log.Trace("{0} TcpLink.OnReceive beginning length {1} payload length {2} after stretching", session.Handle, buffer.Length, payloadLength);
                    }

                    // Handle split packets.
                    if (buffer.Length < session.Length)
                    {
                        Log.Debug("{0} TcpLink.OnReceive split packet #1 {1} of {2} byte(s)", session.Handle, buffer.Length, session.Length);
                        session.Beginning = false;
                        session.ReceiveAsync();
                        return;
                    }

                    while (true)
                    {
                        // pre-process
                        buffer.MarkToRead(session.Length);

                        int typeId;
                        buffer.ReadUInt29(out typeId);

                        Event retrieved = Event.Create(typeId);
                        if (retrieved == null)
                        {
                            Log.Error("{0} Unknown event type id {1}", session.Handle, typeId);
                        }
                        else
                        {
                            retrieved.Load(buffer);

                            // TODO: to be moved into pre-post handler chain
                            retrieved.SessionHandle = session.Socket.Handle;

                            if (Preprocessor != null)
                            {
                                Preprocessor(retrieved, session);
                            }

                            Log.Info("{0} Received {1}", session.Handle, retrieved.ToString());

                            // Post up the retrieved event to the hub which this
                            // link is attached to.
                            Flow.Publish(retrieved);
                        }

                        buffer.Trim();

                        if (buffer.IsEmpty)
                        {
                            Log.Trace("{0} TcpLink.OnReceive completed exactly", session.Handle);
                            break;
                        }

                        Log.Trace("{0} TcpLink.OnReceive continuing to next event with buffer length {1}", session.Handle, buffer.Length);

                        int payloadLength;
                        int numLengthBytes = buffer.ReadUInt29(out payloadLength);

                        Log.Trace("{0} TcpLink.OnReceive next payload length {1} numLengthByte {2}", session.Handle, payloadLength, numLengthBytes);

                        buffer.Shrink(numLengthBytes);
                        session.Length = payloadLength;

                        Log.Trace("{0} TcpLink.OnReceive re-beginning length {1} payload length {2} after stretching", session.Handle, buffer.Length, payloadLength);

                        if (buffer.Length < session.Length)
                        {
                            Log.Debug("{0} TcpLink.OnReceive split packet #2 {1} of {2} byte(s)", session.Handle, buffer.Length, session.Length);
                            session.Beginning = false;
                            session.ReceiveAsync();
                            return;
                        }
                    }

                    session.Beginning = true;
                    session.ReceiveAsync();
                }
                else
                {
                    // reset by peer
                }
            }
            else
            {
                // discon
            }
        }

        public void OnSend(SocketAsyncEventArgs e)
        {
            var session = e.UserToken as AsyncTcpLinkSession;
            Buffer buffer = session.ReceiveBuffer;

            if (e.SocketError == SocketError.Success)
            {
                int bytesTransferred = e.BytesTransferred;

                if (bytesTransferred < session.Length)
                {
                    // Try to send the rest.
                    session.SendBuffer.Shrink(bytesTransferred);
                    session.Length = session.SendBuffer.Length;
                    e.BufferList.Clear();
                    session.SendBuffer.ListOccupiedSegments(e.BufferList);

                    bool pending = socket.SendAsync(e);
                    if (!pending)
                    {
                        OnSend(e);
                    }

                    return;
                }

                session.TrySendNext();
            }
            else
            {
                // discon
            }
        }

        protected override void TearDown()
        {
            Close();

            base.TearDown();
        }
    }

    public class AsyncTcpLinkSession : LinkSession
    {
        private AsyncTcpLinkCase link;  // associated LinkCase
        private Socket socket;          // underlying socket

        private Queue<Event> sendQueue;

        private SocketAsyncEventArgs receiveEventArgs;
        private SocketAsyncEventArgs sendEventArgs;

        private Buffer receiveBuffer;
        private Buffer sendBuffer;

        // Operation context details
        public int Length { get; set; }      // common
        public bool Beginning { get; set; }  // rx
        private volatile bool sending;       // tx
        private byte[] lengthBytes = new byte[4];  // tx

        public Socket Socket { get { return socket; } }

        public Buffer ReceiveBuffer { get { return receiveBuffer; } }
        public Buffer SendBuffer { get { return sendBuffer; } }

        public AsyncTcpLinkSession(AsyncTcpLinkCase link, Socket socket)
            : base(socket.Handle)
        {
            this.link = link;
            this.socket = socket;

            sendQueue = new Queue<Event>();

            receiveEventArgs = new SocketAsyncEventArgs();
            sendEventArgs = new SocketAsyncEventArgs();

            receiveEventArgs.Completed += link.OnCompleted;
            sendEventArgs.Completed += link.OnCompleted;

            receiveEventArgs.BufferList = new List<ArraySegment<byte>>();
            sendEventArgs.BufferList = new List<ArraySegment<byte>>();

            receiveEventArgs.UserToken = this;
            sendEventArgs.UserToken = this;

            receiveBuffer = new Buffer(12);
            sendBuffer = new Buffer(12);
        }

        public override void Close()
        {
        }

        public void Receive()
        {
            Beginning = true;
            ReceiveAsync();
        }

        public override void Send(Event e)
        {
            if (sending)
            {
                lock (sendQueue)
                {
                    sendQueue.Enqueue(e);
                }
            }
            else
            {
                SendAsync(e);
            }
        }

        /// <summary>
        /// Initiates a new asynchronous receive operation on this session.
        /// </summary>
        public void ReceiveAsync()
        {
            receiveEventArgs.BufferList.Clear();
            receiveBuffer.ListAvailableSegments(receiveEventArgs.BufferList);

            bool pending = socket.ReceiveAsync(receiveEventArgs);

            if (!pending)
            {
                link.OnReceive(receiveEventArgs);
            }
        }

        /// <summary>
        /// Initiates a new asynchronous send operation on this session.
        /// </summary>
        public void SendAsync()
        {
            sendEventArgs.BufferList.Clear();

            bool pending = socket.SendAsync(sendEventArgs);

            if (!pending)
            {
                link.OnSend(sendEventArgs);
            }
        }

        public void SendAsync(Event e)
        {
            e.Serialize(sendBuffer);
            int numLengthBytes = Buffer.WriteUInt29(lengthBytes, sendBuffer.Length);
            Length = sendBuffer.Length + numLengthBytes;
            sendEventArgs.BufferList.Clear();
            sendEventArgs.BufferList.Add(new ArraySegment<byte>(lengthBytes, 0, numLengthBytes));

            sendBuffer.ListOccupiedSegments(sendEventArgs.BufferList);

            sending = true;

            bool pending = socket.SendAsync(sendEventArgs);

            if (!pending)
            {
                link.OnSend(sendEventArgs);
            }
        }

        public void TrySendNext()
        {
            Event e;
            lock (sendQueue)
            {
                if (sendQueue.Count == 0)
                {
                    return;
                }

                e = sendQueue.Dequeue();
            }
            
            SendAsync(e);
        }
    }
}
