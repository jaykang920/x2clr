// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using x2.Events;
using x2.Flows;

namespace x2.Links.SocketLink
{
    /// <summary>
    /// TCP/IP server link based on the enhanced SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncTcpServer : TcpServerBase
    {
        private SocketAsyncEventArgs acceptEventArgs;

        public AsyncTcpServer(string name)
            : base(name)
        {
        }

        protected override void AcceptImpl()
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += OnAcceptCompleted;
            }
            else
            {
                acceptEventArgs.AcceptSocket = null;
            }

            bool pending = socket.AcceptAsync(acceptEventArgs);
            if (!pending)
            {
                OnAccept(acceptEventArgs);
            }
        }

        // Completed event handler for AcceptAsync
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnAccept(e);
        }

        // Completion callback for AcceptAsync
        private void OnAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                ((Diagnostics)Diag).IncrementConnectionCount();

                var clientSocket = e.AcceptSocket;

                // Adjust client socket options.
                clientSocket.NoDelay = NoDelay;

                Log.Info("{0} {1} accepted from {2}",
                    Name, clientSocket.Handle, clientSocket.RemoteEndPoint);

                var session = new AsyncTcpLinkSession(this, clientSocket);

                if (BufferTransform != null)
                {
                    session.BufferTransform = (IBufferTransform)BufferTransform.Clone();
                }

                Flow.Publish(new LinkSessionConnected {
                    LinkName = Name,
                    Result = true,
                    Context = session
                });

                session.BeginReceive(true);

                AcceptImpl();
            }
            else
            {
                if (e.SocketError == SocketError.OperationAborted)
                {
                    // Listening socket has been closed.
                }
                else
                {
                    Log.Error("{0} accept error {1}", Name, e.SocketError);
                }
            }
        }
    }

    public class AsyncTcpServerFlow : FrameBasedFlow
    {
        protected AsyncTcpServer link;

        private volatile bool closeOnHeartbeatFailure;

        protected x2.Flows.Timer Timer { get; private set; }

        public bool CloseOnHeartbeatFailure
        {
            get { return closeOnHeartbeatFailure; }
            set { closeOnHeartbeatFailure = value; }
        }
        public double HeartbeatTimeout { get; set; }  // in seconds

        public int Backlog
        {
            get { return link.Backlog; }
            set { link.Backlog = value; }
        }
        public bool Listening
        {
            get { return link.Listening; }
        }

        public Action<Event, LinkSession> Preprocessor
        {
            get { return link.Preprocessor; }
            set { link.Preprocessor = value; }
        }

        public AsyncTcpServerFlow(string name)
            : this(name, new AsyncTcpServer(name))
        {
        }

        public AsyncTcpServerFlow(string name, AsyncTcpServer link)
        {
            this.link = link;
            Add(link);

            link.HeartbeatEventHandler = OnHeartbeatEvent;

            this.name = name;

            Resolution = Time.TicksInSecond;  // 1-second frame resolution
            HeartbeatTimeout = 15;

            Timer = new x2.Flows.Timer(OnTimer);
        }

        public void Close()
        {
            link.Close();
        }

        public void Listen(int port)
        {
            link.Listen(port);
        }

        protected override void SetUp()
        {
            base.SetUp();

            Subscribe(new LinkSessionConnected { LinkName = Name }, OnLinkSessionConnected);
            Subscribe(new LinkSessionDisconnected { LinkName = Name }, OnLinkSessionDisconnected);
        }

        protected override void TearDown()
        {
            Unsubscribe(new LinkSessionDisconnected { LinkName = Name }, OnLinkSessionDisconnected);
            Unsubscribe(new LinkSessionConnected() { LinkName = Name }, OnLinkSessionConnected);

            base.TearDown();
        }

        protected virtual void OnSessionConnected(LinkSessionConnected e) { }

        protected virtual void OnSessionDisconnected(LinkSessionDisconnected e) { }

        private void OnLinkSessionConnected(LinkSessionConnected e)
        {
            OnSessionConnected(e);

            SocketLinkSession linkSession = (SocketLinkSession)e.Context;
            linkSession.HeartbeatTimeoutToken = Timer.Reserve(linkSession, HeartbeatTimeout);
        }

        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            SocketLinkSession linkSession = (SocketLinkSession)e.Context;
            Timer.Cancel(linkSession.HeartbeatTimeoutToken);

            OnSessionDisconnected(e);
        }

        protected override void OnStop()
        {
            base.OnStop();

            Close();
        }

        protected override void Start() { }
        protected override void Stop() { }

        protected override void Update()
        {
            Timer.Tick();
        }

        void OnTimer(object state)
        {
            if (state != null)
            {
                // heartbeat timeout
                var session = (SocketLinkSession)state;
                if (closeOnHeartbeatFailure)
                {
                    Log.Error("{0} {1} heartbeat timeout", Name, session.Handle);
                    session.Close();
                }
                else
                {
                    if (session.Socket == null || !session.Socket.Connected)
                    {
                        return;
                    }
                    Log.Warn("{0} {1} heartbeat timeout", Name, session.Handle);
                    session.HeartbeatTimeoutToken = Timer.Reserve(session, HeartbeatTimeout);
                }
            }
        }

        void OnHeartbeatEvent(SocketLinkSession session, HeartbeatEvent e)
        {
            Timer.Cancel(session.HeartbeatTimeoutToken);
            session.HeartbeatTimeoutToken = Timer.Reserve(session, HeartbeatTimeout);
        }
    }
}
