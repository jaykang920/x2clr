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
        }

        public void BindGeneric1<T>(T e, Action<T> handler)
            where T : Event
        {
            Bind(e, new Handler<T>(handler));
        }

        public void BindGeneric2<T, U>(T e, Action<U> handler)
            where T : Event
            where U : Event
        {
            Bind(e, new Handler<U>(handler));
        }

        public virtual void Bind(Event e, IHandler handler)
        {
            filter.Add(e.GetTypeId(), e.GetFingerprint());
            HandlerSet handlers;
            if (!handlerMap.TryGetValue(e, out handlers))
            {
                handlers = new HandlerSet();
                handlerMap.Add(e, handlers);
            }
            handlers.Add(handler);
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
                            EventEquivalent equivalent = new EventEquivalent(e, slot);
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
                int index = handlers.BinarySearch(handler);
                if (index >= 0)
                {
                    //handlers[index].Combine(handler);
                }
                else
                {
                    index = ~index;
                    handlers.Insert(index, handler);
                }
            }

            public IEnumerable<IHandler> GetEnumerable()
            {
                return handlers;
            }

            public bool Remove(IHandler handler)
            {
                int index = handlers.BinarySearch(handler);
                if (index >= 0)
                {
                    //if (handlers[index].Remove(handler))
                    //{
                        handlers.RemoveAt(index);
                    //}
                }
                return (handlers.Count == 0);
            }
        }
    }

    public class SynchronizedBinding : Binder
    {
        private readonly ReaderWriterLock rwlock;

        public SynchronizedBinding()
        {
            rwlock = new ReaderWriterLock();
        }

        public override void Bind(Event e, IHandler handler)
        {
            rwlock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                base.Bind(e, handler);
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
}
