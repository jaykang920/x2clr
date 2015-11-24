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
            EventFactory.Register<SessionAck>();
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
            List<LinkSession> snapshot = TakeSessionsSnapshot();
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

        protected override void OnSessionConnectedInternal(bool result, object context)
        {
            if (result)
            {
                var session = (LinkSession)context;
                using (new WriteLock(rwlock))
                {
                    sessions.Add(session.Handle, session);
                }
            }
        }

        protected override void OnSessionDisconnectedInternal(int handle, object context)
        {
            var session = (LinkSession)context;
            using (new WriteLock(rwlock))
            {
                sessions.Remove(handle);
            }
            if (SessionRecoveryEnabled)
            {
                lock (recoverable)
                {
                    recoverable.Remove(session.Token);
                }
            }
        }

        protected override void OnSessionRecoveredInternal(int handle, object context)
        {
            var session = (LinkSession)context;
            using (new WriteLock(rwlock))
            {
                sessions[handle] = session;
            }
            lock (recoverable)
            {
                recoverable[session.Token] = session;
            }
        }

        internal override void OnInstantDisconnect(LinkSession session)
        {
            // Ensure that the specified session is recoverable.
            LinkSession existing = null;
            lock (recoverable)
            {
                recoverable.TryGetValue(session.Token, out existing);
            }
            if (!Object.ReferenceEquals(session, existing))
            {
                Log.Info("{0} {1} unrecoverable session", Name, session.Handle);

                OnLinkSessionDisconnectedInternal(session.Handle, session);
                return;
            }

            existing = null;
            using (new ReadLock(rwlock))
            {
                sessions.TryGetValue(session.Handle, out existing);
            }
            if (!Object.ReferenceEquals(session, existing))
            {
                Log.Warn("{0} {1} gave up session recovery", Name, session.Handle);
                return;
            }

            var e = new TimeoutEvent { Key = session };

            Binder.Token binderToken = this.Flow.Subscribe(e, OnSessionRecoveryTimeout);
            TimeFlow.Default.Reserve(e, SessionRecoveryTimeout);
            lock (recoveryTokens)
            {
                recoveryTokens.Add(session.Handle, binderToken);
            }

            Log.Trace("{0} {1} started recovery timer", Name, session.Handle);

            using (new WriteLock(rwlock))
            {
                sessions.Remove(session.Handle);
            }
        }

        internal void OnSessionReq(LinkSession session, SessionReq e)
        {
            string clientToken = e.Token;
            bool flag = false;
            if (!String.IsNullOrEmpty(clientToken))
            {
                LinkSession existing;
                lock (recoverable)
                {
                    flag = recoverable.TryGetValue(clientToken, out existing);
                }
                if (flag)
                {
                    lock (existing.SyncRoot)
                    {
                        int handle = existing.Handle;
                        CancelRecoveryTimer(handle);
                        session.TakeOver(existing);

                        OnLinkSessionRecoveredInternal(handle, session);
                    }
                }
            }

            if (!flag)
            {
                // Issue a new session token for the given session.
                session.Token = Guid.NewGuid().ToString("N");

                Log.Debug("{0} {1} issued session token {2}",
                    Name, session.InternalHandle, session.Token);

                lock (recoverable)
                {
                    recoverable.Add(session.Token, session);
                }
            }

            session.Send(new SessionResp {
                _Transform = false,
                Token = session.Token
            });
        }

        internal void OnSessionAck(LinkSession session, SessionAck e)
        {
            if (!e.Result)
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
            session.Release();

            Log.Debug("{0} {1} session recovery timeout {1} {2}",
                Name, session.Handle, session.Token);

            lock (recoveryTokens)
            {
                recoveryTokens.Remove(session.Handle);
            }

            OnLinkSessionDisconnectedInternal(session.Handle, session);
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

                Log.Trace("{0} {1} canceled recovery timer", Name, handle);
            }
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            // Closes all the active sessions
            List<LinkSession> snapshot = TakeSessionsSnapshot();
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                snapshot[i].Close();
            }

            using (new WriteLock(rwlock))
            {
                sessions.Clear();
            }

            if (SessionRecoveryEnabled)
            {
                lock (recoverable)
                {
                    recoverable.Clear();
                }
                lock (recoveryTokens)
                {
                    recoveryTokens.Clear();
                }
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

        private List<LinkSession> TakeSessionsSnapshot()
        {
            var result = new List<LinkSession>(sessions.Count);
            using (new ReadLock(rwlock))
            {
                var values = sessions.Values;
                for (int i = 0, count = values.Count; i < count; ++i)
                {
                    result.Add(values[i]);
                }
            }
            return result;
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
