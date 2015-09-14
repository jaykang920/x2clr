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
    /// Common base class for single-session client links.
    /// </summary>
    public abstract class ClientLink : SessionBasedLink
    {
        protected LinkSession2 session;

        /// <summary>
        /// Gets the current link session.
        /// </summary>
        public LinkSession2 Session {
            get
            {
                using (new ReadLock(rwlock))
                {
                    return session;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ClientLink class.
        /// </summary>
        protected ClientLink(string name)
            : base(name)
        {
            Diag = new Diagnostics();
        }

        /// <summary>
        /// Sends out the specified event through this link channel.
        /// </summary>
        public override void Send(Event e)
        {
            using (new ReadLock(rwlock))
            {
                if (session == null)
                {
                    Log.Warn("{0} dropped event {1}", Name, e);
                    return;
                }
            }
            session.Send(e);
        }

        internal override void NotifySessionConnected(bool result, object context)
        {
            if (result == true)
            {
                var session = (LinkSession2)context;
                using (new WriteLock(rwlock))
                {
                    this.session = session;
                }
            }

            base.NotifySessionConnected(result, context);
        }

        internal override void NotifySessionDisconnected(int handle, object context)
        {
            using (new WriteLock(rwlock))
            {
                this.session = null;
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
                if (session != null)
                {
                    session.Close();
                    session = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by a derived link class on a successful connect. 
        /// </summary>
        protected virtual void OnConnectInternal(LinkSession2 session)
        {
            session.Polarity = true;

            if (BufferTransform != null)
            {
                InitiateHandshake(session);
            }
            else
            {
                NotifySessionConnected(true, session);
            }
        }
    }
}
