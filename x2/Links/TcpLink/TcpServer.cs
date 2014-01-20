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

namespace x2.Links.TcpLink2
{
    /// <summary>
    /// TCP/IP server link based on the Begin/End pattern.
    /// </summary>
    public class TcpServer : TcpLink2
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

        public TcpServer(string name) : base(name)
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

                socket.BeginAccept(OnAccept, null);

                Log.Info("TcpServer: listening on {0}", endpoint);
            }
            catch (Exception)
            {
                socket = null;
                throw;
            }
        }

        private void OnAccept(IAsyncResult asyncResult)
        {
            var notification = new LinkSessionConnected { LinkName = Name };

            try
            {
                var clientSocket = socket.EndAccept(asyncResult);

                notification.Result = true;

                var session = new TcpLink2.Session(this, clientSocket);
                notification.Context = session;

                session.BeginReceive(true);

                socket.BeginAccept(OnAccept, null);
            }
            catch (Exception)
            {
                Log.Warn("TcpServer: Accept failed with SocketError");
                throw;
            }

            Flow.Publish(notification);
        }
    }

    public class TcpServerFlow : SingleThreadedFlow
    {
        private TcpServer link;

        public string Name { get; private set; }

        public Action<Event, LinkSession> Preprocessor { get; set; }

        public TcpServerFlow(string name)
        {
            link = new TcpServer(name);
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
