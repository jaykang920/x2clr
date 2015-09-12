// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2;
using x2.Events;
using x2.Flows;
using x2.Queues;

namespace x2.Links
{
    /// <summary>
    /// Common base class for multi-session server links.
    /// </summary>
    public class ServerLink : SessionBasedLink
    {
        protected SortedList<int, LinkSession2> sessions;

        /// <summary>
        /// Initializes a new instance of the ServerLink class.
        /// </summary>
        public ServerLink(string name)
            : base(name)
        {
            sessions = new SortedList<int, LinkSession2>();
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
        /// Sends out the specified event through this link channel.
        /// </summary>
        public override void Send(Event e)
        {
            LinkSession2 session;
            using (new ReadLock(rwlock))
            {
                if (!sessions.TryGetValue(e._Handle, out session))
                {
                    return;
                }
            }
            session.Send(e);
        }

        /// <summary>
        /// Broadcasts the specified event to all the connected clients.
        /// </summary>
        public void Broadcast(Event e)
        {
            List<LinkSession2> snapshot;
            using (new ReadLock(rwlock))
            {
                var list = sessions.Values;
                snapshot = new List<LinkSession2>(list.Count);
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

        internal override void NotifySessionConnected(bool result, object context)
        {
            if (result == true)
            {
                var session = (LinkSession2)context;
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

        protected virtual bool AcceptInternal(LinkSession2 session)
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
    }
}
