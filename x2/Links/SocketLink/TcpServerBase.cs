// Copyright (c) 2013-2015 Jae-jun Kang
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
        protected SortedList<int, SocketLinkSession> sessions;
#if SESSION_RECOVERY
        protected Dictionary<string, SocketLinkSession> recoverable;
        protected Dictionary<IntPtr, x2.Flows.Timer.Token> timeoutTokens;
#endif
        protected ReaderWriterLockSlim rwlock;

        private volatile bool disposed;

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
            sessions = new SortedList<int, SocketLinkSession>();
#if SESSION_RECOVERY
            recoverable = new Dictionary<string, SocketLinkSession>();
            timeoutTokens = new Dictionary<IntPtr, x2.Flows.Timer.Token>();
#endif
            rwlock = new ReaderWriterLockSlim();

            Diag = new Diagnostics();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposed) { return; }

            socket.Close();
            rwlock.Dispose();

            disposed = true;
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

#if SESSION_RECOVERY
        public void OnSessionReq(SocketLinkSession session, SessionReq e)
        {
            if (String.IsNullOrEmpty(e.Value))
            {
                session.Token = Guid.NewGuid().ToString().Replace("-", "");

                lock (recoverable)
                {
                    recoverable.Add(session.Token, session);
                    Log.Trace("{0} {1} added recoverable {2}", Name, session.Handle, session.Token);
                }
            }
            else
            {
                session.Token = e.Value;

                lock (recoverable)
                {
                    SocketLinkSession existing;
                    if (recoverable.TryGetValue(e.Value, out existing))
                    {
                        Log.Trace("{0} {1} found recoverable session", Name, session.Handle);
                        recoverable.Remove(e.Value);
                        session.HandOver(existing);
                        lock (existing.SyncRoot)
                        {
                            existing.Status.Recovered = true;
                        }

                        Hub.Post(new LinkSessionRecovered {
                            LinkName = Name,
                            OldHandle = existing.Handle,
                            Context = session
                        });
                    }
                    else
                    {
                        // fail
                    }
                }
            }

            session.Send(new SessionResp {
                _Transform = false,
                Value = session.Token
            });

            if (String.IsNullOrEmpty(e.Value))
            {
                OnSessionSetUp(session);
            }
        }
#endif
#if SESSION_KEEPALIVE
        protected override void OnKeepaliveTick()
        {
            if (!Listening)
            {
                return;
            }
            using (new ReadLock(rwlock))
            {
                var list = sessions.Values;
                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    Keepalive(list[i]);
                }
            }
        }
#endif

        protected abstract void AcceptImpl();

        protected bool AcceptInternal(SocketLinkSession session)
        {
            ((Diagnostics)Diag).IncrementConnectionCount();

            var clientSocket = session.Socket;

            // Adjust client socket options.
            clientSocket.NoDelay = NoDelay;

            Log.Info("{0} {1} accepted from {2}",
                Name, session.Handle, clientSocket.RemoteEndPoint);

            using (new UpgradeableReadLock(rwlock))
            {
                if (sessions.ContainsKey(session.Handle))
                {
                    session.BeginReceive(true);
                    return false;
                }
                using (new WriteLock(rwlock))
                {
                    sessions.Add(session.Handle, session);
                }
            }

#if !SESSION_RECOVERY
            OnSessionSetUp(session);
#endif

            session.BeginReceive(true);
            return true;
        }

#if SESSION_RECOVERY
        public void OnInstantDisconnect(SocketLinkSession session)
        {
            if (disposed) { return; }

            bool recovered;
            lock (session.SyncRoot)
            {
                recovered = session.Status.Recovered;
            }
            if (!recovered)
            {
                var e = new TimeoutEvent { _Channel = Name, Key = session };
                Flow.Subscribe(e, OnConnectionRecoveryTimeout);
                x2.Flows.Timer.Token timeoutToken = TimeFlow.Default.Reserve(e, 10);
                timeoutTokens.Add(session.Handle, timeoutToken);

                Log.Trace("{0} added timeoutToken for {1}", Name, session.Handle);
            }

            using (new WriteLock(rwlock))
            {
                sessions.Remove(session.Id);
            }
        }

        void OnConnectionRecoveryTimeout(TimeoutEvent e)
        {
            var oldSession = (SocketLinkSession)e.Key;
            timeoutTokens.Remove(oldSession.Handle);
            OnDisconnect((SocketLinkSession)e.Key);
        }
#endif
        public override void OnDisconnect(SocketLinkSession session)
        {
            if (disposed) { return; }

            using (new WriteLock(rwlock))
            {
                sessions.Remove(session.Handle);
            }
#if SESSION_RECOVERY
            lock (recoverable)
            {
                recoverable.Remove(session.Token);
            }
#endif

            ((Diagnostics)Diag).DecrementConnectionCount();

            base.OnDisconnect(session);
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
        }

#if SESSION_RECOVERY
        protected override void OnSessionRecovered(LinkSessionRecovered e)
        {
            x2.Flows.Timer.Token timeoutToken;
            var oldSession = (SocketLinkSession)e.Context;
            if (!timeoutTokens.TryGetValue(e.OldHandle, out timeoutToken))
            {
                Log.Trace("{0} timeoutToken not found for {1}", Name, e.OldHandle);
                return;
            }
            timeoutTokens.Remove(e.OldHandle);
            Log.Trace("{0} found timeoutToken for {1}", Name, e.OldHandle);
            TimeFlow.Default.Cancel(timeoutToken);
        }
#endif

        public void Send(Event e)
        {
            SocketLinkSession session;
            using (new ReadLock(rwlock))
            {
                if (!sessions.TryGetValue(e._Handle, out session))
                {
                    return;
                }
            }
            session.Send(e);
        }

        public void Broadcast(Event e)
        {
            using (new ReadLock(rwlock))
            {
                var list = sessions.Values;
                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    list[i].Send(e);
                }
            }
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
