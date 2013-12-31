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

namespace x2.Links
{
    public abstract class TcpLink : Link
    {
        protected Socket socket;

        protected TcpLink(string name) : base(name) { }

        public override void Close()
        {
            if (socket != null)
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }

                // Setting socket as null here causes NullReferenceException
                // later trying to complete asynchronous jobs.
                socket = null;
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

                Log.Debug("{0} TcpLink.OnReceive Socket.EndReceive {1} byte(s)", session.Handle, numBytes);

                if (numBytes > 0)
                {
                    buffer.Stretch(numBytes);

                    Log.Trace("{0} TcpLink.OnReceive buffer length {1} after stretching", session.Handle, buffer.Length);

                    if (asyncState.beginning)
                    {
                        buffer.Rewind();
                        int payloadLength;
                        int numLengthBytes = buffer.ReadUInt29(out payloadLength);
                        buffer.Shrink(numLengthBytes);
                        asyncState.length = payloadLength;

                        Log.Trace("{0} TcpLink.OnReceive beginning length {1} payload length {2} after stretching", session.Handle, buffer.Length, payloadLength);
                    }

                    // Handle split packets.
                    if (buffer.Length < asyncState.length)
                    {
                        Log.Debug("{0} TcpLink.OnReceive split packet #1 {1} of {2} byte(s)", session.Handle, buffer.Length, asyncState.length);
                        session.BeginReceive(this, false);
                        return;
                    }

                    while (true)
                    {
                        // pre-process
                        buffer.MarkToRead(asyncState.length);

                        int typeId;
                        buffer.ReadUInt29(out typeId);

                        Event e = Event.Create(typeId);
                        if (e == null)
                        {
                            Log.Error("{0} Unknown event type id {1}", session.Handle, typeId);
                        }
                        else
                        {
                            e.Load(buffer);

                            // TODO: to be moved into pre-post handler chain
                            e.SessionHandle = session.Socket.Handle;

                            if (PrePostHandler != null)
                            {
                                PrePostHandler(e, session);
                            }

                            Log.Info("{0} Received {1}", session.Handle, e.ToString());

                            // Post up the retrieved event to the hub which this
                            // link is attached to.
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
                        asyncState.length = payloadLength;

                        Log.Trace("{0} TcpLink.OnReceive re-beginning length {1} payload length {2} after stretching", session.Handle, buffer.Length, payloadLength);

                        if (buffer.Length < asyncState.length)
                        {
                            Log.Debug("{0} TcpLink.OnReceive split packet #2 {1} of {2} byte(s)", session.Handle, buffer.Length, asyncState.length);
                            session.BeginReceive(this, false);
                            return;
                        }
                    }

                    session.BeginReceive(this, true);
                }
                else
                {
                    // Connection reset by peer.
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
            {
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

                Log.Debug("{0} TcpLink.OnSend Socket.EndSend {1} of {2} byte(s) completed", asyncState.Session.Handle, numBytes, asyncState.length);

                if (numBytes < asyncState.length)
                {
                    Log.Warn("{0} TcpLink.OnSend trying to send {2} more byte(s)", asyncState.Session.Handle, asyncState.length - numBytes);

                    // Try to send the rest
                    asyncState.Buffer.Shrink(numBytes);
                    asyncState.length = asyncState.Buffer.Length;
                    asyncState.Buffer.ListOccupiedSegments(asyncState.ArraySegments);
                    socket.BeginSend(asyncState.ArraySegments, SocketFlags.None, OnSend, asyncState);
                    return;
                }

                asyncState.Session.ReadyToSend.Set();
            }
            catch (SocketException)
            { // socket error
                LinkSessionDisconnected e = new LinkSessionDisconnected();
                e.LinkName = Name;
                e.Context = asyncState.Session;
                Publish(e);
            }
            catch (ObjectDisposedException)
            {
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
            private AsyncRxState receiveState;
            private volatile bool closing;

            public AutoResetEvent ReadyToSend { get; private set; }

            public Socket Socket { get { return socket; } }

            public string RemoteAddress
            {
                get
                {
                    if (!socket.Connected)
                    {
                        return null;
                    }
                    var endpoint = socket.RemoteEndPoint as IPEndPoint;
                    return endpoint.Address.ToString();
                }
            }

            public Session(Socket socket)
                : base(socket.Handle)
            {
                this.socket = socket;
                
                receiveState = new AsyncRxState();
                receiveState.Session = this;
                receiveState.Buffer = new Buffer(12);

                ReadyToSend = new AutoResetEvent(true);
            }

            public void BeginReceive(TcpLink link, bool beginning)
            {
                receiveState.beginning = beginning;
                receiveState.ArraySegments.Clear();
                receiveState.Buffer.ListAvailableSegments(receiveState.ArraySegments);
                try
                {
                    int bufferLength = 0;
                    for (int i = 0; i < receiveState.ArraySegments.Count; ++i)
                    {
                        bufferLength += receiveState.ArraySegments[i].Count;
                    }
                    Log.Debug("{0} TcpLink.Session.BeginReceive {1} byte(s) in {2} block(s) beginning={3}", Handle, bufferLength, receiveState.ArraySegments.Count, beginning);

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
                {
                    LinkSessionDisconnected e = new LinkSessionDisconnected();
                    e.LinkName = link.Name;
                    e.Context = receiveState.Session;
                    link.Publish(e);
                }
            }

            public void BeginSend(TcpLink link, Buffer buffer)
            {
                //buffer.Write((byte)0x66);
                AsyncTxState asyncState = new AsyncTxState();
                asyncState.Session = this;
                asyncState.Buffer = buffer;

                int numLengthBytes = Buffer.WriteUInt29(asyncState.lengthBytes, buffer.Length);
                asyncState.length = buffer.Length + numLengthBytes;
                asyncState.ArraySegments.Add(new ArraySegment<byte>(asyncState.lengthBytes, 0, numLengthBytes));

                buffer.ListOccupiedSegments(asyncState.ArraySegments);
                try
                {
                    Log.Debug("{0} TcpLink.Session.BeginSend {1} ({2} including length) byte(s)", Handle, buffer.Length, asyncState.length);

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
                {
                    LinkSessionDisconnected e = new LinkSessionDisconnected();
                    e.LinkName = link.Name;
                    e.Context = asyncState.Session;
                    link.Publish(e);
                }
            }

            public override void Close()
            {
                if (closing)
                {
                    return;
                }

                closing = true;
                ReadyToSend.Set();  // let any pending writer thread continue

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            public override void Send(Link link, Event e)
            {
                ReadyToSend.WaitOne();
                if (closing)
                {
                    //ReadyToSend.Close();
                    return;
                }

                var buffer = new Buffer(12);
                e.Serialize(buffer);
                BeginSend((TcpLink)link, buffer);

                Log.Info("{0} Sent {1}", Handle, e.ToString());
            }

            public override string ToString()
            {
                try
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
                catch (ObjectDisposedException)
                {
                    return "TcpLink.Session (Closed)";
                }
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
