// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Common base class for single-session client links.
    /// </summary>
    public abstract class ClientLink : SessionBasedLink
    {
        protected LinkSession session;

        /// <summary>
        /// Gets the current link session.
        /// </summary>
        public LinkSession Session {
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
                var session = (LinkSession)context;
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

            LinkSession session = null;
            using (new WriteLock(rwlock))
            {
                if (this.session != null)
                {
                    session = this.session;
                    this.session = null;
                }
            }
            if (session != null)
            {
                session.Close();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by a derived link class on a successful connect. 
        /// </summary>
        protected virtual void OnConnectInternal(LinkSession session)
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
