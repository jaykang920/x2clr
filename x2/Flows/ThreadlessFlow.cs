// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using x2.Events;
using x2.Queues;

namespace x2.Flows
{
    public class ThreadlessFlow : Flow
    {
        protected IQueue<Event> queue;
        protected readonly object syncRoot;
        protected bool running;

        public ThreadlessFlow()
            : this(new UnboundedQueue<Event>())
        {
        }

        public ThreadlessFlow(IQueue<Event> queue)
        {
            this.queue = queue;
            syncRoot = new Object();
            running = false;
        }

        public override void Feed(Event e)
        {
            queue.Enqueue(e);
        }

        public override Flow StartUp()
        {
            lock (syncRoot)
            {
                if (!running)
                {
                    SetUp();
                    caseStack.SetUp(this);
                    handlerChain = new List<Handler>();
                    stopwatch = new Stopwatch();

                    currentFlow = this;

                    running = true;

                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void ShutDown()
        {
            lock (syncRoot)
            {
                if (!running)
                {
                    return;
                }
                queue.Close(new FlowStop());
                running = false;

                handlerChain = null;

                currentFlow = null;

                caseStack.TearDown(this);
                TearDown();
            }
        }

        public void Dispatch()
        {
            Event e = queue.Dequeue();
            if (e != null)
            {
                Dispatch(e);
            }
        }

        public Event TryDispatch()
        {
            Event e;
            if (queue.TryDequeue(out e))
            {
                Dispatch(e);
                return e;
            }
            return null;
        }

        public bool TryDequeue(out Event e)
        {
            return queue.TryDequeue(out e);
        }

        /// <summary>
        /// Wait for a single event of type (T) with timeout in seconds.
        /// </summary>
        public bool Wait<T>(T expected, out T actual, double seconds)
            where T : Event
        {
            return Wait(expected, out actual, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Wait for a single event of type (T) with timeout.
        /// </summary>
        public bool Wait<T>(T expected, out T actual, TimeSpan timeout)
            where T : Event
        {
            actual = null;
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            while (stopWatch.Elapsed < timeout)
            {
                Event dequeued;
                if (queue.TryDequeue(out dequeued))
                {
                    Dispatch(dequeued);

                    if (expected.IsEquivalent(dequeued))
                    {
                        actual = (T)dequeued;
                        return true;
                    }
                }

                Thread.Sleep(1);
            }
            return false;
        }

        /// <summary>
        /// Wait for multiple events with timeout in seconds.
        /// </summary>
        public bool Wait(double seconds, out Event[] actual, params Event[] expected)
        {
            return Wait(TimeSpan.FromSeconds(seconds), out actual, expected);
        }

        /// <summary>
        /// Wait for multiple events with timeout.
        /// </summary>
        public bool Wait(TimeSpan timeout, out Event[] actual, params Event[] expected)
        {
            int count = 0;
            actual = new Event[expected.Length];
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            while (stopWatch.Elapsed < timeout)
            {
                Event dequeued;
                if (queue.TryDequeue(out dequeued))
                {
                    Dispatch(dequeued);

                    for (int i = 0; i < expected.Length; ++i)
                    {
                        if (actual[i] == null && expected[i].IsEquivalent(dequeued))
                        {
                            actual[i] = dequeued;
                            if (++count >= expected.Length)
                            {
                                return true;
                            }
                            break;
                        }
                    }
                }

                Thread.Sleep(1);
            }
            return false;
        }
    }
}
