// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace x2
{
    public class ThreadlessFlow : EventBasedFlow
    {
        protected bool running;

        public ThreadlessFlow()
            : this(new UnboundedQueue<Event>())
        {
        }

        public ThreadlessFlow(IQueue<Event> queue)
            : base(queue)
        {
            running = false;
        }

        public override Flow Startup()
        {
            lock (syncRoot)
            {
                if (!running)
                {
                    Setup();
                    caseStack.Setup(this);

                    currentFlow = this;
                    equivalent = new EventEquivalent();
                    events = new List<Event>();
                    handlerChain = new List<Handler>();

                    running = true;

                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void Shutdown()
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
                events = null;
                equivalent = null;
                currentFlow = null;

                caseStack.Teardown(this);
                Teardown();
            }
        }

        public void Dispatch()
        {
            if (queue.Dequeue(events) == 0)
            {
                return;
            }
            for (int i = 0, count = events.Count; i < count; ++i)
            {
                Dispatch(events[i]);
            }
            events.Clear();
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

        public List<Event> TryDispatchAll()
        {
            var result = new List<Event>();
            if (queue.TryDequeue(events))
            {
                for (int i = 0, count = events.Count; i < count; ++i)
                {
                    Event e = events[i];
                    Dispatch(e);
                    result.Add(e);
                }
                events.Clear();
            }
            return result;
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

                    if (expected.Equivalent(dequeued))
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
                        if (actual[i] == null && expected[i].Equivalent(dequeued))
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
