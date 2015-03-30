// Copyright (c) 2013-2015 Jae-jun Kang
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
        /// <summary>
        /// Underlying socket object.
        /// </summary>
        protected Socket socket;

        /// <summary>
        /// Synchronization object.
        /// </summary>
        protected object syncRoot = new Object();

        /// <summary>
        /// Gets or sets a value that indicates whether the client sockets are
        /// not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

        /// <summary>
        /// Gets the underlying socket object.
        /// </summary>
        public Socket Socket { get { return socket; } }

#if SESSION_KEEPALIVE
        /// <summary>
        /// Gets or sets whether to check incomming keepalive events.
        /// </summary>
        public bool IncomingKeepaliveEnabled { get; set; }
        /// <summary>
        /// Gets or sets the maximum number of successive keepalive failure to
        /// tolerate before forced close.
        /// </summary>
        public int MaxSuccessiveFailureCount { get; set; }
        /// <summary>
        /// Gets or sets whether to emit outgoing keepalive events.
        /// </summary>
        public bool OutgoingKeepaliveEnabled { get; set; }
#endif

        static SocketLink()
        {
            EventFactory.Register<HandshakeReq>();
            EventFactory.Register<HandshakeResp>();
            EventFactory.Register<HandshakeAck>();
#if SESSION_KEEPALIVE
            EventFactory.Register<KeepaliveEvent>();
#endif
#if SESSION_RECOVERY
            Event.Register<SessionReq>();
            Event.Register<SessionResp>();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the SocketLink class.
        /// </summary>
        protected SocketLink(string name)
            : base(name)
        {
            // Default socket options
            NoDelay = true;
        }

        public virtual void OnDisconnect(SocketLinkSession session)
        {
            Hub.Post(new LinkSessionDisconnected {
                LinkName = Name,
                Context = session
            });
        }

        /// <summary>
        /// Initializes this link on startup.
        /// </summary>
        protected override void SetUp()
        {
            base.SetUp();
#if SESSION_KEEPALIVE
            Flow.SubscribeTo(Name);

            var e = new KeepaliveTick { _Channel = Name, LinkName = Name };
            Bind(e, OnKeepaliveTickEvent);
            TimeFlow.Default.ReserveRepetition(e, new TimeSpan(0, 0, 5));
#endif
        }

        /// <summary>
        /// Cleans up this link on shutdown.
        /// </summary>
        protected override void TearDown()
        {
#if SESSION_KEEPALIVE
            var e = new KeepaliveTick { _Channel = Name, LinkName = Name };
            TimeFlow.Default.CancelRepetition(e);
            Unbind(e, OnKeepaliveTickEvent);

            Flow.UnsubscribeFrom(Name);
#endif
            base.TearDown();
        }

        protected void InitiateHandshake(SocketLinkSession session)
        {
            session.BufferTransform = (IBufferTransform)BufferTransform.Clone();

            var req = new HandshakeReq { _Transform = false };

            byte[] data = session.BufferTransform.InitializeHandshake();

            if (data != null)
            {
                req.Data = data;
            }

            session.Send(req);
        }

        protected void OnSessionSetUp(SocketLinkSession session)
        {
            if (BufferTransform != null)
            {
                InitiateHandshake(session);
            }
            else
            {
                Hub.Post(new LinkSessionConnected {
                    LinkName = Name,
                    Result = true,
                    Context = session
                });
            }
        }

#if SESSION_KEEPALIVE
        protected abstract void OnKeepaliveTick();

        protected bool Keepalive(SocketLinkSession session)
        {
            Log.Trace("{0} {1} keepalive", Name, session.Handle);

            lock (session.SyncRoot)
            {
                if (session.Socket == null || !session.Socket.Connected)
                {
                    return true;
                }
            }

            if (IncomingKeepaliveEnabled)
            {
                if (session.HasReceived)
                {
                    session.HasReceived = false;
                    session.ResetFailureCount();
                }
                else
                {
                    if (session.IncrementFailureCount() > MaxSuccessiveFailureCount)
                    {
                        Log.Error("{0} {1} closed due to the keepalive failure",
                            Name, session.Handle);

                        session.Close();
                        return false;
                    }
                }
            }

            if (OutgoingKeepaliveEnabled)
            {
                if (session.HasSent)
                {
                    session.HasSent = false;
                }
                else
                {
                    Log.Trace("{0} {1} sent keepalive event", Name, session.Handle);

                    session.Send(new KeepaliveEvent { _Transform = false });
                }
            }
            
            return true;
        }

        private void OnKeepaliveTickEvent(KeepaliveTick e)
        {
            OnKeepaliveTick();
        }
#endif
    }
}
