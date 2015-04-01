// Copyright (c) 2013-2015 Jae-jun Kang
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
            lock (syncRoot)
            {
                if (socket == null)
                {
                    return;
                }

                try
                {
                    socket.EndConnect(asyncResult);

                    ConnectInternal(new TcpLinkSession(this, socket));
                }
                catch (Exception e)
                {
                    var endpoint = (EndPoint)asyncResult.AsyncState;

                    Log.Warn("{0} error connecting to {1} : {2}",
                        Name, endpoint, e.Message);

                    RetryInternal(endpoint);
                }
            }
        }
    }
}
