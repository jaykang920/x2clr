// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Threading;

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
        public IBufferTransform BufferTransform { get; set; }

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
        public IBufferTransform BufferTransform { get; set; }

        public LinkSession(IntPtr handle)
        {
            Handle = handle;
        }

        public abstract void Close();

        public abstract void Send(Event e);

        #region Diagnostics

        /// <summary>
        /// Gets the diagnostics object.
        /// </summary>
        public Diagnostics Diag { get; protected set; }

        /// <summary>
        /// Internal diagnostics helper class.
        /// </summary>
        public class Diagnostics
        {
            protected readonly LinkSession owner;

            protected long totalBytesReceived;
            protected long totalBytesSent;
            protected int bytesReceived;
            protected int bytesSent;

            public long TotalBytesReceived
            {
                get { return Interlocked.Read(ref totalBytesReceived); }
            }

            public long TotalBytesSent
            {
                get { return Interlocked.Read(ref totalBytesSent); }
            }

            public int BytesReceived
            {
                get { return bytesReceived; }
            }

            public int BytesSent
            {
                get { return bytesSent; }
            }

            internal Diagnostics(LinkSession owner)
            {
                this.owner = owner;
            }

            internal void AddBytesReceived(int bytesReceived)
            {
                Interlocked.Add(ref totalBytesReceived, (long)bytesReceived);
                Interlocked.Add(ref this.bytesReceived, bytesReceived);
            }

            internal void AddBytesSent(int bytesSent)
            {
                Interlocked.Add(ref totalBytesSent, (long)bytesSent);
                Interlocked.Add(ref this.bytesSent, bytesSent);
            }

            public void ResetBytesReceived()
            {
                Interlocked.Exchange(ref this.bytesReceived, 0);
            }

            public void ResetBytesSent()
            {
                Interlocked.Exchange(ref this.bytesSent, 0);
            }
        }

        #endregion
    }
}
