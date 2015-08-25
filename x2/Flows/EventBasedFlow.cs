// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using x2.Events;
using x2.Queues;

namespace x2.Flows
{
    /// <summary>
    /// Abstract base class for event-based (waiting) execution flows.
    /// </summary>
    public abstract class EventBasedFlow : Flow
    {
        protected IQueue<Event> queue;
        protected readonly object syncRoot;

        protected EventBasedFlow(IQueue<Event> queue)
        {
            this.queue = queue;
            syncRoot = new Object();
        }

        public override void Feed(Event e)
        {
            queue.Enqueue(e);
        }
    }
}
