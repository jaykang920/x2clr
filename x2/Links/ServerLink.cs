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
        protected SortedList<int, LinkSession> sessions;

        /// <summary>
        /// Initializes a new instance of the ServerLink class.
        /// </summary>
        protected ServerLink(string name)
            : base(name)
        {
            sessions = new SortedList<int, LinkSession>();

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
            if (BufferTransform != null)
            {
                InitiateHandshake(session);
            }
            else
            {
                NotifySessionConnected(true, session);
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
