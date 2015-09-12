// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

using x2;

namespace x2.Links.Sockets
{
    public class AsyncTcpServer : AbstractTcpServer
    {
        private const int numConcurrentAcceptors = 16;

        private SocketAsyncEventArgs[] acceptEventArgs;

        public AsyncTcpServer(string name)
            : base(name)
        {
            acceptEventArgs = new SocketAsyncEventArgs[numConcurrentAcceptors];

            for (int i = 0; i < numConcurrentAcceptors; ++i)
            {
                var saea = new SocketAsyncEventArgs();
                saea.Completed += OnAcceptCompleted;
                acceptEventArgs[i] = saea;
            }
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            for (int i = 0; i < numConcurrentAcceptors; ++i)
            {
                var saea = acceptEventArgs[i];
                saea.Completed -= OnAcceptCompleted;
                saea.Dispose();
            }

            acceptEventArgs = null;

            base.Dispose(disposing);
        }

        protected override void AcceptImpl()
        {
            for (int i = 0, count = acceptEventArgs.Length; i < count; ++i)
            {
                AcceptImpl(acceptEventArgs[i]);
            }
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
                if (!AcceptInternal(new AsyncTcpSession(this, e.AcceptSocket)))
                {
                    e.AcceptSocket.Close();
                }
            }
            else
            {
                if (e.SocketError == SocketError.OperationAborted)
                {
                    Log.Info("{0} listening socket closed", Name);
                    return;
                }
                else
                {
                    Log.Error("{0} accept error : {1}", Name, e.SocketError);
                }
            }

            AcceptImpl(e);  // chain into the next accept
        }
    }
}
