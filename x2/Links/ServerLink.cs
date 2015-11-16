// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Common base class for multi-session server links.
    /// </summary>
    public abstract class ServerLink : SessionBasedLink
    {
        /// <summary>
        /// Searchable set of active sessions.
        /// </summary>
        protected SortedList<int, LinkSession> sessions;

        protected Dictionary<string, LinkSession> recoverable;
        protected Dictionary<int, Binder.Token> recoveryTokens;

        static ServerLink()
        {
            EventFactory.Register<SessionReq>();
        }

        /// <summary>
        /// Initializes a new instance of the ServerLink class.
        /// </summary>
        protected ServerLink(string name)
            : base(name)
        {
            sessions = new SortedList<int, LinkSession>();

            recoverable = new Dictionary<string, LinkSession>();
            recoveryTokens = new Dictionary<int, Binder.Token>();

            Diag = new Diagnostics();
        }

        /// <summary>
        /// Broadcasts the specified event to all the connected clients.
        /// </summary>
        public void Broadcast(Event e)
        {
            List<LinkSession> snapshot;
            using (new ReadLock(rwlock))
            {
                snapshot = new List<LinkSession>(sessions.Count);
                var list = sessions.Values;
                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    snapshot.Add(list[i]);
                }
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                snapshot[i].Send(e);
            }
        }

        /// <summary>
        /// Sends out the specified event through this link channel.
        /// </summary>
        public override void Send(Event e)
        {
            LinkSession session;
            using (new ReadLock(rwlock))
            {
                if (!sessions.TryGetValue(e._Handle, out session))
                {
                    return;
                }
            }
            session.Send(e);
        }

        internal override void NotifySessionConnected(bool result, object context)
        {
            if (result == true)
            {
                var session = (LinkSession)context;
                using (new WriteLock(rwlock))
                {
                    sessions.Add(session.Handle, session);
                }
            }

            base.NotifySessionConnected(result, context);
        }

        internal override void NotifySessionDisconnected(int handle, object context)
        {
            using (new WriteLock(rwlock))
            {
                sessions.Remove(handle);
            }

            base.NotifySessionDisconnected(handle, context);
        }

        internal override void OnInstantDisconnect(LinkSession session)
        {
            var e = new TimeoutEvent { Key = session };

            Binder.Token binderToken = this.Flow.Subscribe(e, OnSessionRecoveryTimeout);
            TimeFlow.Default.Reserve(e, SessionRecoveryTimeout);
            lock (recoveryTokens)
            {
                recoveryTokens.Add(session.Handle, binderToken);
            }

            Log.Trace("{0} started recovery timer for {1}", Name, session.Handle);

            using (new WriteLock(rwlock))
            {
                sessions.Remove(session.Handle);
            }
        }

        internal void OnSessionReq(LinkSession session, SessionReq e)
        {
            bool recovered = false;

            string clientToken = e.Token;
            if (!String.IsNullOrEmpty(clientToken))
            {
                bool found = false;
                LinkSession existing;
                lock (recoverable)
                {
                    if (recoverable.TryGetValue(clientToken, out existing))
                    {
                        found = true;
                    }
                }
                if (found)
                {
                    int handle = existing.Handle;
                    CancelRecoveryTimer(handle);

                    session.TakeOver(existing);

                    NotifySessionRecovered(handle, session);

                    recovered = true;
                }
            }

            if (!recovered)
            {
                // Issue a new session token for the given session.
                session.Token = Guid.NewGuid().ToString().Replace("-", "");

                Log.Debug("{0} {1} issued session token {2}",
                    Name, session.Handle, session.Token);

                lock (recoverable)
                {
                    recoverable.Add(session.Token, session);
                }
            }

            session.Send(new SessionResp {
                _Transform = false,
                Token = session.Token
            });

            if (!recovered)
            {
                OnSessionSetup(session);
            }
        }

        void OnSessionRecoveryTimeout(TimeoutEvent e)
        {
            this.Flow.Unsubscribe(e, OnSessionRecoveryTimeout);
            var session = e.Key as LinkSession;
            if (Object.ReferenceEquals(session, null))
            {
                return;
            }

            Log.Debug("{0} session recovery timeout {1} {2}",
                Name, session.Handle, session.Token);

            lock (recoverable)
            {
                recoverable.Remove(session.Token);
            }
            lock (recoveryTokens)
            {
                recoveryTokens.Remove(session.Handle);
            }
        }

        void CancelRecoveryTimer(int handle)
        {
            bool found = false;
            Binder.Token binderToken;
            lock (recoveryTokens)
            {
                if (recoveryTokens.TryGetValue(handle, out binderToken))
                {
                    recoveryTokens.Remove(handle);
                    found = true;
                }
            }
            if (found)
            {
                this.Flow.Unsubscribe(binderToken);
                TimeFlow.Default.Cancel(binderToken.Key);

                Log.Trace("{0} canceled recovery timer for {1}", Name, handle);
            }
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            using (new WriteLock(rwlock))
            {
                sessions.Clear();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by a derived link class on a successful accept.
        /// </summary>
        protected virtual bool OnAcceptInternal(LinkSession session)
        {
            if (!SessionRecoveryEnabled)
            {
                OnSessionSetup(session);
            }
            return true;
        }

        #region Diagnostics

        /// <summary>
        /// Internal diagnostics helper class.
        /// </summary>
        public new class Diagnostics : Link.Diagnostics
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
