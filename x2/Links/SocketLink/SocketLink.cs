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
        protected Socket socket;  // underlying socket

        protected bool incomingKeepaliveEnabled;
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
        /// Gets or sets whether to emit outgoing keepalive events.
        /// </summary>
        public bool OutgoingKeepaliveEnabled
        {
            get { return outgoingKeepaliveEnabled; }
            set { outgoingKeepaliveEnabled = value; }
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

            var e = new KeepaliveTick { LinkName = Name };
            Bind(e, OnKeepaliveTickEvent);
            TimeFlow.Default.ReserveRepetition(e, new TimeSpan(0, 0, 5));
        }

        protected override void TearDown()
        {
            var e = new KeepaliveTick { LinkName = Name };
            TimeFlow.Default.CancelRepetition(e);
            Unbind(e, OnKeepaliveTickEvent);

            base.TearDown();
        }

        protected abstract void OnKeepaliveTick();

        protected void Keepalive(SocketLinkSession session)
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
                    if (session.IncrementFailureCount() > 1)
                    {
                        session.Close();
                        return;
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
        }

        private void OnKeepaliveTickEvent(KeepaliveTick e)
        {
            OnKeepaliveTick();
        }
    }
}
