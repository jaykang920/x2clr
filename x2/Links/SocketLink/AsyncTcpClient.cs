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
                session.Polarity = true;

                if (BufferTransform != null)
                {
                    session.BufferTransform = BufferTransform;
                }

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

    public class AsyncTcpClientFlow : FrameBasedFlow
    {
        protected AsyncTcpClient link;

        private volatile bool closeOnHeartbeatFailure;

        protected x2.Flows.Timer Timer { get; private set; }

        public bool CloseOnHeartbeatFailure
        {
            get { return closeOnHeartbeatFailure; }
            set { closeOnHeartbeatFailure = value; }
        }
        public double HeartbeatTimeout { get; set; }  // in seconds

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

            link.HeartbeatEventHandler = OnHeartbeatEvent;

            this.name = name;

            Resolution = Time.TicksInSecond;  // 1-second frame resolution
            HeartbeatTimeout = 15;

            Timer = new x2.Flows.Timer(OnTimer);
        }

        public void Close()
        {
            Timer.Cancel(link.Session.HeartbeatTimeoutToken);

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

            link.Session.HeartbeatTimeoutToken = Timer.Reserve(new Object(), 15);
            Timer.ReserveRepetition(null, new TimeSpan(0, 0, 5));
        }

        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            Timer.CancelRepetition(null);
            if (link.Session != null)
            {
                Timer.Cancel(link.Session.HeartbeatTimeoutToken);
            }

            OnSessionDisconnected(e);
        }

        protected override void Start() { }
        protected override void Stop() { }

        protected override void Update()
        {
            Timer.Tick();
        }

        void OnTimer(object state)
        {
            if (state == null)
            {
                link.Send(new HeartbeatEvent { Timestamp = DateTime.Now.Ticks });
            }
            else
            {
                // heartbeat timeout
                if (closeOnHeartbeatFailure)
                {
                    Log.Error("{0} {1} heartbeat timeout", Name, link.Session.Handle);
                    Close();
                }
                else
                {
                    if (link.Session == null)
                    {
                        return;
                    }
                    Log.Warn("{0} {1} heartbeat timeout", Name, link.Session.Handle);
                    link.Session.HeartbeatTimeoutToken = Timer.Reserve(new Object(), 15);
                }
            }
        }

        void OnHeartbeatEvent(SocketLinkSession session, HeartbeatEvent e)
        {
            Timer.Cancel(session.HeartbeatTimeoutToken);
            session.HeartbeatTimeoutToken = Timer.Reserve(new Object(), 15);
        }
    }
}
