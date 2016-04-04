﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Abstract base class for non-reliable UDP/IP links.
    /// </summary>
    public abstract class AbstractUdpLink : Link
    {
        protected SortedList<int, EndPoint> map;
        protected Dictionary<EndPoint, int> reverseMap;

        protected Socket socket;

        protected Buffer rxBuffer;
        protected Buffer txBuffer;

        protected Queue<Event> txQueue;
        protected bool txFlag;

        protected ReaderWriterLockSlim rwlock;
        protected object syncRoot = new Object();

        /// <summary>
        /// Initializes a new instance of the AbsractUdpLink class.
        /// </summary>
        protected AbstractUdpLink(string name)
            : base(name)
        {
            map = new SortedList<int, EndPoint>();
            reverseMap = new Dictionary<EndPoint, int>();

            socket = new Socket(IPAddress.Any.AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);

            rxBuffer = new Buffer();
            txBuffer = new Buffer();

            txQueue = new Queue<Event>();

            rwlock = new ReaderWriterLockSlim();

            Diag = new Diagnostics();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            try
            {
                socket.Close();

                rxBuffer.Dispose();
                txBuffer.Dispose();

                rwlock.Dispose();
            }
            catch (Exception e)
            {
                Log.Error("{0} error disposing: {0}", e);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Binds the underlying socket with the specified local port.
        /// </summary>
        public AbstractUdpLink Bind(int localPort)
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, localPort));
            return this;
        }

        /// <summary>
        /// Begin receiving incoming events on this link.
        /// </summary>
        public void Listen()
        {
            if (!socket.IsBound)
            {
                throw new InvalidOperationException();
            }

            socket.SetSocketOption(SocketOptionLevel.IP,
                SocketOptionName.PacketInformation, true);

            BeginReceiveFrom();
        }

        /// <summary>
        /// Adds the specified remote end point to the known peers, and returns
        /// the handle associated with it.
        /// </summary>
        public int AddEndPoint(EndPoint endPoint)
        {
            int handle;
            using (new UpgradeableReadLock(rwlock))
            {
                if (!reverseMap.TryGetValue(endPoint, out handle))
                {
                    handle = HandlePool.Acquire();
                    using (new WriteLock(rwlock))
                    {
                        map.Add(handle, endPoint);
                        reverseMap.Add(endPoint, handle);
                    }
                }
            }
            return handle;
        }

        /// <summary>
        /// Gets the remote end point of the known peer identified by the
        /// specified handle.
        /// </summary>
        public EndPoint GetEndPoint(int handle)
        {
            using (new ReadLock(rwlock))
            {
                EndPoint result;
                if (map.TryGetValue(handle, out result))
                {
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the handle of the known peer associated with the specified
        /// remote end point.
        /// </summary>
        public int GetHandle(EndPoint endPoint)
        {
            using (new ReadLock(rwlock))
            {
                int handle;
                if (reverseMap.TryGetValue(endPoint, out handle))
                {
                    return handle;
                }
            }
            return 0;
        }

        /// <summary>
        /// Removes the specified remote end point from the known peers.
        /// </summary>
        public void RemoveEndPoint(EndPoint endPoint)
        {
            int handle;
            using (new UpgradeableReadLock(rwlock))
            {
                if (!reverseMap.TryGetValue(endPoint, out handle))
                {
                    return;
                }
                using (new WriteLock(rwlock))
                {
                    map.Remove(handle);
                    reverseMap.Remove(endPoint);
                }
            }
            HandlePool.Release(handle);
        }


        /// <summary>
        /// Removes the specified handle from the known peers.
        /// </summary>
        public void RemoveHandle(int handle)
        {
            using (new UpgradeableReadLock(rwlock))
            {
                EndPoint endPoint;
                if (!map.TryGetValue(handle, out endPoint))
                {
                    return;
                }
                using (new WriteLock(rwlock))
                {
                    map.Remove(handle);
                    reverseMap.Remove(endPoint);
                }
            }
            HandlePool.Release(handle);
        }

        /// <summary>
        /// Sends out the specified event to a known peer.
        /// </summary>
        public override void Send(Event e)
        {
            lock (syncRoot)
            {
                if (txFlag)
                {
                    txQueue.Enqueue(e);
                    return;
                }
                txFlag = true;
            }

            BeginSendTo(e);
        }

        protected void BeginReceiveFrom()
        {
            rxBuffer.Reset();

            try
            {
                ReceiveFromInternal();
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                Log.Warn("{0} recv error {1}", Name, e);
                BeginReceiveFrom();
            }
        }

        private void BeginSendTo(Event e)
        {
            int handle = e._Handle;

            EndPoint endPoint;
            using (new ReadLock(rwlock))
            {
                int count = map.Count;

                if (count == 0)
                {
                    Log.Error("{0} no known peers - dropped event {1}", Name, e);
                    goto next;
                }

                if (count == 1 && handle == 0)
                {
                    endPoint = map.Values[0];
                }
                else
                {
                    if (!map.TryGetValue(handle, out endPoint))
                    {
                        Log.Error("{0} unknown handle {1} - dropped event {2}",
                            Name, handle, e);
                        goto next;
                    }
                }
            }

            // Apply the datagram length limit.
            int length = e.GetLength();
            if (length > txBuffer.BlockSize)
            {
                Log.Error("{0} dropped big event {1}", Name, e);
                goto next;
            }

            txBuffer.Reset();
            e.Serialize(new Serializer(txBuffer));

            if (BufferTransform != null)
            {
                BufferTransform.Transform(txBuffer, (int)txBuffer.Length);
            }

            try
            {
                SendToInternal(endPoint);

                Diag.IncrementEventsSent();

                Log.Debug("{0} {1} sent event {2}", Name, handle, e);

                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
            {
                Log.Info("{0} send error {1}", Name, ex);
            }

        next:
            OnSendToInternal(0);
        }

        protected abstract void ReceiveFromInternal();
        protected abstract void SendToInternal(EndPoint endPoint);

        protected void OnReceiveFromInternal(int bytesTransferred, EndPoint endPoint)
        {
            Diag.AddBytesReceived(bytesTransferred);

            int handle;
            using (new ReadLock(rwlock))
            {
                if (!reverseMap.TryGetValue(endPoint, out handle))
                {
                    handle = 0;
                }
            }

            rxBuffer.Stretch(bytesTransferred);
            if (BufferTransform != null)
            {
                try
                {
                    BufferTransform.InverseTransform(rxBuffer, (int)rxBuffer.Length);
                }
                catch (Exception e)
                {
                    Log.Error("{0} {1} buffer inv transform error: {2}", Name, handle, e.Message);
                    return;
                }
            }
            rxBuffer.Rewind();

            var deserializer = new Deserializer(rxBuffer);
            Event retrieved = EventFactory.Create(deserializer);
            if (retrieved == null)
            {
                return;
            }
            else
            {
                try
                {
                    retrieved.Deserialize(deserializer);
                }
                catch (Exception e)
                {
                    Log.Error("{0} {1} error loading event {2}: {3}", Name, handle, retrieved.GetTypeId(), e.ToString());
                    return;
                }

                if (handle != 0)
                {
                    retrieved._Handle = handle;
                }

                Hub.Post(retrieved);

                Diag.IncrementEventsReceived();

                Log.Debug("{0} {1} received event {2}", Name, handle, retrieved);
            }
        }

        protected void OnSendToInternal(int bytesTransferred)
        {
            if (bytesTransferred != 0)
            {
                Diag.AddBytesSent(bytesTransferred);
            }

            Event e;
            lock (syncRoot)
            {
                if (txQueue.Count == 0)
                {
                    txFlag = false;
                    return;
                }
                e = txQueue.Dequeue();
            }

            BeginSendTo(e);
        }
    }
}
