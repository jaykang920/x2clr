// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using x2;

namespace x2.Links.Sockets
{
    /// <summary>
    /// TCP/IP client link based on the Begin/End pattern.
    /// </summary>
    public class TcpClient : AbstractTcpClient
    {
        /// <summary>
        /// Initializes a new instance of the TcpClient class.
        /// </summary>
        public TcpClient(string name)
            : base(name)
        {
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

                socket.BeginConnect(endpoint, OnConnect, socket);
            }
            catch (Exception e)
            {
                Log.Error("{0} error connecting to {1} : {2}",
                    Name, endpoint, e.Message);

                OnConnectError(socket, endpoint);
            }
        }

        // Asynchronous callback for BeginConnect
        private void OnConnect(IAsyncResult asyncResult)
        {
            var socket = (Socket)asyncResult.AsyncState;
            try
            {
                socket.EndConnect(asyncResult);

                OnConnectInternal(new TcpSession(this, socket));
            }
            catch (Exception e)
            {
                Log.Warn("{0} error connecting to {1} : {2}",
                    Name, socket.RemoteEndPoint, e.Message);

                OnConnectError(socket, socket.RemoteEndPoint);
            }
        }
    }
}
