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

namespace x2.Links.TcpLink2
{
    /// <summary>
    /// Common abstract base class for the TCP/IP link pair (client and server)
    /// based on the Begin/End pattern.
    /// </summary>
    public abstract class TcpLink2 : Link
    {
        protected Socket socket;  // underlying socket

        protected TcpLink2(string name)
            : base(name)
        {
        }

        protected override void TearDown()
        {
            Close();
        }

        /// <summary>
        /// Represents a TcpLink communication session.
        /// </summary>
        public class Session : LinkSession
        {
            private TcpLink2 link;  // associated Link
            private Socket socket;  // underlying socket

            private Queue<Event> sendQueue;

            private Buffer recvBuffer;
            private Buffer sendBuffer;

            private IList<ArraySegment<byte>> recvBufferList;
            private IList<ArraySegment<byte>> sendBufferList;

            // Operation context details
            private int length;                         // common
            private bool beginning;                     // rx
            private volatile bool sending;              // tx
            private byte[] lengthBytes = new byte[4];   // tx

            public Socket Socket { get { return socket; } }

            public Session(TcpLink2 link, Socket socket)
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

                //socket = null;
            }

            /// <summary>
            /// Tries to send the specified event through this link session.
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

                SendAsync(e);
            }

            internal void BeginReceive(bool beginning)
            {
                this.beginning = beginning;

                recvBufferList.Clear();
                recvBuffer.ListAvailableSegments(recvBufferList);

                socket.BeginReceive(recvBufferList, SocketFlags.None, OnReceive, null);
            }

            // Asynchronous callback for BeginReceive
            private void OnReceive(IAsyncResult asyncResult)
            {
                try
                {
                    int bytesTransferred = socket.EndReceive(asyncResult);

                    if (bytesTransferred > 0)
                    {
                        recvBuffer.Stretch(bytesTransferred);

                        Log.Trace("{0} TcpLink.OnReceive buffer length {1} after stretching", Handle, recvBuffer.Length);

                        if (beginning)
                        {
                            recvBuffer.Rewind();
                            int payloadLength;
                            int numLengthBytes = recvBuffer.ReadUInt29(out payloadLength);
                            recvBuffer.Shrink(numLengthBytes);
                            length = payloadLength;

                            Log.Trace("{0} TcpLink.OnReceive beginning length {1} payload length {2} after stretching", Handle, recvBuffer.Length, payloadLength);
                        }

                        // Handle split packets.
                        if (recvBuffer.Length < length)
                        {
                            Log.Debug("{0} TcpLink.OnReceive split packet #1 {1} of {2} byte(s)", Handle, recvBuffer.Length, length);
                            BeginReceive(false);
                            return;
                        }

                        while (true)
                        {
                            // pre-process
                            recvBuffer.MarkToRead(length);

                            int typeId;
                            recvBuffer.ReadUInt29(out typeId);

                            Event retrieved = Event.Create(typeId);
                            if (retrieved == null)
                            {
                                Log.Error("{0} Unknown event type id {1}", Handle, typeId);
                            }
                            else
                            {
                                retrieved.Load(recvBuffer);

                                // TODO: to be moved into pre-post handler chain
                                retrieved.SessionHandle = Handle;

                                if (link.Preprocessor != null)
                                {
                                    link.Preprocessor(retrieved, this);
                                }

                                Log.Info("{0} Received {1}", Handle, retrieved.ToString());

                                // Post up the retrieved event to the hub which this
                                // link is attached to.
                                link.Flow.Publish(retrieved);
                            }

                            recvBuffer.Trim();

                            if (recvBuffer.IsEmpty)
                            {
                                Log.Trace("{0} TcpLink.OnReceive completed exactly", Handle);
                                break;
                            }

                            Log.Trace("{0} TcpLink.OnReceive continuing to next event with buffer length {1}", Handle, recvBuffer.Length);

                            int payloadLength;
                            int numLengthBytes = recvBuffer.ReadUInt29(out payloadLength);

                            Log.Trace("{0} TcpLink.OnReceive next payload length {1} numLengthByte {2}", Handle, payloadLength, numLengthBytes);

                            recvBuffer.Shrink(numLengthBytes);
                            length = payloadLength;

                            Log.Trace("{0} TcpLink.OnReceive re-beginning length {1} payload length {2} after stretching", Handle, recvBuffer.Length, payloadLength);

                            if (recvBuffer.Length < length)
                            {
                                Log.Debug("{0} TcpLink.OnReceive split packet #2 {1} of {2} byte(s)", Handle, recvBuffer.Length, length);
                                BeginReceive(false);
                                return;
                            }
                        }

                        BeginReceive(true);
                        return;
                    }

                    // (bytesTransferred == 0) implies a graceful shutdown
                    link.Flow.Publish(new LinkSessionDisconnected {
                        LinkName = link.Name,
                        Context = this
                    });
                }
                catch (Exception e)
                {
                    link.Flow.Publish(new LinkSessionDisconnected {
                        LinkName = link.Name,
                        Context = this
                    });

                    if (e is SocketException)
                    {
                        var se = (SocketException)e;
                        Log.Info("TcpLink.OnReceive SocketException {0} {1}", se.ErrorCode, e.Message);
                    }
                    else
                    {
                        Log.Info("TcpLink.OnReceive Exception {0}", e.Message);
                    }
                }
            }

            // Asynchronous callback for BeginSend
            private void OnSend(IAsyncResult asyncResult)
            {
                try
                {
                    int bytesTransferred = socket.EndSend(asyncResult);

                    if (bytesTransferred < length)
                    {
                        // Try to send the rest.
                        sendBuffer.Shrink(bytesTransferred);
                        length = sendBuffer.Length;

                        sendBufferList.Clear();
                        sendBuffer.ListOccupiedSegments(sendBufferList);

                        socket.BeginSend(sendBufferList, SocketFlags.None, OnSend, null);
                        return;
                    }

                    sendBuffer.Trim();

                    TrySendNext();
                }
                catch (Exception e)
                {
                    link.Flow.Publish(new LinkSessionDisconnected {
                        LinkName = link.Name,
                        Context = this
                    });

                    if (e is SocketException)
                    {
                        var se = (SocketException)e;
                        Log.Info("TcpLink.OnSend SocketException {0} {1}", se.ErrorCode, e.Message);
                    }
                    else
                    {
                        Log.Info("TcpLink.OnSend Exception {0}", e.Message);
                    }
                }
            }

            private void SendAsync(Event e)
            {
                e.Serialize(sendBuffer);
                int numLengthBytes = Buffer.WriteUInt29(lengthBytes, sendBuffer.Length);
                length = sendBuffer.Length + numLengthBytes;

                sendBufferList.Clear();
                sendBufferList.Add(new ArraySegment<byte>(lengthBytes, 0, numLengthBytes));
                sendBuffer.ListOccupiedSegments(sendBufferList);

                socket.BeginSend(sendBufferList, SocketFlags.None, OnSend, null);
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

                SendAsync(e);
            }
        }
    }
}
