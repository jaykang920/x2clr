// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Net.Sockets;

using x2;
using x2.Events;
using x2.Flows;

namespace x2.Links.SocketLink
{
    /// <summary>
    /// Common abstract base class for socket-based links.
    /// </summary>
    public abstract class SocketLink : Link
    {
        protected object syncRoot = new Object();

        protected Socket socket;  // underlying socket

        protected bool incomingKeepaliveEnabled;
        protected int maxSuccessiveFailureCount;
        protected bool outgoingKeepaliveEnabled;

        /// <summary>
        /// Gets the underlying socket object.
        /// </summary>
        public Socket Socket { get { return socket; } }

        /// <summary>
        /// Gets or sets whether to check incomming keepalive events.
        /// </summary>
        public bool IncomingKeepaliveEnabled
        {
            get { return incomingKeepaliveEnabled; }
            set { incomingKeepaliveEnabled = value; }
        }
        /// <summary>
        /// Gets or sets the maximum number of successive keepalive failure to
        /// tolerate before forced close.
        /// </summary>
        public int MaxSuccessiveFailureCount
        {
            get { return maxSuccessiveFailureCount; }
            set { maxSuccessiveFailureCount = value; }
        }
        /// <summary>
        /// Gets or sets whether to emit outgoing keepalive events.
        /// </summary>
        public bool OutgoingKeepaliveEnabled
        {
            get { return outgoingKeepaliveEnabled; }
            set { outgoingKeepaliveEnabled = value; }
        }

        static SocketLink()
        {
            Event.Register<KeepaliveEvent>();
        }

        protected SocketLink(string name)
            : base(name)
        {
        }

        public virtual void OnDisconnect(SocketLinkSession session)
        {
            Flow.Publish(new LinkSessionDisconnected {
                LinkName = Name,
                Context = session
            });
        }

        protected override void SetUp()
        {
            base.SetUp();

            Flow.SubscribeTo(Name);

            var e = new KeepaliveTick { _Channel = Name, LinkName = Name };
            Bind(e, OnKeepaliveTickEvent);
            TimeFlow.Default.ReserveRepetition(e, new TimeSpan(0, 0, 5));
        }

        protected override void TearDown()
        {
            var e = new KeepaliveTick { _Channel = Name, LinkName = Name };
            TimeFlow.Default.CancelRepetition(e);
            Unbind(e, OnKeepaliveTickEvent);

            Flow.UnsubscribeFrom(Name);

            base.TearDown();
        }

        protected abstract void OnKeepaliveTick();

        protected bool Keepalive(SocketLinkSession session)
        {
            if (incomingKeepaliveEnabled)
            {
                if (session.HasReceived)
                {
                    session.HasReceived = false;
                    session.ResetFailureCount();
                }
                else
                {
                    if (session.IncrementFailureCount() > maxSuccessiveFailureCount)
                    {
                        Log.Error("{0} {1} closed due to the keepalive failure",
                            Name, session.Handle);

                        session.Close();
                        return false;
                    }
                }
            }

            if (outgoingKeepaliveEnabled)
            {
                if (session.HasSent)
                {
                    session.HasSent = false;
                }
                else
                {
                    session.Send(new KeepaliveEvent());
                }
            }
            
            return true;
        }

        private void OnKeepaliveTickEvent(KeepaliveTick e)
        {
            OnKeepaliveTick();
        }
    }
}
