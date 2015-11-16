// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Abstract base class for session-based links.
    /// </summary>
    public abstract class SessionBasedLink : Link
    {
        protected ReaderWriterLockSlim rwlock;

        private volatile bool sessionRecoveryEnabled;

        /// <summary>
        /// Gets or sets a boolean value inidicating whether this link supports
        /// automatic session recovery on instant disconnection.
        /// </summary>
        public bool SessionRecoveryEnabled
        {
            get { return sessionRecoveryEnabled; }
            set { sessionRecoveryEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the session recovery timeout (in seconds).
        /// </summary>
        public int SessionRecoveryTimeout { get; set; }

        /// <summary>
        /// A delegate type for hooking up event proprocess notifications.
        /// </summary>
        public delegate void PreprocessEventHandler(LinkSession session, Event e);

        /// <summary>
        /// An event that clients can use to be notified whenever a new event is
        /// ready for preprocessing.
        /// </summary>
        public event PreprocessEventHandler Preprocess;

        static SessionBasedLink()
        {
            EventFactory.Register<HandshakeReq>();
            EventFactory.Register<HandshakeResp>();
            EventFactory.Register<HandshakeAck>();
        }

        /// <summary>
        /// Initializes a new instance of the SessionBasedLink class.
        /// </summary>
        protected SessionBasedLink(string name)
            : base(name)
        {
            rwlock = new ReaderWriterLockSlim();

            SessionRecoveryTimeout = 10;  // 10 seconds by default
        }

        /// <summary>
        /// Notifies the application that a session creation attempt has been
        /// completed with the specified result.
        /// </summary>
        internal virtual void NotifySessionConnected(bool result, object context)
        {
            Hub.Post(new LinkSessionConnected {
                LinkName = Name,
                Result = result,
                Context = context
            });

            Log.Info("{0} connected {1} {2}", Name, result, context);
        }

        /// <summary>
        /// Notifies the application that the link session identified by the
        /// specified session handle has been closed.
        /// </summary>
        internal virtual void NotifySessionDisconnected(int handle, object context)
        {
            Hub.Post(new LinkSessionDisconnected {
                LinkName = Name,
                Handle = handle,
                Context = context
            });

            Log.Info("{0} disconnected {1} {2}", Name, handle, context);
        }

        internal virtual void NotifySessionRecovered(int handle, object context)
        {
            Hub.Post(new LinkSessionRecovered {
                LinkName = Name,
                Handle = handle,
                Context = context
            });

            Log.Info("{0} recovered {1} {2}", Name, handle, context);
        }

        internal abstract void OnInstantDisconnect(LinkSession session);

        internal protected void OnPreprocess(LinkSession session, Event e)
        {
            if (Preprocess != null)
            {
                Preprocess(session, e);
            }
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            rwlock.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by a derived class to initiate a buffer transform handshake.
        /// </summary>
        protected void InitiateHandshake(LinkSession session)
        {
            if (Object.ReferenceEquals(BufferTransform, null))
            {
                return;
            }

            var bufferTransform = (IBufferTransform)BufferTransform.Clone();
            session.BufferTransform = bufferTransform;

            session.Send(new HandshakeReq {
                _Transform = false,
                Data = bufferTransform.InitializeHandshake()
            });
        }

        /// <summary>
        /// Called when a new session creation attempt is completed.
        /// </summary>
        protected virtual void OnSessionConnected(bool result, object context)
        {
        }

        /// <summary>
        /// Called when an existing link session is closed.
        /// </summary>
        protected virtual void OnSessionDisconnected(int handle, object context)
        {
        }

        /// <summary>
        /// Called when a new link session is ready for open.
        /// </summary>
        protected void OnSessionSetup(LinkSession session)
        {
            if (BufferTransform != null)
            {
                InitiateHandshake(session);
            }
            else
            {
                NotifySessionConnected(true, session);
            }
        }

        /// <summary>
        /// Initializes this link on startup.
        /// </summary>
        protected override void Setup()
        {
            Bind(new LinkSessionConnected { LinkName = Name },
                OnLinkSessionConnected);
            Bind(new LinkSessionDisconnected { LinkName = Name },
                OnLinkSessionDisconnected);
        }

        // LinkSessionConnected event handler
        private void OnLinkSessionConnected(LinkSessionConnected e)
        {
            OnSessionConnected(e.Result, e.Context);
        }

        // LinkSessionDisconnected event handler
        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            OnSessionDisconnected(e.Handle, e.Context);
        }
    }
}
