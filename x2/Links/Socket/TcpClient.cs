// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2
{
    /// <summary>
    /// TCP/IP client link based on the Begin/End pattern.
    /// </summary>
    public class TcpClient : AbstractTcpClient
    {
        private EndPoint remoteEndPoint;

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

                remoteEndPoint = endpoint;
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
                    Name, remoteEndPoint, e.Message);

                OnConnectError(socket, remoteEndPoint);
            }
        }
    }
}
