// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

using x2;

namespace x2.Links.Sockets
{
    public class TcpSession : AbstractTcpSession
    {
        public TcpSession(SessionBasedLink link, Socket socket)
            : base(link, socket)
        {
        }

        protected override void ReceiveImpl()
        {
            if (socket == null || !socket.Connected)
            {
                return;
            }
            try
            {
                rxBufferList.Clear();
                rxBuffer.ListAvailableSegments(rxBufferList);

                socket.BeginReceive(rxBufferList, SocketFlags.None, OnReceive, null);
            }
            catch (ObjectDisposedException ode)
            {
                Log.Info("{0} {1} recv error {2}", link.Name, Handle, ode.Message);
            }
        }

        protected override void SendImpl()
        {
            if (socket == null || !socket.Connected)
            {
                return;
            }
            try
            {
                socket.BeginSend(txBufferList, SocketFlags.None, OnSend, null);
            }
            catch (ObjectDisposedException ode)
            {
                Log.Info("{0} {1} send error {2}", link.Name, Handle, ode.Message);
            }
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
                Log.Info("{0} {1} disconnected", link.Name, Handle);
            }
            catch (Exception e)
            {
                Log.Warn("{0} {1} recv error: {2}", link.Name, Handle, e.Message);
            }

            OnDisconnect(socket.RemoteEndPoint);
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
                var se = e as SocketException;
                if (se != null)
                {
                    if (se.SocketErrorCode == SocketError.OperationAborted)
                    {
                        // Socket has been closed.
                        return;
                    }
                }

                Log.Warn("{0} {1} send error: {2}", link.Name, Handle, e.Message);

                OnDisconnect(socket.RemoteEndPoint);
            }
        }
    }
}
