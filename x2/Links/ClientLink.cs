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
        protected LinkSession session;      // current link session
        protected LinkSession tempSession;  // temporary link session

        /// <summary>
        /// Gets the current link session.
        /// </summary>
        public LinkSession Session
        {
            get
            {
                using (new ReadLock(rwlock))
                {
                    return session;
                }
            }
        }

        static ClientLink()
        {
            EventFactory.Register<SessionResp>();
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

        protected override void OnSessionConnectedInternal(bool result, object context)
        {
            if (result)
            {
                var session = (LinkSession)context;
                using (new WriteLock(rwlock))
                {
                    this.session = session;
                }

                Log.Debug("{0} set session {1} {2}",
                    Name, session.Handle, session.Token);
            }
        }

        protected override void OnSessionDisconnectedInternal(int handle, object context)
        {
            using (new WriteLock(rwlock))
            {
                if (!Object.ReferenceEquals(session, null))
                {
                    Log.Debug("{0} reset session {1} {2}",
                        Name, session.Handle, session.Token);

                    session = null;
                }
            }
        }

        protected override void OnSessionRecoveredInternal(int handle, object context)
        {
        }

        internal void OnSessionResp(LinkSession session, SessionResp e)
        {
            LinkSession currentSession = Session;
            string sessionToken = null;
            if (!Object.ReferenceEquals(currentSession, null))
            {
                sessionToken = currentSession.Token;
            }

            // Save the session token from the server.
            session.Token = e.Token;

            if (String.IsNullOrEmpty(sessionToken))
            {
                Log.Debug("{0} {1} session token {2}",
                    Name, session.Handle, session.Token);

                OnSessionSetup(session);
            }
            else
            {
                if (sessionToken.Equals(e.Token))
                {
                    // Recovered
                    session.TakeOver(this.session);
                    this.session = session;

                    OnSessionRecoveredInternal(this.session.Handle, session);
                }
                else
                {
                    OnLinkSessionDisconnectedInternal(currentSession.Handle, currentSession);

                    OnSessionSetup(session);
                }

                tempSession = null;
            }
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

            if (SessionRecoveryEnabled)
            {
                // Temporarily save the given session.
                tempSession = session;

                SendSessionReq(session);
            }
            else
            {
                OnSessionSetup(session);
            }
        }

        private void SendSessionReq(LinkSession tempSession)
        {
            var req = new SessionReq { _Transform = false };

            LinkSession currentSession = Session;
            if (!Object.ReferenceEquals(currentSession, null) &&
                !String.IsNullOrEmpty(currentSession.Token))
            {
                req.Token = currentSession.Token;
            }

            tempSession.Send(req);
        }
    }
}
