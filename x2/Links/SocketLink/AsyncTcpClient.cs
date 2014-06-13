// Copyright (c) 2013, 2014 Jae-jun Kang
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

                connectEventArgs.Dispose();
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
}
