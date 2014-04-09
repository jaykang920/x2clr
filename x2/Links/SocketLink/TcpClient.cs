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

        public TcpClient Link { get { return link; } }

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

        public Action<Event, LinkSession> Preprocessor
        {
            get { return link.Preprocessor; }
            set { link.Preprocessor = value; }
        }

        public TcpClientFlow(string name)
            : this(name, new TcpClient(name))
        {
        }

        public TcpClientFlow(string name, TcpClient link)
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

            link.Session.HeartbeatTimeoutToken = Timer.Reserve(link.Session, HeartbeatTimeout);
            Timer.ReserveRepetition(null, new TimeSpan(0, 0, 5));
        }

        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            SocketLinkSession linkSession = (SocketLinkSession)e.Context;
            Timer.Cancel(linkSession.HeartbeatTimeoutToken);
            Timer.CancelRepetition(null);

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
