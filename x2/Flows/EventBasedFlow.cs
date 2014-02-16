﻿// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

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
        protected readonly IQueue<Event> queue;
        protected readonly object syncRoot;

        protected EventBasedFlow(IQueue<Event> queue, Binder binder)
            : base(binder)
        {
            this.queue = queue;
            syncRoot = new Object();
        }

        public override void Feed(Event e)
        {
            var queueLength = queue.Enqueue(e);
            if (queueLength >= QueueLengthLogThreshold)
            {
                Log.Emit(QueueLengthLogLevel,
                    "{0} queue length {1}", Name, queueLength);
            }
        }

        protected void Run()
        {
            currentFlow = this;
            handlerChain = new List<Handler>();
            stopwatch = new Stopwatch();

            while (true)
            {
                Event e = queue.Dequeue();
                if (e == null)
                {
                    break;
                }
                Dispatch(e);

                Thread.Sleep(0);
            }

            handlerChain = null;
            currentFlow = null;
        }
    }
}
