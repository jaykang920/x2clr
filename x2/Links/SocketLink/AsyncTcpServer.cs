// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

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
    /// TCP/IP server link based on the enhanced SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncTcpServer : TcpServerBase
    {
        private SocketAsyncEventArgs acceptEventArgs;

        public AsyncTcpServer(string name)
            : base(name)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (acceptEventArgs != null)
            {
                acceptEventArgs.Completed -= OnAcceptCompleted;
                acceptEventArgs.Dispose();
                acceptEventArgs = null;
            }
        }

        protected override void AcceptImpl()
        {
            acceptEventArgs = new SocketAsyncEventArgs();
            acceptEventArgs.Completed += OnAcceptCompleted;

            AcceptImpl(acceptEventArgs);
        }

        private void AcceptImpl(SocketAsyncEventArgs e)
        {
            e.AcceptSocket = null;

            bool pending = socket.AcceptAsync(e);
            if (!pending)
            {
                OnAccept(e);
            }
        }

        // Completed event handler for AcceptAsync
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnAccept(e);
        }

        // Completion callback for AcceptAsync
        private void OnAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (!AcceptInternal(new AsyncTcpLinkSession(this, e.AcceptSocket)))
                {
                    e.AcceptSocket.Close();
                }

                AcceptImpl(e);
            }
            else
            {
                if (e.SocketError == SocketError.OperationAborted)
                {
                    // Listening socket has been closed.
                }
                else
                {
                    Log.Error("{0} accept error : {1}", Name, e.SocketError);
                }
            }
        }
    }
}
