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
    /// Abstract base class for TCP/IP server links.
    /// </summary>
    public abstract class AbstractTcpServer : ServerLink
    {
        protected Socket socket;

        /// <summary>
        /// Gets or sets a value that indicates whether the client sockets are
        /// not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

        protected AbstractTcpServer(string name) : base(name)
        {
            // Default socket options
            NoDelay = true;
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

                AcceptInternal();

                Log.Info("{0} listening on {1}", Name, endpoint);
            }
            catch (Exception)
            {
                socket = null;
                throw;
            }
        }

        /// <summary>
        /// Provides an actual implementation of Accept.
        /// </summary>
        protected abstract void AcceptInternal();

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            socket.Close();

            base.Dispose(disposing);
        }

        /// <summary>
        /// <see cref="ServerLink.OnAcceptInternal(LinkSession2)"/>
        /// </summary>
        protected override bool OnAcceptInternal(LinkSession2 session)
        {
            var tcpSession = (AbstractTcpSession)session;
            var clientSocket = tcpSession.Socket;

            // Adjust client socket options.
            clientSocket.NoDelay = NoDelay;

            tcpSession.BeginReceive(true);

            Log.Info("{0} {1} accepted from {2}",
                Name, session.Handle, clientSocket.RemoteEndPoint);

            return base.OnAcceptInternal(session);
        }
    }
}
