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

    public sealed class TimeFlow : Flow
    {
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

        private readonly IQueue<Event> incomming;
        private readonly PriorityQueue<DateTime, Event> outgoing;
        private readonly object syncRoot;
        private Thread thread;

        public TimeFlow()
            : base(new Binder())
        {
            incomming = new UnboundedQueue<Event>();
            syncRoot = new Object();
            thread = null;

            outgoing = new PriorityQueue<DateTime, Event>();
        }
        
        protected internal override void Feed(Event e)
        {
            incomming.Enqueue(e);
        }

        public override void StartUp()
        {
            lock (syncRoot)
            {
                if (thread != null)
                {
                    return;
                }

                SetUp();
                caseStack.SetUp(this);
                thread = new Thread(this.Run);
                thread.Start();
                incomming.Enqueue(new FlowStart());
            }
        }

        public override void ShutDown()
        {
            lock (syncRoot)
            {
                if (thread == null)
                {
                    return;
                }
                incomming.Close(new FlowStop());
                thread.Join();
                thread = null;

                caseStack.TearDown(this);
                TearDown();
            }
        }

        public Token Reserve(TimeSpan delay, Event e)
        {
            return Reserve(DateTime.Now + delay, e);
        }

        public Token Reserve(DateTime when, Event e)
        {
            lock (outgoing)
            {
                outgoing.Enqueue(when, e);
            }
            return new Token(when, e);
        }

        public void Cancel(Token token)
        {
            lock (outgoing)
            {
                outgoing.Remove(token.key, token.value);
            }
        }

        private void Run()
        {
            currentFlow = this;
            handlerChain = new List<IHandler>();

            while (true)
            {
                Event e;
                if (incomming.TryDequeue(out e))
                {
                    Dispatch(e);

                    if (e.GetTypeId() == (int)BuiltinType.FlowStop)
                    {
                        break;
                    }
                }

                List<Event> events = null;
                lock (outgoing)
                {
                    if (outgoing.Count != 0)
                    {
                        DateTime next = outgoing.Peek();
                        if (DateTime.Now > next)
                        {
                            events = outgoing.DequeueBundle();
                        }
                    }
                }
                if ((object)events != null)
                {
                    foreach (var deferred in events)
                    {
                        PublishAway(deferred);
                    }
                }

                Thread.Sleep(1);
            }

            handlerChain = null;
            currentFlow = null;
        }
    }
}
