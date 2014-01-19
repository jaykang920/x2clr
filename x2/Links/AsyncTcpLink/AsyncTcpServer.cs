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
    /// <summary>
    /// TCP/IP server link based on the enhanced SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncTcpServer : AsyncTcpLink
    {
        private int backlog;

        /// <summary>
        /// Gets or sets the maximum length of the pending connections queue.
        /// </summary>
        public int Backlog
        {
            get { return backlog; }
            set
            {
                if (socket != null)
                {
                    throw new InvalidOperationException();
                }
                backlog = value;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the server socket is ready.
        /// </summary>
        public bool Listening
        {
            get { return (socket != null && socket.IsBound ); }
        }

        public AsyncTcpServer(string name) : base(name)
        {
            backlog = Int32.MaxValue;
        }

        public override void Close()
        {
            if (socket == null) { return; }
            socket.Close();
            socket = null;
        }

        public void Listen(int port)
        {
            Listen(IPAddress.Any, port);
        }

        public void Listen(string ip, int port)
        {
            Listen(IPAddress.Parse(ip), port);
        }

        public void Listen(IPAddress ip, int port)
        {
            if (socket != null)
            {
                throw new InvalidOperationException();
            }

            try
            {
                socket = new Socket(ip.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                EndPoint endpoint = new IPEndPoint(ip, port);
                socket.Bind(endpoint);
                socket.Listen(backlog);

                Accept(null);

                Log.Info("AsyncTcpServer: listening on {0}", endpoint);
            }
            catch (Exception)
            {
                socket = null;
                throw;
            }
        }

        private void Accept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += OnAcceptCompleted;
            }
            else
            {
                // Clear the client socket before reusing the context object.
                e.AcceptSocket = null;
            }

            bool pending = socket.AcceptAsync(e);
            if (!pending)
            {
                OnAccept(e);
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnAccept(e);
        }

        private void OnAccept(SocketAsyncEventArgs e)
        {
            var notification = new LinkSessionConnected { LinkName = Name };

            if (e.SocketError == SocketError.Success)
            {
                notification.Result = true;
            }
            else
            {
                Log.Warn("AsyncTcpClient: ConnectAsync failed with SocketError {1}",
                    e.SocketError);
            }

            if (notification.Result)
            {
                Socket clientSocket = e.AcceptSocket;

                var session = new AsyncTcpLink.Session(this, clientSocket);
                notification.Context = session;

                session.ReceiveAsync(true);

                Accept(e);
            }

            Flow.Publish(notification);
        }
    }

    public class AsyncTcpServerFlow : SingleThreadedFlow
    {
        private AsyncTcpServer linkCase;

        public string Name { get; private set; }

        public Action<Event, LinkSession> Preprocessor { get; set; }

        public AsyncTcpServerFlow(string name)
        {
            linkCase = new AsyncTcpServer(name);
            Add(linkCase);

            Name = name;
        }

        public void Close()
        {
            linkCase.Close();
        }

        public void Listen(int port)
        {
            linkCase.Listen(port);
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

        protected virtual void OnSessionConnected(LinkSessionConnected e) {}

        protected virtual void OnSessionDisconnected(LinkSessionDisconnected e) {}

        private void OnLinkSessionConnected(LinkSessionConnected e)
        {
            OnSessionConnected(e);
        }

        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            OnSessionDisconnected(e);
        }
    }
}
