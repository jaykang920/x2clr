// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using x2.Events;
using x2.Flows;
using x2.Queues;

namespace x2.Links
{
    public abstract class TcpLink : Link
    {
        protected Socket socket;

        protected TcpLink(string name) : base(name) { }

        public override void Close()
        {
            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                // Setting socket as null here causes NullReferenceException
                // later trying to complete asynchronous jobs.
            }
        }

        public void OnReceive(IAsyncResult asyncResult)
        {
            AsyncRxState asyncState = (AsyncRxState)asyncResult.AsyncState;
            Buffer buffer = asyncState.Buffer;
            Session session = asyncState.Session;
            try
            {
                int numBytes = session.Socket.EndReceive(asyncResult);

                if (numBytes > 0)
                {
                    buffer.Stretch(numBytes);

                    if (asyncState.beginning)
                    {
                        buffer.Rewind();
                        int payloadLength;
                        int numLengthBytes = buffer.ReadUInt29(out payloadLength);
                        buffer.Shrink(numLengthBytes);
                        asyncState.length = payloadLength;
                    }

                    if (buffer.Length < asyncState.length)
                    {
                        session.BeginReceive(this, false);
                        return;
                    }

                    // end sentinel check
                    if (buffer[asyncState.length - 1] != sentinel)
                    {
                        // protocol format error
                    }

                    // pre-process
                    buffer.MarkToRead(asyncState.length);

                    int typeId;
                    buffer.ReadUInt29(out typeId);
                    Event e = Event.Create(typeId);
                    if (e == null)
                    {
                        // error
                    }
                    else
                    {
                        e.Load(buffer);
                        e.SessionHandle = session.Socket.Handle;

                        // Post up the retrieved event to the hubs to which this
                        // link is attached.
                        if (IsSelfPublishingEnabled)
                        {
                            Publish(e);
                        }
                        else
                        {
                            PublishAway(e);
                        }
                    }

                    buffer.Trim();
                    session.BeginReceive(this, true);
                }
                else
                {
                    // connection reset by peer
                    LinkSessionDisconnected e = new LinkSessionDisconnected();
                    e.LinkName = Name;
                    e.Context = session;
                    Publish(e);
                }
            }
            catch (SocketException)
            { // socket error
                LinkSessionDisconnected e = new LinkSessionDisconnected();
                e.LinkName = Name;
                e.Context = session;
                Publish(e);
            }
            catch (ObjectDisposedException)
            { // socket closed
                LinkSessionDisconnected e = new LinkSessionDisconnected();
                e.LinkName = Name;
                e.Context = session;
                Publish(e);
            }
        }

        public void OnSend(IAsyncResult asyncResult)
        {
            AsyncState asyncState = (AsyncState)asyncResult.AsyncState;
            try
            {
                int numBytes = asyncState.Session.Socket.EndSend(asyncResult);
            }
            catch (SocketException)
            { // socket error
                LinkSessionDisconnected e = new LinkSessionDisconnected();
                e.LinkName = Name;
                e.Context = asyncState.Session;
                Publish(e);
            }
            catch (ObjectDisposedException)
            { // socket closed
                LinkSessionDisconnected e = new LinkSessionDisconnected();
                e.LinkName = Name;
                e.Context = asyncState.Session;
                Publish(e);
            }
        }

        /*
        protected override void SetUp()
        {
            base.SetUp();

            Session.receiveCallback = new AsyncCallback(this.OnReceive);
            Session.sendCallback = new AsyncCallback(this.OnSend);
        }
         */

        protected override void TearDown()
        {
            Close();

            base.TearDown();
        }

        public new class Session : Link.Session
        {
            public static AsyncCallback receiveCallback;
            public static AsyncCallback sendCallback;

            private readonly Socket socket;
            AsyncRxState receiveState;

            public Socket Socket { get { return socket; } }

            public Session(Socket socket)
                : base(socket.Handle)
            {
                this.socket = socket;
                receiveState = new AsyncRxState();
                receiveState.Session = this;
                receiveState.Buffer = new Buffer(12);
            }

            public void BeginReceive(TcpLink link, bool beginning)
            {
                receiveState.beginning = beginning;
                receiveState.ArraySegments.Clear();
                receiveState.Buffer.ListAvailableSegments(receiveState.ArraySegments);
                try
                {
                    socket.BeginReceive(receiveState.ArraySegments, SocketFlags.None, link.OnReceive, receiveState);
                }
                catch (SocketException)
                { // socket error
                    LinkSessionDisconnected e = new LinkSessionDisconnected();
                    e.LinkName = link.Name;
                    e.Context = receiveState.Session;
                    link.Publish(e);
                }
                catch (ObjectDisposedException)
                { // socket closed
                    LinkSessionDisconnected e = new LinkSessionDisconnected();
                    e.LinkName = link.Name;
                    e.Context = receiveState.Session;
                    link.Publish(e);
                }
            }

            public void BeginSend(TcpLink link, Buffer buffer)
            {
                // pre-process
                buffer.Write(sentinel);
                AsyncTxState asyncState = new AsyncTxState();
                asyncState.Session = this;
                asyncState.Buffer = buffer;

                int numLengthBytes = Buffer.WriteUInt29(asyncState.lengthBytes, buffer.Length);
                asyncState.length = buffer.Length + numLengthBytes;
                asyncState.ArraySegments.Add(new ArraySegment<byte>(asyncState.lengthBytes, 0, numLengthBytes));

                buffer.ListOccupiedSegments(asyncState.ArraySegments);
                try
                {
                    socket.BeginSend(asyncState.ArraySegments, SocketFlags.None, link.OnSend, asyncState);
                }
                catch (SocketException)
                { // socket error
                    LinkSessionDisconnected e = new LinkSessionDisconnected();
                    e.LinkName = link.Name;
                    e.Context = asyncState.Session;
                    link.Publish(e);
                }
                catch (ObjectDisposedException)
                { // socket closed
                    LinkSessionDisconnected e = new LinkSessionDisconnected();
                    e.LinkName = link.Name;
                    e.Context = asyncState.Session;
                    link.Publish(e);
                }
            }

            public override void Send(Link link, Event e)
            {
                var buffer = new Buffer(12);
                e.Serialize(buffer);
                BeginSend((TcpLink)link, buffer);
            }

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder
                    .Append("TcpLink.Session { Handle=")
                    .Append(socket.Handle)
                    .Append(" Local=")
                    .Append(socket.LocalEndPoint)
                    .Append(" Remote=")
                    .Append(socket.RemoteEndPoint)
                    .Append(" }");
                return stringBuilder.ToString();
            }
        }

        protected class AsyncState
        {
            public Session Session;
            public Buffer Buffer;
            public IList<ArraySegment<byte>> ArraySegments;
            public int length;

            public AsyncState()
            {
                ArraySegments = new List<ArraySegment<byte>>();
            }
        }

        protected class AsyncRxState : AsyncState
        {
            public bool beginning = true;
        }

        protected class AsyncTxState : AsyncState
        {
            public byte[] lengthBytes = new byte[4];
        }

        public class ConnectionTag
        {
            public string Tag { get; set; }
            public EndPoint EndPoint { get; set; }
        }
    }
}
