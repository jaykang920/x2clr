// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

using x2.Events;
using x2.Flows;
using x2.Queues;

namespace x2
{
    public abstract class Link : SingleThreadedFlow
    {
        protected const byte sentinel = 0x55;

        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets whether this link itself is included as a target when
        /// publishing remote events. Default value is false.
        /// XXX: to be deprecated
        /// </summary>
        public bool IsSelfPublishingEnabled { get; set; }

        public Link(string name)
        {
            Name = name;
        }

        public abstract void Close();

        protected override void SetUp()
        {
            base.SetUp();

            Subscribe(new LinkSessionConnected(), OnLinkSessionConnected);
            Subscribe(new LinkSessionDisconnected(), OnLinkSessionDisconnected);
        }

        protected override void TearDown()
        {
            Unsubscribe(new LinkSessionDisconnected(), OnLinkSessionDisconnected);
            Unsubscribe(new LinkSessionConnected(), OnLinkSessionConnected);

            base.TearDown();
        }

        protected virtual void OnSessionConnected(LinkSessionConnected e) {}

        protected virtual void OnSessionDisconnected(LinkSessionDisconnected e) {}

        private void OnLinkSessionConnected(LinkSessionConnected e)
        {
            OnSessionConnected(e);
        }

        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            OnSessionDisconnected(e);
        }


        public abstract class Session
        {
            public IntPtr Handle { get; private set; }

            public Session(IntPtr handle)
            {
                Handle = handle;
            }

            public abstract void Send(Link link, x2.Event e);
        }
    }
}
