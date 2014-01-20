// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using x2.Events;
using x2.Flows;

namespace x2.Links.TcpLink2
{
    /// <summary>
    /// TCP/IP client link based on the Begin/End pattern.
    /// </summary>
    public class TcpClient : TcpLink2
    {
        protected TcpLink2.Session session;

        private Stopwatch stopwatch = new Stopwatch();
        private int retryCount;

        public bool AutoReconnect { get; set; }
        public int MaxRetryCount { get; set; }  // 0 for unlimited
        public long RetryInterval { get; set; }  // in millisec

        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }

        public TcpClient(string name)
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
                Log.Error("TcpClient.Connect: error resolving target host {0} - {1}", host, e.Message);
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

            Log.Info("TcpClient: connecting to {0}:{1}", ip, port);

            socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            EndPoint endpoint = new IPEndPoint(ip, port);
            BeginConnect(endpoint);
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Close();

            if (AutoReconnect)
            {
                Connect(RemoteHost, RemotePort);
            }
        }

        private void BeginConnect(EndPoint endpoint)
        {
            stopwatch.Reset();
            stopwatch.Start();

            socket.BeginConnect(endpoint, OnConnect, endpoint);
        }

        private void OnConnect(IAsyncResult asyncResult)
        {
            var notification = new LinkSessionConnected { LinkName = Name };

            try
            {
                socket.EndConnect(asyncResult);
                
                notification.Result = true;
                retryCount = 0;

                var session = new TcpLink2.Session(this, socket);
                notification.Context = session;

                session.BeginReceive(true);
            }
            catch (Exception)
            {
                Log.Warn("AsyncTcpClient: connect failed");

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
                    
                    BeginConnect((EndPoint)asyncResult.AsyncState);
                }

                notification.Context = (EndPoint)asyncResult.AsyncState;
            }

            Flow.Publish(notification);
        }
    }

    public class TcpClientFlow : SingleThreadedFlow
    {
        private TcpClient link;

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

        public TcpClientFlow(string name)
        {
            link = new TcpClient(name);
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
