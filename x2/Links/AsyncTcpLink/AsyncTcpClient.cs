// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using x2.Events;
using x2.Flows;

namespace x2.Links.AsyncTcpLink
{
    /// <summary>
    /// TCP/IP client link based on the enhanced SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncTcpClient : AsyncTcpLink
    {
        protected AsyncTcpLink.Session session;

        private Stopwatch stopwatch = new Stopwatch();
        private int retryCount;

        public bool AutoReconnect { get; set; }
        public int MaxRetryCount { get; set; }  // 0 for unlimited
        public long RetryInterval { get; set; }  // in millisec

        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }

        public AsyncTcpClient(string name)
            : base(name)
        {
        }

        public override void Close()
        {
            if (socket == null) { return; }

            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            socket.Close();

            socket = null;
        }

        public void Connect(string host, int port)
        {
            RemoteHost = host;
            RemotePort = port;

            IPAddress ip = null;
            try
            {
                ip = Dns.GetHostAddresses(host)[0];
            }
            catch (Exception e)
            {
                Log.Error("AsyncTcpClient.Connect: error resolving target host {0} - {1}", host, e.Message);
            }

            if (ip != null)
            {
                Connect(ip, port);
            }
        }

        public void Connect(IPAddress ip, int port)
        {
            if (socket != null)
            {
                throw new InvalidOperationException();
            }

            Log.Info("AsyncTcpClient: connecting to {0}:{1}", ip, port);

            socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var e = new SocketAsyncEventArgs();
            e.Completed += OnConnectCompleted;
            e.RemoteEndPoint = new IPEndPoint(ip, port);

            ConnectAsync(e);
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Close();

            if (AutoReconnect)
            {
                Connect(RemoteHost, RemotePort);
            }
        }

        private void ConnectAsync(SocketAsyncEventArgs e)
        {
            stopwatch.Reset();
            stopwatch.Start();

            bool pending = socket.ConnectAsync(e);
            if (!pending)
            {
                OnConnect(e);
            }
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnConnect(e);
        }

        private void OnConnect(SocketAsyncEventArgs e)
        {
            var notification = new LinkSessionConnected { LinkName = Name };

            if (e.SocketError == SocketError.Success)
            {
                notification.Result = true;
                
                ///...
                ///

                // Reset the retry counter
                retryCount = 0;
            }
            else
            {
                Log.Warn("AsyncTcpClient: connect failed with SocketError {0}",
                    e.SocketError);

                // Connection retry
                if (MaxRetryCount <= 0 ||
                    (MaxRetryCount > 0 && retryCount < MaxRetryCount))
                {
                    ++retryCount;

                    stopwatch.Stop();
                    if (stopwatch.ElapsedMilliseconds < RetryInterval)
                    {
                        Thread.Sleep((int)(RetryInterval - stopwatch.ElapsedMilliseconds));
                    }
                    
                    ConnectAsync(e);
                }
            }

            if (notification.Result)
            {
                var session = new AsyncTcpLink.Session(this, socket);
                notification.Context = session;

                session.ReceiveAsync(true);
            }
            else
            {
                notification.Context = e.RemoteEndPoint;
            }

            Flow.Publish(notification);
        }
    }

    public class AsyncTcpClientFlow : SingleThreadedFlow
    {
        private AsyncTcpClient linkCase;

        public string Name { get; private set; }

        public bool AutoReconnect
        {
            get { return linkCase.AutoReconnect; }
            set { linkCase.AutoReconnect = value; }
        }
        public int MaxRetryCount  // 0 for unlimited
        {
            get { return linkCase.MaxRetryCount; }
            set { linkCase.MaxRetryCount = value; }
        }
        public long RetryInterval  // in millisec
        {
            get { return linkCase.RetryInterval; }
            set { linkCase.RetryInterval = value; }
        }

        public Action<Event, LinkSession> Preprocessor { get; set; }

        public AsyncTcpClientFlow(string name)
        {
            linkCase = new AsyncTcpClient(name);
            Add(linkCase);

            Name = name;
        }

        public void Close()
        {
            linkCase.Close();
        }

        public void Connect(string host, int port)
        {
            linkCase.Connect(host, port);
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
