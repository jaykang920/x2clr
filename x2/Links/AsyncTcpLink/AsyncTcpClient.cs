// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Net;
using System.Net.Sockets;

using x2.Events;
using x2.Flows;

namespace x2.Links.AsyncTcpLink
{
    public class AsyncTcpClientCase : AsyncTcpLinkCase
    {
        protected AsyncTcpLinkSession session;

        public AsyncTcpClientCase(string name)
            : base(name)
        {
        }

        public override void Close()
        {
            if (socket == null) { return; }

            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Send);
            }
            socket.Close();

            socket = null;
        }

        public void Connect(string host, int port)
        {
            IPAddress ip = null;
            try
            {
                ip = Dns.GetHostAddresses(host)[0];
            }
            catch (Exception e)
            {
                Log.Error("AsyncTcpClientCase.Connect: error resolving target host {0} - {1}", host, e.Message);
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

            Log.Info("AsyncTcpClientCase: connecting to {0}:{1}", ip, port);

            socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var e = new SocketAsyncEventArgs();
            e.Completed += OnConnectCompleted;
            e.RemoteEndPoint = new IPEndPoint(ip, port);

            bool pending = socket.ConnectAsync(e);
            if (!pending)
            {
                OnConnect(e);
            }
        }

        /*
        protected void Reconnect(string ip, int port)
        {
            Reconnect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        protected void Reconnect(EndPoint endpoint)
        {
            if (socket == null)
            {
                throw new InvalidOperationException();
            }
            socket.BeginConnect(endpoint, this.OnConnect, endpoint);
        }
        */

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
            }
            else
            {
                Log.Warn("AsyncTcpClientCase: ConnectAsync failed with SocketError {1}",
                    e.SocketError);
            }

            if (notification.Result)
            {
                AsyncTcpLinkSession session = new AsyncTcpLinkSession(this, socket);
                notification.Context = session;

                session.Receive();
            }
            else
            {
                notification.Context = e.RemoteEndPoint;
            }

            Flow.Publish(notification);
        }
    }

    public class AsyncTcpClient : SingleThreadedFlow
    {
        private AsyncTcpClientCase linkCase;

        public string Name { get; private set; }

        public Action<Event, LinkSession> Preprocessor { get; set; }

        public AsyncTcpClient(string name)
        {
            linkCase = new AsyncTcpClientCase(name);
            Add(linkCase);

            Name = name;
        }

        public void Close()
        {
            linkCase.Close();
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
