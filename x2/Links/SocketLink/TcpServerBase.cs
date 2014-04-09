// Copyright (c) 2013 Jae-jun Kang
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
            catch (Exception e)
            {
                socket = null;
                throw;
            }
        }

        protected abstract void AcceptImpl();

        public override void OnDisconnect(SocketLinkSession session)
        {
            Diag.DecrementConnectionCount();

            base.OnDisconnect(session);
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            var session = (SocketLinkSession)e.Context;
            session.Close();
        }

        #region Diagnostics

        /// <summary>
        /// Gets the diagnostics object.
        /// </summary>
        public Diagnostics Diag { get; protected set; }

        /// <summary>
        /// Internal diagnostics helper class.
        /// </summary>
        public class Diagnostics
        {
            protected readonly TcpServerBase owner;

            protected int connectionCount;

            protected long totalBytesReceived;
            protected long totalBytesSent;
            protected long bytesReceived;
            protected long bytesSent;

            public int ConnectionCount
            {
                get { return connectionCount; }
            }

            public long TotalBytesReceived
            {
                get { return Interlocked.Read(ref totalBytesReceived); }
            }

            public long TotalBytesSent
            {
                get { return Interlocked.Read(ref totalBytesSent); }
            }

            public long BytesReceived
            {
                get { return Interlocked.Read(ref bytesReceived); }
            }

            public long BytesSent
            {
                get { return Interlocked.Read(ref bytesSent); }
            }

            internal Diagnostics(TcpServerBase owner)
            {
                this.owner = owner;
            }

            internal void IncrementConnectionCount()
            {
                Interlocked.Increment(ref connectionCount);
            }

            internal void DecrementConnectionCount()
            {
                Interlocked.Decrement(ref connectionCount);
            }

            internal void AddBytesReceived(long bytesReceived)
            {
                Interlocked.Add(ref totalBytesReceived, bytesReceived);
                Interlocked.Add(ref this.bytesReceived, bytesReceived);
            }

            internal void AddBytesSent(long bytesSent)
            {
                Interlocked.Add(ref totalBytesSent, bytesSent);
                Interlocked.Add(ref this.bytesSent, bytesSent);
            }

            public void ResetBytesReceived()
            {
                Interlocked.Exchange(ref this.bytesReceived, 0L);
            }

            public void ResetBytesSent()
            {
                Interlocked.Exchange(ref this.bytesSent, 0L);
            }
        }

        #endregion  // Diagnostics
    }
}
