// Copyright (c) 2013, 2014 Jae-jun Kang
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

                if (!AcceptInternal(new TcpLinkSession(this, clientSocket)))
                {
                    clientSocket.Close();
                }

                AcceptImpl();
            }
            catch (Exception e)
            {
                Log.Warn("{0} accept error : {1}", Name, e.Message);
            }
        }
    }
}
