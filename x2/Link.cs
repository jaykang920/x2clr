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

        protected Link() : base(new UnboundedQueue<Event>()) {}

        public abstract void Close();

        protected override void SetUp()
        {
            base.SetUp();

            Subscribe(new LinkSessionConnected(), this, OnLinkSessionConnected);
            Subscribe(new LinkSessionDisconnected(), this, OnLinkSessionDisconnected);
        }

        protected override void TearDown()
        {
            Unsubscribe(new LinkSessionDisconnected(), this, OnLinkSessionDisconnected);
            Unsubscribe(new LinkSessionConnected(), this, OnLinkSessionConnected);

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


        public abstract class Session<T>
        {
            private readonly T handle;

            public T Handle { get { return handle; } }

            public Session(T handle)
            {
                this.handle = handle;
            }

            public abstract void Send(x2.Event e);
        }
    }
}
