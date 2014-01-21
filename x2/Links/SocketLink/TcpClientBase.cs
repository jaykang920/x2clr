// Copyright (c) 2013 Jae-jun Kang
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
    public abstract class TcpClientBase : SocketLink
    {
        protected SocketLinkSession session;

        protected string remoteHost;
        protected int remotePort;

        protected Stopwatch stopwatch;
        protected int retryCount;

        public bool AutoReconnect { get; set; }
        public int MaxRetryCount { get; set; }  // 0 for unlimited
        public long RetryInterval { get; set; }  // in millisec

        public TcpClientBase(string name)
            : base(name)
        {
            stopwatch = new Stopwatch();
        }

        public override void Close()
        {
            if (session == null)
            {
                return;
            }

            session.Close();

            session = null;
            socket = null;
        }

        public void Connect(string host, int port)
        {
            remoteHost = host;
            remotePort = port;

            IPAddress ip = null;
            try
            {
                ip = Dns.GetHostAddresses(host)[0];
            }
            catch (Exception e)
            {
                Log.Error("TcpClient: error resolving target host {0} - {1}",
                    host, e.Message);
                throw;
            }

            Connect(ip, port);
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Close();

            if (AutoReconnect)
            {
                Connect(remoteHost, remotePort);
            }
        }

        private void Connect(IPAddress ip, int port)
        {
            if (socket != null)
            {
                throw new InvalidOperationException();
            }

            Log.Info("TcpClient: connecting to {0}:{1}", ip, port);

            socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            BeginConnect(new IPEndPoint(ip, port));
        }

        private void BeginConnect(EndPoint endpoint)
        {
            stopwatch.Reset();
            stopwatch.Start();

            ConnectImpl(endpoint);
        }

        protected abstract void ConnectImpl(EndPoint endpoint);

        protected void ConnectInternal()
        {
            // Reset the retry counter.
            retryCount = 0;
        }

        protected void RetryInternal(EndPoint endpoint)
        {
            if (MaxRetryCount <= 0 ||
                (MaxRetryCount > 0 && retryCount < MaxRetryCount))
            {
                ++retryCount;

                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds < RetryInterval)
                {
                    Thread.Sleep((int)(RetryInterval - stopwatch.ElapsedMilliseconds));
                }

                BeginConnect(endpoint);
            }
        }
    }
}
