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
        /// <summary>
        /// Gets or sets a value that indicates whether the client sockets are
        /// not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

        protected AbstractTcpClient(string name)
            : base(name)
        {
            // Default socket options
            NoDelay = true;
        }

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

        private void Connect(IPAddress ip, int port)
        {
            IPEndPoint ep = new IPEndPoint(ip, port);
            try
            {
                ConnectInternal(ep);

                Log.Info("{0} connecting to {1}", Name, ep);
            }
            catch (Exception e)
            {
                Log.Error("{0} error connecting to {1} : {2}",
                    Name, ep, e.Message);

                new LinkSessionConnected {
                    LinkName = Name,
                    Result = false,
                    Context = ep
                }.Post();
            }
        }

        /// <summary>
        /// Provides an actual implementation of Connect.
        /// </summary>
        protected abstract void ConnectInternal(EndPoint endpoint);

        /// <summary>
        /// <see cref="ClientLink.OnConnectInternal(LinkSession2)"/>
        /// </summary>
        protected override void OnConnectInternal(LinkSession2 session)
        {
            var tcpSession = (AbstractTcpSession)session;
            Socket socket = tcpSession.Socket;

            // Adjust socket options.
            socket.NoDelay = NoDelay;

            tcpSession.BeginReceive(true);

            Log.Info("{0} {1} connected to {2}",
                Name, session.Handle, socket.RemoteEndPoint);

            base.OnConnectInternal(session);
        }
    }
}
