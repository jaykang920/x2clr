// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2.Events;
using x2.Queues;

namespace x2.Flows
{
    public class PriorityQueue<TPriority, TItem>
    {
        private readonly SortedDictionary<TPriority, List<TItem>> store;

        public int Count { get { return store.Count; } }

        public PriorityQueue()
        {
            store = new SortedDictionary<TPriority, List<TItem>>();
        }

        public void Enqueue(TPriority priority, TItem item)
        {
            List<TItem> items;
            if (!store.TryGetValue(priority, out items))
            {
                items = new List<TItem>();
                store.Add(priority, items);
            }
            items.Add(item);
        }

        public TItem Dequeue()
        {
            var first = First();
            var items = first.Value;
            TItem item = items[0];
            items.RemoveAt(0);
            if (items.Count == 0)
            {
                store.Remove(first.Key);
            }
            return item;
        }

        public List<TItem> DequeueBundle()
        {
            var first = First();
            store.Remove(first.Key);
            return first.Value;
        }

        public TPriority Peek()
        {
            var first = First();
            return first.Key;
        }

        public void Remove(TPriority priority, TItem item)
        {
            List<TItem> items;
            if (store.TryGetValue(priority, out items))
            {
                items.Remove(item);
                if (items.Count == 0)
                {
                    store.Remove(priority);
                }
            }
        }

        // First() extension method workaround to clearly support .NEt 2.0
        private KeyValuePair<TPriority, List<TItem>> First()
        {
            using (var enumerator = store.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

    // TODO: Repeated occurrence, scheduling, canceling, time-scale factor
    public sealed class TimeFlow : FrameBasedFlow
    {
        private const string defaultName = "default";

        private static readonly Map map = new Map();

        private readonly string name;
        private readonly PriorityQueue<DateTime, Event> reserved;
        private readonly Generator generator;

        /// <summary>
        /// Gets the default(anonymous) TimeFlow.
        /// </summary>
        public static TimeFlow Default { get { return Get(); } }

        /// <summary>
        /// Gets the name of this TimeFlow.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private TimeFlow(string name)
        {
            this.name = name;
            reserved = new PriorityQueue<DateTime, Event>();
            generator = new Generator(this);
        }

        /// <summary>
        /// Creates a default(anonymous) TimeFlow.
        /// </summary>
        public static TimeFlow Create()
        {
            return Create(defaultName);
        }

        /// <summary>
        /// Creates a named TimeFlow.
        /// </summary>
        public static TimeFlow Create(string name)
        {
            if (name == null)
            {
                throw new NullReferenceException();
            }
            return map.Create(name);
        }

        /// <summary>
        /// Gets the default(anonymous) TimeFlow.
        /// </summary>
        public static TimeFlow Get()
        {
            return Get(defaultName);
        }

        /// <summary>
        /// Gets the named TimeFlow.
        /// </summary>
        public static TimeFlow Get(string name)
        {
            if (name == null)
            {
                throw new NullReferenceException();
            }
            return map.Get(name);
        }

        public Token Reserve(Event e, float seconds)
        {
            return Reserve(e, DateTime.Now.AddSeconds(seconds));
        }
        
        public Token Reserve(Event e, TimeSpan delay)
        {
            return Reserve(e, DateTime.Now.Add(delay));
        }

        public Token Reserve(Event e, DateTime when)
        {
            lock (reserved)
            {
                reserved.Enqueue(when, e);
            }
            return new Token(when, e);
        }

        public void Cancel(Token token)
        {
            lock (reserved)
            {
                reserved.Remove(token.key, token.value);
            }
        }

        public void ReserveRepetition(Event e, TimeSpan interval)
        {
            generator.Add(e, new TimeTag(interval));
        }

        public void ReserveRepetition(Event e, DateTime nextTime, TimeSpan interval)
        {
            generator.Add(e, new TimeTag(nextTime, interval));
        }

        public void CancelRepetition(Event e)
        {
            generator.Remove(e);
        }

        protected override void Start() { }
        protected override void Stop() { }

        protected override void Update()
        {
            DateTime now = DateTime.Now;
            List<Event> events = null;
            lock (reserved)
            {
                if (reserved.Count != 0)
                {
                    DateTime next = reserved.Peek();
                    if (now >= next)
                    {
                        events = reserved.DequeueBundle();
                    }
                }
            }
            if ((object)events != null)
            {
                for (int i = 0; i < events.Count; ++i)
                {
                    PublishAway(events[i]);
                }
            }

            generator.Tick(now);
        }

        private class Map
        {
            private readonly IDictionary<string, TimeFlow> timeFlows;
            private readonly ReaderWriterLock rwlock;

            internal Map()
            {
                timeFlows = new Dictionary<string, TimeFlow>();
                rwlock = new ReaderWriterLock();
            }

            internal TimeFlow Get(string name)
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    TimeFlow timeFlow;
                    return timeFlows.TryGetValue(name, out timeFlow) ? timeFlow : null;
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }

            internal TimeFlow Create(string name)
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    TimeFlow timeFlow;
                    if (!timeFlows.TryGetValue(name, out timeFlow))
                    {
                        timeFlow = new TimeFlow(name);
                        timeFlows.Add(name, timeFlow);
                    }
                    return timeFlow;
                }
                finally
                {
                    rwlock.ReleaseWriterLock();
                }
            }
        }

        public struct Token
        {
            public DateTime key;
            public Event value;

            public Token(DateTime key, Event value)
            {
                this.key = key;
                this.value = value;
            }
        }

        private class TimeTag
        {
            public DateTime NextTime { get; set; }
            public TimeSpan Interval { get; private set; }

            public TimeTag(TimeSpan interval)
                : this(DateTime.Now + interval, interval)
            {
            }

            public TimeTag(DateTime nextTime, TimeSpan interval)
            {
                NextTime = nextTime;
                Interval = interval;
            }
        }

        private class Generator
        {
            private readonly ReaderWriterLock rwlock;
            private readonly IDictionary<Event, TimeTag> map;
            private readonly TimeFlow owner;

            public Generator(TimeFlow owner)
            {
                rwlock = new ReaderWriterLock();
                map = new Dictionary<Event, TimeTag>();
                this.owner = owner;
            }

            public void Add(Event e, TimeTag timeTag)
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    map[e] = timeTag;
                }
                finally
                {
                    rwlock.ReleaseWriterLock();
                }
            }

            public void Remove(Event e)
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    map.Remove(e);
                }
                finally
                {
                    rwlock.ReleaseWriterLock();
                }
            }

            public void Tick(DateTime now)
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    foreach (var pair in map)
                    {
                        TimeTag timeTag = pair.Value;
                        if (now >= timeTag.NextTime)
                        {
                            owner.PublishAway(pair.Key);
                            timeTag.NextTime = now + timeTag.Interval;
                        }
                    }
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }
        }
    }
}
