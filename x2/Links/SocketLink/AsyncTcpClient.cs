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
    /// TCP/IP client link based on the enhanced SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncTcpClient : TcpClientBase
    {
        private SocketAsyncEventArgs connectEventArgs;

        public AsyncTcpClient(string name)
            : base(name)
        {
        }

        protected override void ConnectImpl(EndPoint endpoint)
        {
            if (connectEventArgs == null)
            {
                connectEventArgs = new SocketAsyncEventArgs();
                connectEventArgs.Completed += OnConnectCompleted;
            }
            connectEventArgs.RemoteEndPoint = endpoint;

            bool pending = socket.ConnectAsync(connectEventArgs);
            if (!pending)
            {
                OnConnect(connectEventArgs);
            }
        }

        // Completed event handler for ConnectAsync
        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnConnect(e);
        }

        // Completion callback for ConnectAsync
        private void OnConnect(SocketAsyncEventArgs e)
        {
            var noti = new LinkSessionConnected { LinkName = Name };

            if (e.SocketError == SocketError.Success)
            {
                // Adjust socket options.
                socket.NoDelay = NoDelay;

                Log.Info("{0} {1} connected to {2}", Name, socket.Handle, socket.RemoteEndPoint);

                noti.Result = true;

                ConnectInternal();

                connectEventArgs = null;

                session = new AsyncTcpLinkSession(this, socket);

                noti.Context = session;
                Flow.Publish(noti);

                session.BeginReceive(true);
            }
            else
            {
                Log.Warn("{0} connect error {1}", Name, e.SocketError);

                noti.Context = e.RemoteEndPoint;
                Flow.Publish(noti);

                RetryInternal(e.RemoteEndPoint);
            }
        }
    }

    public class AsyncTcpClientFlow : SingleThreadedFlow
    {
        private AsyncTcpClient link;

        public string Name { get; private set; }

        public bool AutoReconnect
        {
            get { return link.AutoReconnect; }
            set { link.AutoReconnect = value; }
        }
        public int MaxRetryCount  // 0 for unlimited
        {
            get { return link.MaxRetryCount; }
            set { link.MaxRetryCount = value; }
        }
        public long RetryInterval  // in millisec
        {
            get { return link.RetryInterval; }
            set { link.RetryInterval = value; }
        }

        public Action<Event, LinkSession> Preprocessor { get; set; }

        public AsyncTcpClientFlow(string name)
        {
            link = new AsyncTcpClient(name);
            Add(link);

            Name = name;
        }

        public void Close()
        {
            link.Close();
        }

        public void Connect(string host, int port)
        {
            link.Connect(host, port);
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
    }
}
