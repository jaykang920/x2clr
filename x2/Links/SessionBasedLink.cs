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
    public abstract class SessionBasedLink : Link2
    {
        /// <summary>
        /// Initializes a new instance of the SessionfulLink class.
        /// </summary>
        protected SessionBasedLink(string name) : base(name) { }

        /// <summary>
        /// Initializes this link on startup.
        /// </summary>
        protected override void SetUp()
        {
            Bind(new LinkSessionConnected { LinkName = Name }, OnLinkSessionConnected);
            Bind(new LinkSessionDisconnected { LinkName = Name }, OnLinkSessionDisconnected);
        }

        protected virtual void OnSessionConnected(bool result, object context) { }
        protected virtual void OnSessionDisconnected(object context) { }

        private void OnLinkSessionConnected(LinkSessionConnected e)
        {
            OnSessionConnected(e.Result, e.Context);
        }
        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            OnSessionDisconnected(e.Context);
        }
    }
}
