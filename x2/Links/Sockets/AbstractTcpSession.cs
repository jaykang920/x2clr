// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using x2;

namespace x2.Links.Sockets
{
    /// <summary>
    /// Abstract base class for TCP/IP link sessions.
    /// </summary>
    public abstract class AbstractTcpSession : LinkSession2
    {
        protected Socket socket;

        /// <summary>
        /// Gets the underlying Socket object.
        /// </summary>
        public Socket Socket { get { return socket; } }

        /// <summary>
        /// Initializes a new instance of the AbstractTcpSession class.
        /// </summary>
        protected AbstractTcpSession(SessionBasedLink link, Socket socket)
            : base(link)
        {
            this.socket = socket;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            if (socket != null)
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                socket.Close();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by a derived session class on send/receive error.
        /// </summary>
        protected void OnDisconnect()
        {
            EndPoint endpoint;
            try
            {
                endpoint = socket.RemoteEndPoint;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            OnDisconnect(endpoint);
        }
    }
}
