// Copyright (c) 2013 Jae-jun Kang
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
                Log.Warn("{0} accept error {1}", Name, e.SocketError);
            }
        }
    }

    public class AsyncTcpServerFlow : SingleThreadedFlow
    {
        protected AsyncTcpServer link;

        public string Name { get; private set; }

        public int Backlog
        {
            get { return link.Backlog; }
            set { link.Backlog = value; }
        }
        public bool Listening
        {
            get { return link.Listening; }
        }

        public Action<Event, LinkSession> Preprocessor { get; set; }

        public AsyncTcpServerFlow(string name)
        {
            link = new AsyncTcpServer(name);
            Add(link);

            Name = name;
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
        }

        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            OnSessionDisconnected(e);
        }

        protected override void OnStop()
        {
            base.OnStop();

            Close();
        }
    }
}
