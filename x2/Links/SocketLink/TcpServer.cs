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

namespace x2.Links.SocketLink
{
    /// <summary>
    /// TCP/IP server link based on the Begin/End pattern.
    /// </summary>
    public class TcpServer : TcpServerBase
    {
        public TcpServer(string name)
            : base(name)
        {
        }

        protected override void AcceptImpl()
        {
            socket.BeginAccept(OnAccept, null);
        }

        // Asynchronous callback for BeginAccept
        private void OnAccept(IAsyncResult asyncResult)
        {
            try
            {
                var clientSocket = socket.EndAccept(asyncResult);

                var session = new TcpLinkSession(this, clientSocket);

                Flow.Publish(new LinkSessionConnected {
                    LinkName = Name,
                    Result = true,
                    Context = session
                });

                session.BeginReceive(true);

                AcceptImpl();
            }
            catch (Exception e)
            {
                Log.Warn("{0} accept error: {1}", Name, e.Message);
            }
        }
    }

    public class TcpServerFlow : SingleThreadedFlow
    {
        private TcpServer link;

        public string Name { get; private set; }

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
