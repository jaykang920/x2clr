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
    /// TCP/IP client link based on the Begin/End pattern.
    /// </summary>
    public class TcpClient : TcpClientBase
    {
        public TcpClient(string name)
            : base(name)
        {
        }

        protected override void ConnectImpl(EndPoint endpoint)
        {
            socket.BeginConnect(endpoint, OnConnect, endpoint);
        }

        // Asynchronous callback for BeginConnect
        private void OnConnect(IAsyncResult asyncResult)
        {
            var noti = new LinkSessionConnected { LinkName = Name };

            try
            {
                socket.EndConnect(asyncResult);

                // Adjust socket options.
                socket.NoDelay = NoDelay;

                Log.Info("{0} {1} connected to {2}", Name, socket.Handle, socket.RemoteEndPoint);

                noti.Result = true;

                ConnectInternal();

                session = new TcpLinkSession(this, socket);
                session.Polarity = true;

                if (BufferTransform != null)
                {
                    session.BufferTransform = BufferTransform;
                }

                noti.Context = session;
                Flow.Publish(noti);

                session.BeginReceive(true);
            }
            catch (Exception e)
            {
                Log.Warn("{0} connect error: {1}", Name, e.Message);

                var endpoint = (EndPoint)asyncResult.AsyncState;
                
                noti.Context = endpoint;
                Flow.Publish(noti);

                RetryInternal(endpoint);
            }
        }
    }

    public class TcpClientFlow : FrameBasedFlow
    {
        protected TcpClient link;

        private volatile bool closeOnHeartbeatFailure;

        protected x2.Flows.Timer Timer { get; private set; }

        public string Name { get; private set; }

        public bool CloseOnHeartbeatFailure
        {
            get { return closeOnHeartbeatFailure; }
            set { closeOnHeartbeatFailure = value; }
        }

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

        public TcpClientFlow(string name)
        {
            link = new TcpClient(name);
            Add(link);

            link.HeartbeatEventHandler = OnHeartbeatEvent;

            Name = name;

            Resolution = Time.TicksInSecond;  // 1-second frame resolution

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
                //if (closeOnHeartbeatFailure)
                //{
                    Close();
                //}
            }
        }

        void OnHeartbeatEvent(SocketLinkSession session, HeartbeatEvent e)
        {
            Timer.Cancel(session.HeartbeatTimeoutToken);
            session.HeartbeatTimeoutToken = Timer.Reserve(new Object(), 15);
        }
    }
}
