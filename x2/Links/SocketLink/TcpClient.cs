// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using x2.Events;
using x2.Flows;

namespace x2.Links.SocketLink
{
    /// <summary>
    /// TCP/IP client link based on the Begin/End pattern.
    /// </summary>
    public class TcpClient : TcpClientBase
    {
        public TcpClient(string name)
            : base(name)
        {
        }

        protected override void ConnectImpl(EndPoint endpoint)
        {
            socket.BeginConnect(endpoint, OnConnect, endpoint);
        }

        // Asynchronous callback for BeginConnect
        private void OnConnect(IAsyncResult asyncResult)
        {
            var noti = new LinkSessionConnected { LinkName = Name };

            try
            {
                socket.EndConnect(asyncResult);

                // Adjust socket options.
                socket.NoDelay = NoDelay;

                Log.Info("{0} {1} connected to {2}", Name, socket.Handle, socket.RemoteEndPoint);

                noti.Result = true;

                ConnectInternal();

                var newSession = new TcpLinkSession(this, socket);
                newSession.Polarity = true;

                if (BufferTransform != null)
                {
                    newSession.BufferTransform = BufferTransform;
                }

                lock (syncRoot)
                {
                    session = newSession;
                }

                noti.Context = session;
                Flow.Publish(noti);

                session.BeginReceive(true);
            }
            catch (Exception e)
            {
                Log.Warn("{0} connect error: {1}", Name, e.Message);

                var endpoint = (EndPoint)asyncResult.AsyncState;
                
                noti.Context = endpoint;
                Flow.Publish(noti);

                RetryInternal(endpoint);
            }
        }
    }
}
