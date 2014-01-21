// Copyright (c) 2013 Jae-jun Kang
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
    public class AsyncTcpLinkSession : SocketLinkSession
    {
        private SocketAsyncEventArgs recvEventArgs;
        private SocketAsyncEventArgs sendEventArgs;

        public AsyncTcpLinkSession(SocketLink link, Socket socket)
            : base(link, socket)
        {
            recvEventArgs = new SocketAsyncEventArgs();
            sendEventArgs = new SocketAsyncEventArgs();

            recvEventArgs.Completed += OnReceiveCompleted;
            sendEventArgs.Completed += OnSendCompleted;

            recvEventArgs.UserToken = this;
            sendEventArgs.UserToken = this;
        }

        protected override void ReceiveImpl()
        {
            recvEventArgs.BufferList = recvBufferList;

            bool pending = socket.ReceiveAsync(recvEventArgs);
            if (!pending)
            {
                OnReceive(recvEventArgs);
            }
        }

        protected override void SendImpl()
        {
            sendEventArgs.BufferList = sendBufferList;

            bool pending = socket.SendAsync(sendEventArgs);
            if (!pending)
            {
                OnSend(sendEventArgs);
            }
        }

        // Completed event handler for ReceiveAsync
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            var session = (AsyncTcpLinkSession)e.UserToken;

            session.OnReceive(e);
        }

        // Completed event handler for SendAsync
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            var session = (AsyncTcpLinkSession)e.UserToken;

            session.OnSend(e);
        }

        // Completion callback for ReceiveAsync
        private void OnReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    ReceiveInternal(e.BytesTransferred);
                    return;
                }

                // (e.BytesTransferred == 0) implies a graceful shutdown
            }
            else
            {
                // log error
            }

            link.Flow.Publish(new LinkSessionDisconnected {
                LinkName = link.Name,
                Context = this
            });
        }

        // Completion callback for SendAsync
        private void OnSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                SendInternal(e.BytesTransferred);
            }
            else
            {
                link.Flow.Publish(new LinkSessionDisconnected {
                    LinkName = link.Name,
                    Context = this
                });
            }
        }
    }
}
