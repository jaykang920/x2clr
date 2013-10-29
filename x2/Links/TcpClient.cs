// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Net;
using System.Net.Sockets;

using x2.Events;

namespace x2.Links
{
    public class TcpClient : TcpLink
    {
        public TcpClient(string name) : base(name) { }

        protected void Connect(string ip, int port)
        {
            Connect(IPAddress.Parse(ip), port);
        }

        protected void Connect(IPAddress ip, int port)
        {
            if (socket != null)
            {
                throw new InvalidOperationException();
            }
            socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(ip, port);
            socket.BeginConnect(endpoint, this.OnConnect, endpoint);
        }

        protected void Reconnect(string ip, int port)
        {
            Reconnect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        protected void Reconnect(EndPoint endpoint)
        {
            if (socket == null)
            {
                throw new InvalidOperationException();
            }
            socket.BeginConnect(endpoint, this.OnConnect, endpoint);
        }

        private void OnConnect(IAsyncResult asyncResult)
        {
            LinkSessionConnected e = new LinkSessionConnected();
            e.LinkName = Name;
            try
            {
                socket.EndConnect(asyncResult);
                e.Result = true;
            }
            catch (SocketException se)
            {
                // socket error
            }

            if (e.Result)
            {
                Session session = new Session(socket);
                e.Result = true;
                e.Context = session;

                session.BeginReceive(this, true);
            }
            else
            {
                e.Context = asyncResult.AsyncState;  // remote endpoint
            }
            Publish(e);
        }
    }
}
