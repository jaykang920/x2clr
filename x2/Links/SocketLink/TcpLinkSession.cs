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
    public class TcpLinkSession : SocketLinkSession
    {
        public TcpLinkSession(SocketLink link, Socket socket)
            : base(link, socket)
        {
        }

        protected override void ReceiveImpl()
        {
            socket.BeginReceive(recvBufferList, SocketFlags.None, OnReceive, null);
        }

        protected override void SendImpl()
        {
            socket.BeginSend(sendBufferList, SocketFlags.None, OnSend, null);
        }

        // Asynchronous callback for BeginReceive
        private void OnReceive(IAsyncResult asyncResult)
        {
            try
            {
                int bytesTransferred = socket.EndReceive(asyncResult);

                if (bytesTransferred > 0)
                {
                    ReceiveInternal(bytesTransferred);
                    return;
                }

                // (bytesTransferred == 0) implies a graceful shutdown
                link.Flow.Publish(new LinkSessionDisconnected {
                    LinkName = link.Name,
                    Context = this
                });
            }
            catch (Exception e)
            {
                link.Flow.Publish(new LinkSessionDisconnected {
                    LinkName = link.Name,
                    Context = this
                });

                if (e is SocketException)
                {
                    var se = (SocketException)e;
                    Log.Info("TcpLink.OnReceive SocketException {0} {1}", se.ErrorCode, e.Message);
                }
                else
                {
                    Log.Info("TcpLink.OnReceive Exception {0}", e.Message);
                }
            }
        }

        // Asynchronous callback for BeginSend
        private void OnSend(IAsyncResult asyncResult)
        {
            try
            {
                int bytesTransferred = socket.EndSend(asyncResult);

                SendInternal(bytesTransferred);
            }
            catch (Exception e)
            {
                link.Flow.Publish(new LinkSessionDisconnected {
                    LinkName = link.Name,
                    Context = this
                });

                if (e is SocketException)
                {
                    var se = (SocketException)e;
                    Log.Info("TcpLink.OnSend SocketException {0} {1}", se.ErrorCode, e.Message);
                }
                else
                {
                    Log.Info("TcpLink.OnSend Exception {0}", e.Message);
                }
            }
        }
    }
}
