// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2;
using x2.Events;
using x2.Flows;
using x2.Queues;

namespace x2
{
    /// <summary>
    /// Common base class for link cases.
    /// </summary>
    public class Link : Case
    {
        private static HashSet<string> names;

        private volatile bool disposed;

        public string Name { get; private set; }
        public IBufferTransform BufferTransform { get; set; }

        public Action<Event, LinkSession> Preprocessor { get; set; }

        static Link()
        {
            names = new HashSet<string>();
        }

        /// <summary>
        /// Initializes a new instance of the Link class.
        /// </summary>
        protected Link(string name)
        {
            lock (names)
            {
                if (names.Contains(name))
                {
                    throw new ArgumentException("requested link name is already in use");
                }

                Name = name;
                names.Add(name);
            }
        }

        ~Link()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes this link and releases all the associated resources.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposed) { return; }

            if (BufferTransform != null)
            {
                BufferTransform.Dispose();
                BufferTransform = null;
            }

            lock (names)
            {
                names.Remove(Name);
            }

            disposed = true;
        }

        /// <summary>
        /// Initializes this link on startup.
        /// </summary>
        protected override void SetUp()
        {
            Bind(new LinkSessionConnected { LinkName = Name }, OnLinkSessionConnected);
            Bind(new LinkSessionDisconnected { LinkName = Name }, OnLinkSessionDisconnected);
#if SESSION_RECOVERY
            Bind(new LinkSessionRecovered { LinkName = Name }, OnLinkSessionRecovered);
#endif
        }

        /// <summary>
        /// Cleans up this link on shutdown.
        /// </summary>
        protected override void TearDown()
        {
            Unbind(new LinkSessionConnected { LinkName = Name }, OnLinkSessionConnected);
            Unbind(new LinkSessionDisconnected { LinkName = Name }, OnLinkSessionDisconnected);
#if SESSION_RECOVERY
            Unbind(new LinkSessionRecovered { LinkName = Name }, OnLinkSessionRecovered);
#endif
            Close();
        }

        protected virtual void OnSessionConnected(LinkSessionConnected e) { }

        protected virtual void OnSessionDisconnected(LinkSessionDisconnected e) { }

#if SESSION_RECOVERY
        protected virtual void OnSessionRecovered(LinkSessionRecovered e) { }
#endif

        private void OnLinkSessionConnected(LinkSessionConnected e)
        {
            OnSessionConnected(e);
        }

        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            OnSessionDisconnected(e);
        }

#if SESSION_RECOVERY
        private void OnLinkSessionRecovered(LinkSessionRecovered e)
        {
            OnSessionRecovered(e);
        }
#endif

        #region Diagnostics

        /// <summary>
        /// Gets the diagnostics object.
        /// </summary>
        public LinkDiagnostics Diag { get; protected set; }

        #endregion  // Diagnostics
    }

    /// <summary>
    /// Abstract base class for concrete link sessions.
    /// </summary>
    public abstract class LinkSession : IDisposable
    {
        private static RangedIntPool handlePool;
        private volatile bool disposed;

        /// <summary>
        /// Gets the link session handle that is unique in the current process.
        /// </summary>
        public int Handle { get; private set; }

        /// <summary>
        /// Gets or sets the BufferTransform for this link session.
        /// </summary>
        public IBufferTransform BufferTransform { get; set; }

        static LinkSession()
        {
            handlePool = new RangedIntPool(1, 65536, true);  // [1, 65536]
        }

        /// <summary>
        /// Initializes a new instance of the LinkSession class.
        /// </summary>
        protected LinkSession()
        {
            Handle = handlePool.Acquire();  // acquire a session handle
        }

        ~LinkSession()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes this link session and releases all the associated resources.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Implements IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) { return; }

#if !SESSION_RECOVERY
            if (BufferTransform != null)
            {
                BufferTransform.Dispose();
            }
#endif

            handlePool.Release(Handle);  // release the session handle

            disposed = true;
        }

        /// <summary>
        /// Sends out the specified event through this link session.
        /// </summary>
        public abstract void Send(Event e);

        #region Diagnostics

        /// <summary>
        /// Gets the diagnostics object.
        /// </summary>
        public LinkDiagnostics Diag { get; protected set; }
        
        #endregion  // Diagnostics
    }

    #region Diagnostics

    /// <summary>
    /// Link diagnostics helper class.
    /// </summary>
    public class LinkDiagnostics
    {
        protected long totalBytesReceived;
        protected long totalBytesSent;
        protected long bytesReceived;
        protected long bytesSent;

        protected long totalEventsReceived;
        protected long totalEventsSent;
        protected long eventsReceived;
        protected long eventsSent;

        public long TotalBytesReceived
        {
            get { return Interlocked.Read(ref totalBytesReceived); }
        }

        public long TotalBytesSent
        {
            get { return Interlocked.Read(ref totalBytesSent); }
        }

        public long BytesReceived
        {
            get { return Interlocked.Read(ref bytesReceived); }
        }

        public long BytesSent
        {
            get { return Interlocked.Read(ref bytesSent); }
        }

        public long TotalEventsReceived
        {
            get { return Interlocked.Read(ref totalEventsReceived); }
        }

        public long TotalEventsSent
        {
            get { return Interlocked.Read(ref totalEventsSent); }
        }

        public long EventsReceived
        {
            get { return Interlocked.Read(ref eventsReceived); }
        }

        public long EventsSent
        {
            get { return Interlocked.Read(ref eventsSent); }
        }

        internal virtual void AddBytesReceived(long bytesReceived)
        {
            Interlocked.Add(ref totalBytesReceived, bytesReceived);
            Interlocked.Add(ref this.bytesReceived, bytesReceived);
        }

        internal virtual void AddBytesSent(long bytesSent)
        {
            Interlocked.Add(ref totalBytesSent, bytesSent);
            Interlocked.Add(ref this.bytesSent, bytesSent);
        }

        public void ResetBytesReceived()
        {
            Interlocked.Exchange(ref bytesReceived, 0L);
        }

        public void ResetBytesSent()
        {
            Interlocked.Exchange(ref bytesSent, 0L);
        }

        internal virtual void IncrementEventsReceived()
        {
            Interlocked.Increment(ref totalEventsReceived);
            Interlocked.Increment(ref eventsReceived);
        }

        internal virtual void IncrementEventsSent()
        {
            Interlocked.Increment(ref totalEventsSent);
            Interlocked.Increment(ref eventsSent);
        }

        public void ResetEventsReceived()
        {
            Interlocked.Exchange(ref eventsReceived, 0L);
        }

        public void ResetEventsSent()
        {
            Interlocked.Exchange(ref eventsSent, 0L);
        }
    }

    #endregion  // Diagnostics
}
