// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

using x2;

namespace x2.Links.Sockets
{
    public class TcpServer : AbstractTcpServer
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

                if (!AcceptInternal(new TcpSession(this, clientSocket)))
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
