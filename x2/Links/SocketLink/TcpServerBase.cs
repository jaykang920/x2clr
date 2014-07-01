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
    /// Common abstract base class for TCP/IP server links.
    /// </summary>
    public abstract class TcpServerBase : SocketLink
    {
        protected int backlog;
        protected Dictionary<IntPtr, SocketLinkSession> sessions;

        //protected Dictionary<string, SocketLinkSession> recoverable;

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

        public TcpServerBase(string name) : base(name)
        {
            backlog = Int32.MaxValue;
            sessions = new Dictionary<IntPtr, SocketLinkSession>();
            //recoverable = new Dictionary<string, SocketLinkSession>();

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

        /*
        public void OnSessionTokenReq(SocketLinkSession session, SessionTokenReq e)
        {
            // XXX: check

            //string token;
            if (String.IsNullOrEmpty(e.Value))
            {
                session.Token = Guid.NewGuid().ToString().Replace("-", "");
            }
            else
            {
                session.Token = e.Value;
            }

            session.Send(new SessionTokenResp {
                Value = session.Token
            });

            Hub.Post(new LinkSessionConnected {
                LinkName = Name,
                Result = true,
                Context = session
            });
        }
        */

        protected override void OnKeepaliveTick()
        {
            if (!Listening)
            {
                return;
            }

            lock (sessions)
            {
                foreach (var session in sessions.Values)
                {
                    Keepalive(session);
                }
            }
        }

        protected abstract void AcceptImpl();

        protected void AcceptInternal(SocketLinkSession session)
        {
            ((Diagnostics)Diag).IncrementConnectionCount();

            var clientSocket = session.Socket;

            // Adjust client socket options.
            clientSocket.NoDelay = NoDelay;

            Log.Info("{0} {1} accepted from {2}",
                Name, clientSocket.Handle, clientSocket.RemoteEndPoint);

            lock (sessions)
            {
                sessions.Add(clientSocket.Handle, session);
            }

            if (BufferTransform != null)
            {
                InitiateHandshake(session);
            }
            //
            else
            {
                Hub.Post(new LinkSessionConnected {
                    LinkName = Name,
                    Result = true,
                    Context = session
                });
            }

            session.BeginReceive(true);
        }

        public void OnInstantDisconnect(SocketLinkSession session)
        {
            var e = new TimeoutEvent { _Channel = Name, Key = session };
            Flow.Subscribe(e, OnConnectionRecoveryTimeout);
            x2.Flows.Timer.Token? timerToken = TimeFlow.Default.Reserve(e, 10);

            lock (sessions)
            {
                sessions.Remove(session.Handle);
            }
            /*
            lock (recoverable)
            {
                recoverable.Add(session.Token, session);
            }
            */
        }

        void OnConnectionRecoveryTimeout(TimeoutEvent e)
        {
            OnDisconnect((SocketLinkSession)e.Key);
        }

        public override void OnDisconnect(SocketLinkSession session)
        {
            lock (sessions)
            {
                sessions.Remove(session.Handle);
            }
            /*
            lock (recoverable)
            {
                recoverable.Remove(session.Token);
            }
            */

            ((Diagnostics)Diag).DecrementConnectionCount();

            base.OnDisconnect(session);
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            var session = (SocketLinkSession)e.Context;
            session.CloseInternal();
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
