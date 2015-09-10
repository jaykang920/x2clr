// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using x2.Events;
using x2.Flows;
using x2.Queues;

namespace x2.Links.Sockets
{
    /// <summary>
    /// Common abstract base class for TCP/IP server links.
    /// </summary>
    public abstract class AbstractTcpServer : ServerLink
    {
        protected Socket socket;

        private volatile bool disposed;

        /// <summary>
        /// Gets or sets a value that indicates whether the client sockets are
        /// not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

        public AbstractTcpServer(string name) : base(name)
        {
            // Default socket options
            NoDelay = true;

            Diag = new Diagnostics();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            try
            {
                socket.Close();
            }
            finally
            {
                disposed = true;
            }

            base.Dispose(disposing);
        }

        public void Listen(int port)
        {
            Listen(IPAddress.Any, port);
        }

        public void Listen(string ip, int port)
        {
            Listen(IPAddress.Parse(ip), port);
        }

        public void Listen(IPAddress ip, int port)
        {
            if (socket != null)
            {
                throw new InvalidOperationException();
            }

            try
            {
                socket = new Socket(ip.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                EndPoint endpoint = new IPEndPoint(ip, port);
                socket.Bind(endpoint);
                socket.Listen(Int32.MaxValue);

                AcceptImpl();

                Log.Info("{0} listening on {1}", Name, endpoint);
            }
            catch (Exception)
            {
                socket = null;
                throw;
            }
        }

        protected abstract void AcceptImpl();

        protected override bool AcceptInternal(LinkSession2 session)
        {
            var tcpSession = (AbstractTcpSession)session;
            var clientSocket = tcpSession.Socket;

            // Adjust client socket options.
            clientSocket.NoDelay = NoDelay;

            Log.Info("{0} {1} accepted from {2}",
                Name, session.Handle, clientSocket.RemoteEndPoint);

            tcpSession.BeginReceive(true);

            return base.AcceptInternal(session);
        }
    }
}
