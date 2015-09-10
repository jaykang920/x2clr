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
    public abstract class ClientLink : SessionBasedLink
    {
        protected LinkSession2 session;  // current link session

        protected ReaderWriterLockSlim rwlock;

        private volatile bool disposed;

        public ClientLink(string name)
            : base(name)
        {
            rwlock = new ReaderWriterLockSlim();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            using (new WriteLock(rwlock))
            {
                session.Close();
                session = null;
            }
            rwlock.Dispose();

            disposed = true;

            base.Dispose(disposing);
        }

        public override void Send(Event e)
        {
            LinkSession2 session;
            using (new ReadLock(rwlock))
            {
                session = this.session;
            }
            if (session == null)
            {
                Log.Warn("{0} dropped event {1}", Name, e);
                return;
            }
            session.Send(e);
        }

        protected virtual void ConnectInternal(LinkSession2 session)
        {
            session.Polarity = true;

            using (new WriteLock(rwlock))
            {
                this.session = session;
            }

            new LinkSessionConnected {
                LinkName = Name,
                Result = true,
                Context = session
            }.Post();
        }

        protected override void OnSessionDisconnected(object context)
        {
            base.OnSessionDisconnected(context);

            using (new WriteLock(rwlock))
            {
                session = null;
            }
        }
    }
}
