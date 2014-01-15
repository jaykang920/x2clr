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

        public bool Listening
        {
            get { return (socket != null && socket.IsBound); }
        }

        public TcpServer(string name) : base(name)
        {
            backlog = (int)SocketOptionName.MaxConnections;
        }

        public void OnAccept(IAsyncResult asyncResult)
        {
            if (socket == null)
            {
                // Closed already
                return;
            }

            try
            {
                Socket clientSocket = socket.EndAccept(asyncResult);

                Log.Info("{0} Accepted connection from {1}",
                    clientSocket.Handle, clientSocket.RemoteEndPoint);

                Session session = new Session(clientSocket);
                LinkSessionConnected e = new LinkSessionConnected();
                e.LinkName = Name;
                e.Result = true;
                e.Context = session;
                Publish(e);

                session.BeginReceive(this, true);

                socket.BeginAccept(this.OnAccept, null);
            }
            catch (ObjectDisposedException ode)
            { // listening socket closed
                Console.WriteLine(ode.Message);
            }
        }

        public void Listen(int port)
        {
            Listen(IPAddress.Any, port);
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
                socket.BeginAccept(this.OnAccept, null);

                Log.Info("Listening on {0}", endpoint);
            }
            catch (Exception)
            {
                socket = null;
                throw;
            }
        }

        public void Listen(string ip, int port)
        {
            Listen(IPAddress.Parse(ip), port);
        }
    }
}
