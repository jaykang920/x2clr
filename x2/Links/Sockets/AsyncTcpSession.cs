// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2
{
    /// <summary>
    /// TCP/IP link session based on the SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncTcpSession : AbstractTcpSession
    {
        private SocketAsyncEventArgs recvEventArgs;
        private SocketAsyncEventArgs sendEventArgs;

        public AsyncTcpSession(SessionBasedLink link, Socket socket)
            : base(link, socket)
        {
            recvEventArgs = new SocketAsyncEventArgs();
            sendEventArgs = new SocketAsyncEventArgs();

            recvEventArgs.Completed += OnReceiveCompleted;
            sendEventArgs.Completed += OnSendCompleted;
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            if (recvEventArgs != null)
            {
                recvEventArgs.Completed -= OnReceiveCompleted;
                recvEventArgs.Dispose();

                Log.Debug("{0} {1} freed recvEventArgs", link.Name, Handle);
            }
            if (sendEventArgs != null)
            {
                sendEventArgs.Completed -= OnSendCompleted;
                sendEventArgs.Dispose();

                Log.Debug("{0} {1} freed sendEventArgs", link.Name, Handle);
            }

            base.Dispose(disposing);
        }

        protected override void ReceiveInternal()
        {
            try
            {
                rxBufferList.Clear();
                rxBuffer.ListAvailableSegments(rxBufferList);
                recvEventArgs.BufferList = rxBufferList;

                bool pending = socket.ReceiveAsync(recvEventArgs);
                if (!pending)
                {
                    Log.Debug("{0} {1} ReceiveAsync completed immediately", link.Name, Handle);

                    OnReceive(recvEventArgs);
                }
            }
            catch (ObjectDisposedException ode)
            {
                Log.Debug("{0} {1} recv error {2}", link.Name, Handle, ode.Message);
            }
            catch (Exception e)
            {
                Log.Info("{0} {1} recv error {2}", link.Name, Handle, e);

                OnDisconnect();
            }
        }

        protected override void SendInternal()
        {
            try
            {
                sendEventArgs.BufferList = txBufferList;

                bool pending = socket.SendAsync(sendEventArgs);
                if (!pending)
                {
                    Log.Debug("{0} {1} SendAsync completed immediately", link.Name, Handle);

                    OnSend(sendEventArgs);
                }
            }
            catch (ObjectDisposedException ode)
            {
                Log.Debug("{0} {1} send error {2}", link.Name, Handle, ode.Message);
            }
            catch (Exception e)
            {
                Log.Info("{0} {1} send error {2}", link.Name, Handle, e);

                OnDisconnect();
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
            e.BufferList = null;

            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    OnReceiveInternal(e.BytesTransferred);
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

            OnDisconnect();
        }

        // Completion callback for SendAsync
        private void OnSend(SocketAsyncEventArgs e)
        {
            e.BufferList = null;

            if (e.SocketError == SocketError.Success)
            {
                //lock (syncTx)
                {
                    OnSendInternal(e.BytesTransferred);
                }
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

                OnDisconnect();
            }
        }
    }
}
