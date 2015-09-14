// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using x2.Events;
using x2.Flows;

namespace x2.Links.Sockets
{
    /// <summary>
    /// Abstract base class for TCP/IP client links.
    /// </summary>
    public abstract class AbstractTcpClient : ClientLink
    {
        private int retryCount;
        private DateTime startTime;
        private EndPoint remoteEndPoint;

        // Socket option properties

        /// <summary>
        /// Gets or sets a value that indicates whether the client sockets are
        /// not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

        // Connect properties

        /// <summary>
        /// Gets or sets the maximum number of connection retry before the link
        /// declares a connection failure.
        /// </summary>
        /// <remarks>
        /// Default value is 0 (no retry). A negative integer such as -1 means
        /// that the link should retry for unlimited times.
        /// </remarks>
        public int MaxRetryCount { get; set; }
        /// <summary>
        /// Gets or sets the connection retry interval time in milliseconds.
        /// </summary>
        public double RetryInterval { get; set; }

        // Reconnect properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether this link should
        /// start a new connection attempt automatically on disconnect.
        /// </summary>
        public bool AutoReconnect { get; set; }
        /// <summary>
        /// Gets or sets a delay before an automatic reconnect, in milliseconds.
        /// </summary>
        public int ReconnectDelay { get; set; }

        /// <summary>
        /// Initializes a new instance of the AbstractTcpClient class.
        /// </summary>
        /// <param name="name"></param>
        protected AbstractTcpClient(string name)
            : base(name)
        {
            // Default socket options
            NoDelay = true;
        }

        /// <summary>
        /// Connects to the specified host and port.
        /// </summary>
        public void Connect(string host, int port)
        {
            IPAddress ip = null;
            try
            {
                ip = Dns.GetHostAddresses(host)[0];
            }
            catch (Exception e)
            {
                Log.Error("{0} error resolving target host {1} : {2}",
                    Name, host, e.Message);
                throw;
            }

            Connect(ip, port);
        }

        /// <summary>
        /// Connects to the specified IP address and port.
        /// </summary>
        public void Connect(IPAddress ip, int port)
        {
            IPEndPoint endpoint = new IPEndPoint(ip, port);

            Log.Info("{0} connecting to {1}", Name, endpoint);

            // Reset the retry counter.
            retryCount = 0;

            Connect(null, endpoint);
        }

        /// <summary>
        /// Reconnects to the last successful remote address.
        /// </summary>
        public void Reconnect()
        {
            if (remoteEndPoint == null)
            {
                return;
            }
            Connect(null, remoteEndPoint);
        }

        private void Connect(Socket socket, EndPoint endpoint)
        {
            startTime = DateTime.UtcNow;

            ConnectInternal(socket, endpoint);
        }

        internal override void NotifySessionDisconnected(int handle, object context)
        {
            base.NotifySessionDisconnected(handle, context);

            if (AutoReconnect)
            {
                Thread.Sleep(ReconnectDelay);

                Reconnect();
            }
        }

        /// <summary>
        /// Provides an actual implementation of Connect.
        /// </summary>
        protected abstract void ConnectInternal(Socket socket, EndPoint endpoint);

        /// <summary>
        /// <see cref="ClientLink.OnConnectInternal"/>
        /// </summary>
        protected override void OnConnectInternal(LinkSession2 session)
        {
            var tcpSession = (AbstractTcpSession)session;
            Socket socket = tcpSession.Socket;

            // Adjust socket options.
            socket.NoDelay = NoDelay;

            // Save the remote endpoint to reconnect.
            remoteEndPoint = socket.RemoteEndPoint;

            tcpSession.BeginReceive(true);

            Log.Info("{0} {1} connected to {2}",
                Name, session.Handle, socket.RemoteEndPoint);

            base.OnConnectInternal(session);
        }

        /// <summary>
        /// Called by a derived link class when a connection attempt fails.
        /// </summary>
        protected virtual void OnConnectError(Socket socket, EndPoint endpoint)
        {
            if (MaxRetryCount < 0 ||
                (MaxRetryCount > 0 && retryCount < MaxRetryCount))
            {
                if (MaxRetryCount > 0)
                {
                    ++retryCount;
                }

                double elapsedMillisecs =
                    (DateTime.UtcNow - startTime).TotalMilliseconds;
                if (elapsedMillisecs < RetryInterval)
                {
                    Thread.Sleep((int)(RetryInterval - elapsedMillisecs));
                }

                Connect(socket, endpoint);
            }
            else
            {
                socket.Close();

                NotifySessionConnected(false, endpoint);
            }
        }
    }
}
