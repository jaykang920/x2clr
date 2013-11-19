// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    public class Binder
    {
        private readonly Dictionary<Event, HandlerSet> handlerMap;
        private readonly Filter filter;

        public Binder()
        {
            handlerMap = new Dictionary<Event, HandlerSet>();
            filter = new Filter();
            Diag = new Diagnostics(this);
        }

        public virtual Token Bind(Event e, IHandler handler)
        {
            filter.Add(e.GetTypeId(), e.GetFingerprint());
            HandlerSet handlers;
            if (!handlerMap.TryGetValue(e, out handlers))
            {
                handlers = new HandlerSet();
                handlerMap.Add(e, handlers);
            }
            handlers.Add(handler);

            var token = new Token(e, handler);
            if (handler.Action.Target is EventSink)
            {
                var eventSink = (EventSink)handler.Action.Target;
                eventSink.AddBinding(token);
            }
            return token;
        }

        public virtual int BuildHandlerChain(Event e, List<IHandler> handlerChain)
        {
            Event.Tag tag = (Event.Tag)e.GetTypeTag();
            Fingerprint fingerprint = e.GetFingerprint();
            while (tag != null)
            {
                int typeId = tag.TypeId;
                IEnumerable<Slot> slots = filter.Get(typeId);
                if (slots != null)
                {
                    foreach (Fingerprint slot in slots)
                    {
                        if (slot.IsEquivalent(fingerprint))
                        {
                            EventEquivalent equivalent = new EventEquivalent(e, slot, typeId);
                            HandlerSet handlers;
                            if (handlerMap.TryGetValue(equivalent, out handlers))
                            {
                                handlerChain.AddRange(handlers.GetEnumerable());
                            }
                        }
                    }
                }
                tag = (Event.Tag)tag.Base;
            }
            // sort result
            return handlerChain.Count;
        }

        public virtual void Unbind(Event e, IHandler handler)
        {
            UnbindInternal(e, handler);

            var token = new Token(e, handler);
            if (handler.Action.Target is EventSink)
            {
                var eventSink = (EventSink)handler.Action.Target;
                eventSink.RemoveBinding(token);
            }
        }

        public void Unbind(Token token)
        {
            UnbindInternal(token.key, token.value);
        }

        private void UnbindInternal(Event e, IHandler handler)
        {
            HandlerSet handlers;
            if (handlerMap.TryGetValue(e, out handlers))
            {
                if (handlers.Remove(handler))
                {
                    handlerMap.Remove(e);
                }
            }
            filter.Remove(e.GetTypeId(), e.GetFingerprint());
        }

        public struct Token
        {
            public Event key;
            public IHandler value;

            public override bool Equals(object obj)
            {
                if (!(obj is Token))
                {
                    return false;
                }

                Token other = (Token)obj;
                if (!key.Equals(other.key) || !value.Equals(other.value))
                {
                    return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                Hash hash = new Hash(Hash.Seed);
                hash.Update(key.GetHashCode());
                hash.Update(value.GetHashCode());
                return hash.Code;
            }

            public Token(Event key, IHandler value)
            {
                this.key = key;
                this.value = value;
            }

            public static bool operator ==(Token x, Token y)
            {
                return x.Equals(y);
            }

            public static bool operator !=(Token x, Token y)
            {
                return !x.Equals(y);
            }
        }

        private class Filter
        {
            private readonly Dictionary<int, List<Slot>> map;

            internal Filter()
            {
                map = new Dictionary<int, List<Slot>>();
            }

            internal void Add(int typeId, Fingerprint fingerprint)
            {
                List<Slot> slots;
                if (map.TryGetValue(typeId, out slots) == false)
                {
                    slots = new List<Slot>();
                    map.Add(typeId, slots);
                }
                Slot slot = new Slot(fingerprint);
                int index = slots.BinarySearch(slot);
                if (index >= 0)
                {
                    slots[index].IncrementRefCount();
                }
                else
                {
                    index = ~index;
                    slots.Insert(index, slot);
                }
            }

            internal IEnumerable<Slot> Get(int typeId)
            {
                List<Slot> slots;
                map.TryGetValue(typeId, out slots);
                return slots;
            }

            internal void Remove(int typeId, Fingerprint fingerprint)
            {
                List<Slot> slots;
                if (map.TryGetValue(typeId, out slots) == false)
                {
                    return;
                }
                int index = slots.BinarySearch(new Slot(fingerprint));
                if (index >= 0)
                {
                    if (slots[index].DecrementRefCount() == 0)
                    {
                        slots.RemoveAt(index);
                    }
                    if (slots.Count == 0)
                    {
                        map.Remove(typeId);
                    }
                }
            }
        }

        private class HandlerSet
        {
            private readonly List<IHandler> handlers;

            public HandlerSet()
            {
                handlers = new List<IHandler>();
            }

            public void Add(IHandler handler)
            {
                handlers.Add(handler);
            }

            public IEnumerable<IHandler> GetEnumerable()
            {
                return handlers;
            }

            public bool Remove(IHandler handler)
            {
                handlers.Remove(handler);
                return (handlers.Count == 0);
            }
        }

        #region Diagnostics

        /// <summary>
        /// Gets the diagnostics object.
        /// </summary>
        public Diagnostics Diag { get; private set; }

        /// <summary>
        /// Internal diagnostics helper class.
        /// </summary>
        public class Diagnostics
        {
            private readonly Binder owner;

            internal Diagnostics(Binder owner)
            {
                this.owner = owner;
            }
        }

        #endregion
    }

    public class SynchronizedBinding : Binder
    {
        private readonly ReaderWriterLock rwlock;

        public SynchronizedBinding()
        {
            rwlock = new ReaderWriterLock();
        }

        public override Token Bind(Event e, IHandler handler)
        {
            rwlock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return base.Bind(e, handler);
            }
            finally
            {
                rwlock.ReleaseWriterLock();
            }
        }

        public override int BuildHandlerChain(Event e, List<IHandler> handlerChain)
        {
            rwlock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return base.BuildHandlerChain(e, handlerChain);
            }
            finally
            {
                rwlock.ReleaseReaderLock();
            }
        }

        public override void Unbind(Event e, IHandler handler)
        {
            rwlock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                base.Unbind(e, handler);
            }
            finally
            {
                rwlock.ReleaseWriterLock();
            }
        }
    }

    /// <summary>
    /// Extends Fingerprint class to hold an additional reference count.
    /// </summary>
    internal class Slot : Fingerprint, IComparable<Slot>
    {
        private int refCount;

        /// <summary>
        /// Initializes a new instance of the Slot class that contains bit values
        /// copied from the specified Fingerprint.
        /// </summary>
        /// <param name="fingerprint">A Fingerprint object to copy from.</param>
        public Slot(Fingerprint fingerprint)
            : base(fingerprint)
        {
            refCount = 1;
        }

        /// <summary>
        /// Increases the reference count of this Slot.
        /// </summary>
        /// <returns>The resultant reference count.</returns>
        public int IncrementRefCount()
        {
            return Interlocked.Increment(ref refCount);
        }

        /// <summary>
        /// Compares this Slot with the specified Slot object.
        /// </summary>
        /// Implements IComparable(T).CompareTo interface.
        /// <param name="other">
        /// A Slot object to be compared with this.
        /// </param>
        /// <returns>
        /// A value that indicates the relative order of the Slot objects being
        /// compared. Zero return value means that this is equal to <c>other</c>,
        /// while negative(positive) integer return value means that this is
        /// less(greater) than <c>other</c>.
        /// </returns>
        public int CompareTo(Slot other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Decreases the reference count of this Slot.
        /// </summary>
        /// <returns>The resultant reference count.</returns>
        public int DecrementRefCount()
        {
            return Interlocked.Decrement(ref refCount);
        }
    }
}
