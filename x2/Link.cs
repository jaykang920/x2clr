// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

using x2;
using x2.Events;
using x2.Flows;
using x2.Queues;

namespace x2
{
    /// <summary>
    /// Abstract base class for concrete link cases.
    /// </summary>
    public abstract class Link : Case
    {
        public string Name { get; private set; }

        public Action<Event, LinkSession> Preprocessor { get; set; }

        public Link(string name)
        {
            Name = name;
        }

        public abstract void Close();

        protected override void SetUp()
        {
            Bind(new LinkSessionConnected { LinkName = Name }, OnLinkSessionConnected);
            Bind(new LinkSessionDisconnected { LinkName = Name }, OnLinkSessionDisconnected);
        }

        protected override void TearDown()
        {
            Close();
        }

        protected virtual void OnSessionConnected(LinkSessionConnected e) { }

        protected virtual void OnSessionDisconnected(LinkSessionDisconnected e) { }

        private void OnLinkSessionConnected(LinkSessionConnected e)
        {
            OnSessionConnected(e);
        }

        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            OnSessionDisconnected(e);
        }
    }

    /// <summary>
    /// Abstract base class for concrete link sessions.
    /// </summary>
    public abstract class LinkSession
    {
        public IntPtr Handle { get; private set; }

        public LinkSession(IntPtr handle)
        {
            Handle = handle;
        }

        public abstract void Close();

        public abstract void Send(Event e);
    }


    // <to be deprecated>
    public abstract class LinkFlow : SingleThreadedFlow
    {
        public string Name { get; private set; }

        public Action<Event, Session> Preprocessor { get; set; }

        /// <summary>
        /// Gets or sets whether this link itself is included as a target when
        /// publishing remote events. Default value is false.
        /// XXX: to be deprecated
        /// </summary>
        public bool IsSelfPublishingEnabled { get; set; }

        public LinkFlow(string name)
        {
            Name = name;
        }

        public abstract void Close();

        protected override void SetUp()
        {
            base.SetUp();

            Subscribe(new LinkSessionConnected { LinkName = Name }, OnLinkSessionConnected);
            Subscribe(new LinkSessionDisconnected { LinkName = Name }, OnLinkSessionDisconnected);
        }

        protected override void TearDown()
        {
            Unsubscribe(new LinkSessionDisconnected { LinkName = Name }, OnLinkSessionDisconnected);
            Unsubscribe(new LinkSessionConnected() { LinkName = Name }, OnLinkSessionConnected);

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

            public abstract void Close();

            public abstract void Send(LinkFlow link, Event e);
        }
    }
}
