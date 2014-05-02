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

    // TODO: time scaling
    public class Timer
    {
        private readonly PriorityQueue<DateTime, object> reserved;
        private readonly Repeater repeater;

        private readonly TimerCallback callback;

        public Timer(TimerCallback callback)
        {
            reserved = new PriorityQueue<DateTime, object>();
            repeater = new Repeater(this);

            this.callback = callback;
        }

        public Token Reserve(object state, double seconds)
        {
            return Reserve(state, DateTime.Now.AddSeconds(seconds));
        }

        public Token Reserve(object state, TimeSpan delay)
        {
            return Reserve(state, DateTime.Now.Add(delay));
        }

        public Token Reserve(object state, DateTime when)
        {
            lock (reserved)
            {
                reserved.Enqueue(when, state);
            }
            return new Token(when, state);
        }

        public void Cancel(Token token)
        {
            lock (reserved)
            {
                reserved.Remove(token.key, token.value);
            }
        }

        public void ReserveRepetition(object state, TimeSpan interval)
        {
            repeater.Add(state, new Tag(interval));
        }

        public void ReserveRepetition(object state, DateTime nextTime, TimeSpan interval)
        {
            repeater.Add(state, new Tag(nextTime, interval));
        }

        public void CancelRepetition(object state)
        {
            repeater.Remove(state);
        }

        public void Tick()
        {
            DateTime now = DateTime.Now;
            IList<object> events = null;
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
                    callback(events[i]);
                }
            }

            repeater.Tick(now);
        }

        public struct Token
        {
            public DateTime key;
            public object value;

            public Token(DateTime key, object value)
            {
                this.key = key;
                this.value = value;
            }
        }

        private class Tag
        {
            public DateTime NextTime { get; set; }
            public TimeSpan Interval { get; private set; }

            public Tag(TimeSpan interval)
                : this(DateTime.Now + interval, interval)
            {
            }

            public Tag(DateTime nextTime, TimeSpan interval)
            {
                NextTime = nextTime;
                Interval = interval;
            }
        }

        private class Repeater
        {
            private readonly ReaderWriterLockSlim rwlock;
            private readonly IDictionary<object, Tag> map;
            private readonly Timer owner;

            private Tag defaultCase;

            public Repeater(Timer owner)
            {
                rwlock = new ReaderWriterLockSlim();
                map = new Dictionary<object, Tag>();
                this.owner = owner;
            }

            public void Add(object state, Tag timeTag)
            {
                rwlock.EnterWriteLock();
                try
                {
                    if (state != null)
                    {
                        map[state] = timeTag;
                    }
                    else
                    {
                        defaultCase = timeTag;
                    }
                }
                finally
                {
                    rwlock.ExitWriteLock();
                }
            }

            public void Remove(object state)
            {
                rwlock.EnterWriteLock();
                try
                {
                    if (state != null)
                    {
                        map.Remove(state);
                    }
                    else
                    {
                        defaultCase = null;
                    }
                }
                finally
                {
                    rwlock.ExitWriteLock();
                }
            }

            public void Tick(DateTime now)
            {
                rwlock.EnterReadLock();
                try
                {
                    if (defaultCase != null)
                    {
                        TryFire(now, null, defaultCase);
                    }
                    if (map.Count != 0)
                    {
                        foreach (var pair in map)
                        {
                            TryFire(now, pair.Key, pair.Value);
                        }
                    }
                }
                finally
                {   
                    rwlock.ExitReadLock();
                }
            }

            private void TryFire(DateTime now, object state, Tag tag)
            {
                if (now >= tag.NextTime)
                {
                    owner.callback(state);
                    tag.NextTime = now + tag.Interval;
                }
            }
        }
    }

    public sealed class TimeFlow : FrameBasedFlow
    {
        private const string defaultName = "default";

        private static readonly Map map;

        private Timer timer;

        /// <summary>
        /// Gets the default(anonymous) TimeFlow.
        /// </summary>
        public static TimeFlow Default { get { return Get(); } }

        static TimeFlow()
        {
            map = new Map();

            TimeFlow defaultFlow = map.Create(defaultName);
            Hub.Instance.Attach(defaultFlow);
            defaultFlow.StartUp();
        }

        private TimeFlow(string name)
            : base(null)
        {
            timer = new Timer(OnTimer);
            this.name = name;
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

        public Timer.Token Reserve(Event e, double seconds)
        {
            return timer.Reserve(e, seconds);
        }
        
        public Timer.Token Reserve(Event e, TimeSpan delay)
        {
            return timer.Reserve(e, delay);
        }

        public Timer.Token Reserve(Event e, DateTime when)
        {
            return timer.Reserve(e, when);
        }

        public void Cancel(Timer.Token token)
        {
            timer.Cancel(token);
        }

        public void ReserveRepetition(Event e, TimeSpan interval)
        {
            timer.ReserveRepetition(e, interval);
        }

        public void ReserveRepetition(Event e, DateTime nextTime, TimeSpan interval)
        {
            timer.ReserveRepetition(e, nextTime, interval);
        }

        public void CancelRepetition(Event e)
        {
            timer.CancelRepetition(e);
        }

        protected override void Start() { }
        protected override void Stop() { }

        protected override void Update()
        {
            timer.Tick();
        }

        private class Map
        {
            private readonly IDictionary<string, TimeFlow> timeFlows;
            private readonly ReaderWriterLockSlim rwlock;

            public Map()
            {
                timeFlows = new Dictionary<string, TimeFlow>();
                rwlock = new ReaderWriterLockSlim();
            }

            internal TimeFlow Create(string name)
            {
                rwlock.EnterWriteLock();
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
                    rwlock.ExitWriteLock();
                }
            }

            internal TimeFlow Get(string name)
            {
                rwlock.EnterReadLock();
                try
                {
                    TimeFlow timeFlow;
                    return timeFlows.TryGetValue(name, out timeFlow) ? timeFlow : null;
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }
        }

        void OnTimer(object state)
        {
            Publish((Event)state);
        }
    }
}
