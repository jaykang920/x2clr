// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using x2.Events;
using x2.Flows;
using x2.Queues;

namespace x2.Links.SocketLink
{
    /// <summary>
    /// TCP/IP server link based on the Begin/End pattern.
    /// </summary>
    public abstract class TcpServerBase : SocketLink
    {
        protected int backlog;

        /// <summary>
        /// Gets or sets the maximum length of the pending connections queue.
        /// </summary>
        public int Backlog
        {
            get { return backlog; }
            set
            {
                if (socket != null)
                {
                    throw new InvalidOperationException();
                }
                backlog = value;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the server socket is ready.
        /// </summary>
        public bool Listening
        {
            get { return (socket != null && socket.IsBound ); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the client sockets are
        /// not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

        public TcpServerBase(string name) : base(name)
        {
            backlog = Int32.MaxValue;

            // Default socket options
            NoDelay = true;

            Diag = new Diagnostics();
        }

        public override void Close()
        {
            // TODO client sockets?

            if (socket == null) { return; }
            socket.Close();
            socket = null;
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
                socket.Listen(backlog);

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

        public override void OnDisconnect(SocketLinkSession session)
        {
            ((Diagnostics)Diag).DecrementConnectionCount();

            base.OnDisconnect(session);
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            var session = (SocketLinkSession)e.Context;
            session.Close();
        }

        #region Diagnostics

        /// <summary>
        /// Internal diagnostics helper class.
        /// </summary>
        public class Diagnostics : LinkDiagnostics
        {
            protected int connectionCount;

            public int ConnectionCount
            {
                get { return connectionCount; }
            }

            internal void IncrementConnectionCount()
            {
                Interlocked.Increment(ref connectionCount);
            }

            internal void DecrementConnectionCount()
            {
                Interlocked.Decrement(ref connectionCount);
            }
        }

        #endregion  // Diagnostics
    }
}
