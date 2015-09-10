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
    public abstract class ServerLink : SessionBasedLink
    {
        protected SortedList<int, LinkSession2> sessions;
        protected ReaderWriterLockSlim rwlock;

        private volatile bool disposed;

        public ServerLink(string name)
            : base(name)
        {
            sessions = new SortedList<int, LinkSession2>();
            rwlock = new ReaderWriterLockSlim();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            try
            {
                sessions.Clear();
                rwlock.Dispose();
            }
            finally
            {
                disposed = true;
            }

            base.Dispose(disposing);
        }

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

        protected virtual bool AcceptInternal(LinkSession2 session)
        {
            using (new WriteLock(rwlock))
            {
                sessions.Add(session.Handle, session);
            }

            new LinkSessionConnected {
                LinkName = Name,
                Result = true,
                Context = session
            }.Post();

            return true;
        }
    }
}
