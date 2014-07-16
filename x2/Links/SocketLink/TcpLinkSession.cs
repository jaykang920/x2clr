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
    public class TcpLinkSession : SocketLinkSession
    {
        public TcpLinkSession(SocketLink link, Socket socket)
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
                socket.BeginReceive(recvBufferList, SocketFlags.None, OnReceive, null);
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
                socket.BeginSend(sendBufferList, SocketFlags.None, OnSend, null);
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
                    lock (syncRoot)
                    {
                        ReceiveInternal(bytesTransferred);
                    }
                    return;
                }

                // (bytesTransferred == 0) implies a graceful shutdown
                Log.Info("{0} {1} disconnected", link.Name, Handle);
            }
            catch (Exception e)
            {
                Log.Warn("{0} {1} recv error: {2}", link.Name, Handle, e.Message);
            }

            OnDisconnect();
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

                OnDisconnect();
            }
        }
    }
}
