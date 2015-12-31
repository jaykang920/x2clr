﻿// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// TCP/IP link session based on the Begin/End pattern.
    /// </summary>
    public class TcpSession : AbstractTcpSession
    {
        public TcpSession(SessionBasedLink link, Socket socket)
            : base(link, socket)
        {
        }

        protected override void ReceiveInternal()
        {
            try
            {
                rxBufferList.Clear();
                rxBuffer.ListAvailableSegments(rxBufferList);

                socket.BeginReceive(rxBufferList, SocketFlags.None, OnReceive, null);
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Log.Warn("{0} {1} recv error {2}", link.Name, handle, e);

                OnDisconnect();
            }
        }

        protected override void SendInternal()
        {
            try
            {
                socket.BeginSend(txBufferList, SocketFlags.None, OnSend, null);
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Log.Warn("{0} {1} send error {2}", link.Name, handle, e);

                OnDisconnect();
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
                    OnReceiveInternal(bytesTransferred);
                    return;
                }

                // (bytesTransferred == 0) implies a graceful shutdown
                Log.Info("{0} {1} disconnected", link.Name, handle);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                var se = e as SocketException;
                if (se != null &&
                    se.SocketErrorCode == SocketError.OperationAborted)
                {
                    return;
                }
                Log.Warn("{0} {1} recv error {2}", link.Name, handle, e);
            }
            OnDisconnect();
        }

        // Asynchronous callback for BeginSend
        private void OnSend(IAsyncResult asyncResult)
        {
            try
            {
                int bytesTransferred = socket.EndSend(asyncResult);

                OnSendInternal(bytesTransferred);
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                var se = e as SocketException;
                if (se != null &&
                    se.SocketErrorCode == SocketError.OperationAborted)
                {
                    return;
                }
                Log.Warn("{0} {1} send error {2}", link.Name, handle, e);
                OnDisconnect();
            }
        }
    }
}
