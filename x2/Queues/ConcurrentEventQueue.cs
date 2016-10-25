// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

// Proposed and contributed by @keedongpark

#if NET40

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Unbounded event queue based on .NET 4 concurrent (lock-free) queue.
    /// </summary>
    public class ConcurrentEventQueue : EventQueue
    {
        private ConcurrentQueue<Event> queue;
        private volatile bool closing;

        public override int Length { get { return queue.Count; } }

        public ConcurrentEventQueue()
        {
            queue = new ConcurrentQueue<Event>();
        }

        public override void Close(Event finalItem)
        {
            closing = true;
            queue.Enqueue(finalItem);
        }

        public override Event Dequeue()
        {
            Event result = null;
            SpinWait spinWait = new SpinWait();

            while (!closing)
            {
                if (queue.TryDequeue(out result))
                {
                    break;
                }
                spinWait.SpinOnce();
            }

            return result;
        }

        public override void Enqueue(Event item)
        {
            queue.Enqueue(item);
        }

        public override bool TryDequeue(out Event value)
        {
            return queue.TryDequeue(out value);
        }
    }
}

#endif
