// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Abstract base class for concrete link sessions.
    /// </summary>
    public abstract class LinkSession : IDisposable
    {
        protected int handle;
        protected SessionBasedLink link;
        protected bool polarity;

        protected Buffer rxBuffer;
        protected List<ArraySegment<byte>> rxBufferList;
        protected List<ArraySegment<byte>> txBufferList;

        protected List<Event> eventsSending;
        protected List<Event> eventsToSend;
        protected List<SendBuffer> buffersSending;

        protected int lengthToReceive;
        protected int lengthToSend;

        protected bool rxBeginning;

        protected bool rxTransformed;
        protected bool rxTransformReady;
        protected bool txTransformReady;

        protected bool txFlag;

        protected object syncRoot = new Object();

        protected volatile bool closing;
        protected volatile bool disposed;

        protected int rxCounter;
        protected int txCounter;

        /// <summary>
        /// Gets or sets the BufferTransform for this link session.
        /// </summary>
        public IBufferTransform BufferTransform { get; set; }

        /// <summary>
        /// Gets or sets the context object associated with this link session.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Gets or sets the link session handle that is unique in the current
        /// process.
        /// </summary>
        public int Handle
        {
            get { return handle; }
            set { handle = value; }
        }

        /// <summary>
        /// Gets the link associated with this session.
        /// </summary>
        public SessionBasedLink Link { get { return link; } }

        /// <summary>
        /// Gets or sets a boolean value indicating whether this session is an
        /// active (client) session. A passive (server) session will return false.
        /// </summary>
        public bool Polarity
        {
            get { return polarity; }
            set { polarity = value; }
        }

        /// <summary>
        /// Gets or sets the session token for this session.
        /// </summary>
        public string Token { get; set; }

        internal virtual int InternalHandle { get { return handle; } }

        internal object SyncRoot { get { return syncRoot; } }

        /// <summary>
        /// Initializes a new instance of the LinkSession class.
        /// </summary>
        protected LinkSession(SessionBasedLink link)
        {
            this.link = link;

            rxBuffer = new Buffer();
            rxBufferList = new List<ArraySegment<byte>>();
            txBufferList = new List<ArraySegment<byte>>();

            eventsSending = new List<Event>();
            eventsToSend = new List<Event>();
            buffersSending = new List<SendBuffer>();

            Diag = new Diagnostics(this);
        }

        ~LinkSession()
        {
            Dispose(false);
        }

        /// <summary>
        /// Actively closes this link session and releases all the associated
        /// resources.
        /// </summary>
        public virtual void Close()
        {
            closing = true;

            if (link.SessionRecoveryEnabled)
            {
                Send(new SessionEnd { _Transform = false });
            }

            OnClose();

            CloseInternal();
        }

        protected abstract void OnClose();

        protected internal void CloseInternal()
        {
            Dispose();
        }

        /// <summary>
        /// Implements IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            lock (syncRoot)
            {
                Dispose(true);
            }
            GC.SuppressFinalize(this);
        }

        public void Release()
        {
            if (!Object.ReferenceEquals(BufferTransform, null))
            {
                BufferTransform.Dispose();
                BufferTransform = null;
            }
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) { return; }

            disposed = true;

            Log.Info("{0} closed {1}", link.Name, this);

            if (link.SessionRecoveryEnabled == false &&
                !Object.ReferenceEquals(BufferTransform, null))
            {
                BufferTransform.Dispose();
                BufferTransform = null;
            }

            txBufferList.Clear();
            rxBufferList.Clear();
            rxBuffer.Dispose();

            for (int i = 0, count = buffersSending.Count; i < count; ++i)
            {
                buffersSending[i].Dispose();
            }
            buffersSending.Clear();
        }

        /// <summary>
        /// Sends out the specified event through this link session.
        /// </summary>
        public void Send(Event e)
        {
            if (disposed)
            {
                if (link.SessionRecoveryEnabled)
                {
                    Log.Debug("{0} {1} buffered {2}", link.Name, handle, e);
                }
                else
                {
                    Log.Warn("{0} {1} dropped {2}", link.Name, handle, e);
                    return;
                }
            }

            lock (syncRoot)
            {
                eventsToSend.Add(e);

                if (disposed || txFlag)
                {
                    return;
                }

                txFlag = true;
            }

            BeginSend();
        }

        internal void InheritFrom(LinkSession oldSession)
        {
            lock (syncRoot)
            {
                handle = oldSession.Handle;
                Token = oldSession.Token;

                Log.Debug("{0} {1} session inheritance {2}", link.Name, handle, Token);

                BufferTransform = oldSession.BufferTransform;
                rxTransformReady = oldSession.rxTransformReady;
                txTransformReady = oldSession.txTransformReady;

                lock (oldSession.syncRoot)
                {
                    if (oldSession.eventsToSend.Count != 0)
                    {
                        eventsToSend.AddRange(oldSession.eventsToSend);
                        oldSession.eventsToSend.Clear();
                    }
                }
            }
        }

        public void TakeOver(LinkSession oldSession)
        {
            lock (syncRoot)
            {
                lock (oldSession.syncRoot)
                {
                    if (oldSession.eventsToSend.Count != 0)
                    {
                        eventsToSend.AddRange(oldSession.eventsToSend);
                        oldSession.eventsToSend.Clear();
                    }
                }
                if (eventsToSend.Count != 0)
                {
                    txFlag = true;
                    BeginSend();
                }
            }
        }

        internal void BeginReceive(bool beginning)
        {
            rxBeginning = beginning;

            ReceiveInternal();
        }

        internal void BeginSend()
        {
            lock (syncRoot)
            {
                if (eventsToSend.Count == 0)
                {
                    return;
                }
                // swap buffer
                if (eventsSending.Count != 0)
                {
                    eventsSending.Clear();
                }
                List<Event> temp = eventsSending;
                eventsSending = eventsToSend;
                eventsToSend = temp;
                temp = null;
            }

            // capture send buffer
            txBufferList.Clear();
            lengthToSend = 0;
            int count = eventsSending.Count;
            int bufferCount = buffersSending.Count;
            if (bufferCount < count)
            {
                for (int i = 0, n = count - bufferCount; i < n; ++i)
                {
                    buffersSending.Add(new SendBuffer());
                }
            }
            else
            {
                for (int i = 0, n = bufferCount - count; i < n; ++i)
                {
                    int j = bufferCount - (i + 1);
                    buffersSending[j].Dispose();
                    buffersSending.RemoveAt(j);
                }
            }
            for (int i = 0; i < count; ++i)
            {
                Event e = eventsSending[i];

                var sendBuffer = buffersSending[i];
                sendBuffer.Reset();
                e.Serialize(new Serializer(sendBuffer.Buffer));

                bool transformed = false;
                if (BufferTransform != null && txTransformReady && e._Transform)
                {
                    BufferTransform.Transform(sendBuffer.Buffer, (int)sendBuffer.Buffer.Length);
                    transformed = true;
                }

                Interlocked.Increment(ref txCounter);

                BuildHeader(sendBuffer, transformed);

                sendBuffer.ListOccupiedSegments(txBufferList);
                lengthToSend += sendBuffer.Length;

                OnEventSent(e);
            }

            SendInternal();
        }

        protected abstract void BuildHeader(SendBuffer sendBuffer, bool transformed);
        protected abstract bool ParseHeader();

        protected abstract void ReceiveInternal();
        protected abstract void SendInternal();

        protected void OnReceiveInternal(int bytesTransferred)
        {
            Diag.AddBytesReceived(bytesTransferred);

            Log.Trace("{0} {1} received {2} byte(s)",
                link.Name, InternalHandle, bytesTransferred);

            if (disposed)
            {
                return;
            }

            rxBuffer.Stretch(bytesTransferred);

            if (rxBeginning)
            {
                rxBuffer.Rewind();

                if (!ParseHeader())
                {
                    BeginReceive(true);
                    return;
                }
            }

            // Handle split packets.
            if (rxBuffer.Length < lengthToReceive)
            {
                BeginReceive(false);
                return;
            }

            while (true)
            {
                rxBuffer.MarkToRead(lengthToReceive);

                Log.Trace("{0} {1} marked {2} byte(s) to read",
                    link.Name, InternalHandle, lengthToReceive);

                if (BufferTransform != null && rxTransformReady && rxTransformed)
                {
                    try
                    {
                        BufferTransform.InverseTransform(rxBuffer, lengthToReceive);
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} {1} buffer inv transform error: {2}",
                            link.Name, InternalHandle, e.Message);
                        goto next;
                    }
                }
                rxBuffer.Rewind();

                Interlocked.Increment(ref rxCounter);

                var deserializer = new Deserializer(rxBuffer);
                Event retrieved = EventFactory.Create(deserializer);
                if ((object)retrieved == null)
                {
                    goto next;
                }
                else
                {
                    try
                    {
                        retrieved.Deserialize(deserializer);
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} {1} error loading event {2}: {3}",
                            link.Name, InternalHandle, retrieved.GetTypeId(), e.ToString());
                        goto next;
                    }

                    OnEventReceived(retrieved);

                    if (!Process(retrieved))
                    {
                        retrieved._Handle = Handle;

                        link.OnPreprocess(this, retrieved);

                        Hub.Post(retrieved);
                    }
                }
            next:
                rxBuffer.Trim();
                if (rxBuffer.IsEmpty)
                {
                    break;
                }

                if (!ParseHeader())
                {
                    BeginReceive(true);
                    return;
                }

                if (rxBuffer.Length < lengthToReceive)
                {
                    BeginReceive(false);
                    return;
                }
            }

            BeginReceive(true);
        }

        protected void OnDisconnect(object context)
        {
            Log.Debug("{0} {1} OnDisconnect", link.Name, InternalHandle);

            if (handle <= 0)
            {
                return;
            }

            if (link.SessionRecoveryEnabled && !closing)
            {
                lock (syncRoot)
                {
                    link.OnInstantDisconnect(this);
                }
            }
            else
            {
                link.OnLinkSessionDisconnectedInternal(handle, context);
            }
        }

        protected void OnSendInternal(int bytesTransferred)
        {
            Diag.AddBytesSent(bytesTransferred);

            Log.Trace("{0} {1} sent {2}/{3} byte(s)",
                link.Name, InternalHandle, bytesTransferred, lengthToSend);

            lock (syncRoot)
            {
                if (disposed)
                {
                    txFlag = false;
                    return;
                }
                if (eventsToSend.Count == 0)
                {
                    eventsSending.Clear();
                    txFlag = false;
                    return;
                }
            }

            BeginSend();
        }

        protected virtual bool Process(Event e)
        {
            switch (e.GetTypeId())
            {
                case (int)LinkEventType.HandshakeReq:
                    {
                        var req = (HandshakeReq)e;
                        var resp = new HandshakeResp { _Transform = false };
                        byte[] response = null;
                        try
                        {
                            ManualResetEvent waitHandle =
                                LinkWaitHandlePool.Acquire(InternalHandle);
                            waitHandle.WaitOne(new TimeSpan(0, 0, 30));
                            LinkWaitHandlePool.Release(InternalHandle);
                            response = BufferTransform.Handshake(req.Data);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("{0} {1} error handshaking : {2}",
                                link.Name, InternalHandle, ex.ToString());
                        }
                        if (response != null)
                        {
                            resp.Data = response;
                        }
                        Send(resp);
                    }
                    break;
                case (int)LinkEventType.HandshakeResp:
                    {
                        var ack = new HandshakeAck { _Transform = false };
                        var resp = (HandshakeResp)e;
                        try
                        {
                            if (BufferTransform.FinalizeHandshake(resp.Data))
                            {
                                rxTransformReady = true;
                                ack.Result = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("{0} {1} error finishing handshake : {2}",
                                link.Name, InternalHandle, ex.ToString());
                        }
                        Send(ack);
                    }
                    break;
                case (int)LinkEventType.HandshakeAck:
                    {
                        var ack = (HandshakeAck)e;
                        bool result = ack.Result;

                        if (result)
                        {
                            txTransformReady = true;
                        }

                        link.OnLinkSessionConnectedInternal(result, (result ? this : null));
                    }
                    break;
                case (int)LinkEventType.SessionReq:
                    if (link.SessionRecoveryEnabled && polarity == false)
                    {
                        var server = (ServerLink)link;
                        server.OnSessionReq(this, (SessionReq)e);
                    }
                    break;
                case (int)LinkEventType.SessionResp:
                    if (link.SessionRecoveryEnabled && polarity == true)
                    {
                        var client = (ClientLink)link;
                        client.OnSessionResp(this, (SessionResp)e);
                    }
                    break;
                case (int)LinkEventType.SessionAck:
                    if (link.SessionRecoveryEnabled && polarity == false)
                    {
                        var server = (ServerLink)link;
                        server.OnSessionAck(this, (SessionAck)e);
                    }
                    break;
                case (int)LinkEventType.SessionEnd:
                    if (link.SessionRecoveryEnabled)
                    {
                        closing = true;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        protected virtual void OnEventReceived(Event e)
        {
            Diag.IncrementEventsReceived();
        }

        protected virtual void OnEventSent(Event e)
        {
            Diag.IncrementEventsSent();
        }

        #region Diagnostics

        /// <summary>
        /// Gets or sets the diagnostics object.
        /// </summary>
        public Diagnostics Diag { get; set; }

        /// <summary>
        /// Link session diagnostics helper class.
        /// </summary>
        public class Diagnostics : Link.Diagnostics
        {
            protected LinkSession owner;

            public Diagnostics(LinkSession owner)
            {
                this.owner = owner;
            }

            internal override void AddBytesReceived(long bytesReceived)
            {
                base.AddBytesReceived(bytesReceived);

                if (owner.Link != null)
                {
                    owner.Link.Diag.AddBytesReceived(bytesReceived);
                }
            }

            internal override void AddBytesSent(long bytesSent)
            {
                base.AddBytesSent(bytesSent);

                if (owner.Link != null)
                {
                    owner.Link.Diag.AddBytesSent(bytesSent);
                }
            }

            internal override void IncrementEventsReceived()
            {
                base.IncrementEventsReceived();

                if (owner.Link != null)
                {
                    owner.Link.Diag.IncrementEventsReceived();
                }
            }

            internal override void IncrementEventsSent()
            {
                base.IncrementEventsSent();

                if (owner.Link != null)
                {
                    owner.Link.Diag.IncrementEventsSent();
                }
            }
        }

        #endregion  // Diagnostics
    }
}
