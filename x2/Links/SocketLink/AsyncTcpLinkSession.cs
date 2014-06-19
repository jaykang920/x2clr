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
        }

        /// <summary>
        /// Closes this session.
        /// </summary>
        public override void Close()
        {
            lock (syncRoot)
            {
                base.Close();

                if (recvEventArgs != null)
                {
                    recvEventArgs.Dispose();
                    recvEventArgs = null;
                }
                if (sendEventArgs != null)
                {
                    sendEventArgs.Dispose();
                    sendEventArgs = null;
                }
            }
        }

        protected override void ReceiveImpl()
        {
            if (recvEventArgs == null || socket == null)
            {
                return;
            }
            try
            {
                recvEventArgs.BufferList = recvBufferList;

                bool pending = socket.ReceiveAsync(recvEventArgs);
                if (!pending)
                {
                    OnReceive(recvEventArgs);
                }
            }
            catch (ObjectDisposedException ode)
            {
                Log.Info("{0} {1} recv error {2}", link.Name, Handle, ode.Message);
            }
        }

        protected override void SendImpl()
        {
            if (sendEventArgs == null || socket == null)
            {
                return;
            }
            try
            {
                sendEventArgs.BufferList = sendBufferList;

                bool pending = socket.SendAsync(sendEventArgs);
                if (!pending)
                {
                    OnSend(sendEventArgs);
                }
            }
            catch (ObjectDisposedException ode)
            {
                Log.Info("{0} {1} send error {2}", link.Name, Handle, ode.Message);
            }
        }

        // Completed event handler for ReceiveAsync
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnReceive(e);
        }

        // Completed event handler for SendAsync
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnSend(e);
        }

        // Completion callback for ReceiveAsync
        private void OnReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    lock (syncRoot)
                    {
                        ReceiveInternal(e.BytesTransferred);
                    }
                    return;
                }

                // (e.BytesTransferred == 0) implies a graceful shutdown
                Log.Info("{0} {1} disconnected", link.Name, Handle);
            }
            else
            {
                if (e.SocketError != SocketError.OperationAborted)
                {
                    Log.Warn("{0} {1} recv error {2}", link.Name, Handle, e.SocketError);
                }
            }

            link.OnDisconnect(this);
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
                if (e.SocketError == SocketError.OperationAborted)
                {
                    // Socket has been closed.
                    return;
                }
                else
                {
                    Log.Warn("{0} {1} send error {2}", link.Name, Handle, e.SocketError);
                }

                link.OnDisconnect(this);
            }
        }
    }
}
