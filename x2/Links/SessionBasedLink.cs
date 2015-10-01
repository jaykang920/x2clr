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
        }

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
