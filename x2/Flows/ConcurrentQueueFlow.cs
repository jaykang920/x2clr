// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;

namespace x2
{
    /// <summary>
    /// Abstract base class for event-based concurrent flow
    /// </summary>
    public abstract class ConcurrentQueueFlow : Flow
    {
        protected ConcurrentQueue<Event> queue;
        protected readonly object syncRoot;

        protected ConcurrentQueueFlow()
        {
            queue = new ConcurrentQueue<Event>();
            syncRoot = new Object();
        }

        public override void Feed(Event e)
        {
            queue.Enqueue(e);
        }

        protected override void OnHeartbeat()
        {
            int length = queue.Count;
            if (length >= LongQueueLogThreshold)
            {
                Log.Emit(LongQueueLogLevel, "{0} queue length {1}", name, length);
            }
        }
    }
}
