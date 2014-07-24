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
            lock (syncRoot)
            {
                OnConnect(e);
            }
        }

        // Completion callback for ConnectAsync
        private void OnConnect(SocketAsyncEventArgs e)
        {
            if (socket == null)
            {
                return;
            }

            if (e.SocketError == SocketError.Success)
            {
                connectEventArgs.Completed -= OnConnectCompleted;
                connectEventArgs.Dispose();
                connectEventArgs = null;

                Log.Info("{0} {1} connected to {2}",
                    Name, socket.Handle, socket.RemoteEndPoint);

                ConnectInternal(new AsyncTcpLinkSession(this, socket));
            }
            else
            {
                Log.Warn("{0} error connecting to {1} : {2}",
                    Name, e.RemoteEndPoint, e.SocketError);

                RetryInternal(e.RemoteEndPoint);
            }
        }
    }
}
