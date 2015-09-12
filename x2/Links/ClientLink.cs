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
    public class ClientLink : SessionBasedLink
    {
        protected LinkSession2 session;

        /// <summary>
        /// Gets or sets the current link session.
        /// </summary>
        public LinkSession2 Session {
            get { return session; }
            set { session = value; }
        }

        /// <summary>
        /// Initializes a new instance of the ClientLink class.
        /// </summary>
        public ClientLink(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            using (new WriteLock(rwlock))
            {
                session.Close();
                session = null;
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
                session = this.session;
            }
            if (session == null)
            {
                Log.Warn("{0} dropped event {1}", Name, e);
                return;
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

        protected virtual void ConnectInternal(LinkSession2 session)
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
