// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

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
                ((Diagnostics)Diag).IncrementConnectionCount();

                var clientSocket = e.AcceptSocket;

                // Adjust client socket options.
                clientSocket.NoDelay = NoDelay;

                Log.Info("{0} {1} accepted from {2}",
                    Name, clientSocket.Handle, clientSocket.RemoteEndPoint);

                var session = new AsyncTcpLinkSession(this, clientSocket);

                if (BufferTransform != null)
                {
                    session.BufferTransform = (IBufferTransform)BufferTransform.Clone();
                }

                lock (sessions)
                {
                    sessions.Add(clientSocket.Handle, session);
                }

                if (BufferTransform != null)
                {
                    byte[] data = session.BufferTransform.InitializeHandshake();
                    session.Send(new HandshakeReq { Data = data });
                }
                else
                {
                    Flow.Publish(new LinkSessionConnected {
                        LinkName = Name,
                        Result = true,
                        Context = session
                    });
                }

                session.BeginReceive(true);

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
                    Log.Error("{0} accept error {1}", Name, e.SocketError);
                }
            }
        }
    }
}
