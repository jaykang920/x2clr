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

                ((Diagnostics)Diag).IncrementConnectionCount();

                // Adjust client socket options.
                clientSocket.NoDelay = NoDelay;

                Log.Info("{0} {1} accepted from {2}",
                    Name, clientSocket.Handle, clientSocket.RemoteEndPoint);

                var session = new TcpLinkSession(this, clientSocket);

                if (BufferTransform != null)
                {
                    session.BufferTransform = (IBufferTransform)BufferTransform.Clone();
                }

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
}
