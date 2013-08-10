// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using x2.Events;

namespace x2.Links
{
    public class TcpServer : TcpLink
    {
        private int backlog;

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

        public TcpServer()
        {
            backlog = (int)SocketOptionName.MaxConnections;
        }

        public void OnAccept(IAsyncResult asyncResult)
        {
            try
            {
                Socket clientSocket = socket.EndAccept(asyncResult);

                Session session = new Session(this, clientSocket);
                LinkSessionConnected e = new LinkSessionConnected();
                e.Context = session;
                Feed(e);

                session.BeginReceive(true);

                socket.BeginAccept(this.OnAccept, null);
            }
            catch (ObjectDisposedException ode)
            { // listening socket closed
                Console.WriteLine(ode.Message);
            }
        }

        protected void Listen(int port)
        {
            Listen(IPAddress.Any, port);
        }

        protected void Listen(IPAddress ip, int port)
        {
            if (socket != null)
            {
                throw new InvalidOperationException();
            }
            try
            {
                socket = new Socket(ip.AddressFamily,
                  SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(ip, port));
                socket.Listen(backlog);
                socket.BeginAccept(this.OnAccept, null);
            }
            catch (Exception e)
            {
                socket = null;
                throw e;
            }
        }

        protected void Listen(string ip, int port)
        {
            Listen(IPAddress.Parse(ip), port);
        }
    }
}
