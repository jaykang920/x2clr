// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using x2.Events;
using x2.Flows;

namespace x2.Links.Sockets
{
    /// <summary>
    /// TCP/IP client link based on the Begin/End pattern.
    /// </summary>
    public class TcpClient : AbstractTcpClient
    {
        public TcpClient(string name)
            : base(name)
        {
        }

        protected override void ConnectImpl(EndPoint endpoint)
        {
            var socket = new Socket(
                endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.BeginConnect(endpoint, OnConnect, socket);
        }

        // Asynchronous callback for BeginConnect
        private void OnConnect(IAsyncResult asyncResult)
        {
            try
            {
                var socket = (Socket)asyncResult.AsyncState;
                socket.EndConnect(asyncResult);

                ConnectInternal(new TcpSession(this, socket));
            }
            catch (Exception e)
            {
                var socket = (Socket)asyncResult.AsyncState;

                Log.Warn("{0} error connecting to {1} : {2}",
                    Name, socket.RemoteEndPoint, e.Message);

                //RetryInternal(endpoint);
            }
        }
    }
}
