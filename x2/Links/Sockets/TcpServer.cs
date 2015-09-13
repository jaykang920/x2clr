// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

using x2;

namespace x2.Links.Sockets
{
    /// <summary>
    /// TCP/IP server link based on the Begin/End pattern.
    /// </summary>
    public class TcpServer : AbstractTcpServer
    {
        /// <summary>
        /// Initializes a new instance of the TcpServer class.
        /// </summary>
        public TcpServer(string name)
            : base(name)
        {
        }

        /// <summary>
        /// <see cref="AbstractTcpServer.AcceptInternal"/>
        /// </summary>
        protected override void AcceptInternal()
        {
            socket.BeginAccept(OnAccept, null);
        }

        // Asynchronous callback for BeginAccept
        private void OnAccept(IAsyncResult asyncResult)
        {
            try
            {
                var clientSocket = socket.EndAccept(asyncResult);

                if (!OnAcceptInternal(new TcpSession(this, clientSocket)))
                {
                    NotifySessionConnected(false, clientSocket.RemoteEndPoint);
                    clientSocket.Close();
                }
            }
            catch (ObjectDisposedException)
            {
                Log.Info("{0} listening socket closed", Name);
                return;
            }
            catch (Exception e)
            {
                Log.Error("{0} accept error : {1}", Name, e.Message);
            }

            AcceptInternal();
        }
    }
}
