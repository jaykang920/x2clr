// Copyright (c) 2013-2015 Jae-jun Kang
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
        protected const int bufferBlockExponent = 12;

        private static RangedIntPool handlePool;

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

        protected object txSync = new Object();

        protected volatile bool disposed;

        /// <summary>
        /// Gets or sets the BufferTransform for this link session.
        /// </summary>
        public IBufferTransform BufferTransform { get; set; }

        /// <summary>
        /// Gets the link session handle that is unique in the current process.
        /// </summary>
        public int Handle { get { return handle; } }

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

        static LinkSession()
        {
            handlePool = new RangedIntPool(1, 65536, true);  // [1, 65536]
        }

        /// <summary>
        /// Initializes a new instance of the LinkSession class.
        /// </summary>
        protected LinkSession(SessionBasedLink link)
        {
            handle = handlePool.Acquire();
            this.link = link;

            rxBuffer = new Buffer(bufferBlockExponent);
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

            Log.Info("{0} {1} closed", link.Name, handle);

            if (BufferTransform != null)
            {
                BufferTransform.Dispose();
            }

            txBufferList.Clear();
            rxBufferList.Clear();
            rxBuffer.Dispose();

            for (int i = 0, count = buffersSending.Count; i < count; ++i)
            {
                buffersSending[i].Dispose();
            }
            buffersSending.Clear();

            handlePool.Release(handle);

            disposed = true;
        }

        /// <summary>
        /// Sends out the specified event through this link session.
        /// </summary>
        public void Send(Event e)
        {
            if (link == null)
            {
                return;
            }

            lock (txSync)
            {
                eventsToSend.Add(e);

                if (txFlag)
                {
                    return;
                }

                txFlag = true;
            }

            BeginSend();
        }

        internal void BeginReceive(bool beginning)
        {
            rxBeginning = beginning;

            ReceiveInternal();
        }

        internal void BeginSend()
        {
            lock (txSync)
            {
                if (eventsToSend.Count == 0)
                {
                    return;
                }
                // swap buffer
                eventsSending.Clear();
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

                uint header = 0;
                if (BufferTransform != null && txTransformReady && e._Transform)
                {
                    BufferTransform.Transform(sendBuffer.Buffer, (int)sendBuffer.Buffer.Length);
                    header = 1;
                }
                header |= ((uint)sendBuffer.Buffer.Length << 1);

                sendBuffer.HeaderLength = Serializer.WriteVariable(sendBuffer.HeaderBytes, header);

                sendBuffer.ListOccupiedSegments(txBufferList);
                lengthToSend += sendBuffer.Length;

                LogEventSent(e);
            }

            SendInternal();
        }

        protected abstract void ReceiveInternal();
        protected abstract void SendInternal();

        protected void OnReceiveInternal(int bytesTransferred)
        {
            Diag.AddBytesReceived(bytesTransferred);

            Log.Trace("{0} {1} received {2} byte(s)",
                link.Name, Handle, bytesTransferred);

            rxBuffer.Stretch(bytesTransferred);

            if (rxBeginning)
            {
                rxBuffer.Position = 0;
                uint header;
                int headerLength;
                try
                {
                    headerLength = rxBuffer.ReadVariable(out header);
                }
                catch (System.IO.EndOfStreamException)
                {
                    // Need more to start parsing.
                    BeginReceive(true);
                    return;
                }
                rxBuffer.Shrink(headerLength);
                lengthToReceive = (int)(header >> 1);
                rxTransformed = ((header & 1) != 0);
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

                Log.Trace("{0} {1} marked {2} byte(s) to read", link.Name, Handle, lengthToReceive);

                if (BufferTransform != null && rxTransformReady && rxTransformed)
                {
                    try
                    {
                        BufferTransform.InverseTransform(rxBuffer, lengthToReceive);
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} {1} buffer transform error: {2}", link.Name, Handle, e.Message);
                        goto next;
                    }
                }
                rxBuffer.Position = 0;

                var deserializer = new Deserializer(rxBuffer);
                Event retrieved = EventFactory.Create(deserializer);
                if (retrieved == null)
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
                        Log.Error("{0} {1} error loading event {2}: {3}", link.Name, Handle, retrieved.GetTypeId(), e.ToString());
                        goto next;
                    }

                    retrieved._Handle = Handle;

                    link.OnPreprocess(this, retrieved);

                    LogEventReceived(retrieved);

                    if (!Process(retrieved))
                    {
                        Hub.Post(retrieved);
                    }
                }
            next:
                rxBuffer.Trim();
                if (rxBuffer.IsEmpty)
                {
                    break;
                }

                uint header;
                int headerLength;
                try
                {
                    headerLength = rxBuffer.ReadVariable(out header);
                }
                catch (System.IO.EndOfStreamException)
                {
                    BeginReceive(true);
                    return;
                }
                rxBuffer.Shrink(headerLength);
                lengthToReceive = (int)(header >> 1);
                rxTransformed = ((header & 1) != 0);

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
            Link.NotifySessionDisconnected(handle, context);

            Close();
        }

        protected void OnSendInternal(int bytesTransferred)
        {
            Diag.AddBytesSent(bytesTransferred);

            Log.Trace("{0} {1} sent {2}/{3} byte(s)",
                link.Name, Handle, bytesTransferred, lengthToSend);

            lock (txSync)
            {
                if (eventsToSend.Count == 0)
                {
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
                            response = BufferTransform.Handshake(req.Data);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("{0} {1} error handshaking : {2}",
                                link.Name, Handle, ex.ToString());
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
                                link.Name, Handle, ex.ToString());
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

                        Link.NotifySessionConnected(result, (result ? this : null));
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        protected virtual void LogEventReceived(Event e)
        {
            Diag.IncrementEventsReceived();
        }

        protected virtual void LogEventSent(Event e)
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
