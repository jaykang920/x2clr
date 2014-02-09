// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Net.Sockets;

using x2;
using x2.Events;

namespace x2.Links.SocketLink
{
    /// <summary>
    /// Common abstract base class for socket-based links.
    /// </summary>
    public abstract class SocketLink : Link
    {
        protected Socket socket;  // underlying socket

        public Action<SocketLinkSession, HeartbeatEvent> HeartbeatEventHandler { get; set; }

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
    }
}
