﻿// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2
{
    /// <summary>
    /// TCP/IP client link based on the SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncTcpClient : AbstractTcpClient
    {
        private SocketAsyncEventArgs connectEventArgs;
    
        /// <summary>
        /// Initializes a new instance of the AsyncTcpClient class.
        /// </summary>
        public AsyncTcpClient(string name)
            : base(name)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!Object.ReferenceEquals(connectEventArgs, null))
            {
                connectEventArgs.Completed -= OnConnectCompleted;
                connectEventArgs.Dispose();
                connectEventArgs = null;
            }

            base.Dispose(disposing);  // chain into the base implementation
        }

        /// <summary>
        /// <see cref="AbstractTcpClient.ConnectInternal"/>
        /// </summary>
        protected override void ConnectInternal(Socket socket, EndPoint endpoint)
        {
            try
            {
                if (Object.ReferenceEquals(socket, null))
                {
                    socket = new Socket(
                        endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                }
                if (Object.ReferenceEquals(connectEventArgs, null))
                {
                    connectEventArgs = new SocketAsyncEventArgs();
                    connectEventArgs.Completed += OnConnectCompleted;
                }
                connectEventArgs.RemoteEndPoint = endpoint;
                connectEventArgs.UserToken = socket;

                bool pending = socket.ConnectAsync(connectEventArgs);
                if (!pending)
                {
                    OnConnect(connectEventArgs);
                }
            }
            catch (Exception e)
            {
                Log.Error("{0} error connecting to {1} : {2}",
                    Name, endpoint, e.Message);

                OnConnectError(socket, endpoint);
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
            if (disposed) { return; }

            var socket = (Socket)e.UserToken;
            if (e.SocketError == SocketError.Success)
            {
                connectEventArgs.Completed -= OnConnectCompleted;
                connectEventArgs.Dispose();
                connectEventArgs = null;

                OnConnectInternal(new AsyncTcpSession(this, socket));
            }
            else
            {
                Log.Warn("{0} error connecting to {1} : {2}",
                    Name, e.RemoteEndPoint, e.SocketError);

                OnConnectError(socket, e.RemoteEndPoint);
            }
        }
    }
}
