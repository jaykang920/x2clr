// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2.Events;
using x2.Queues;

namespace x2.Flows
{
    public class ThreadlessFlow : Flow
    {
        protected readonly IQueue<Event> queue;
        protected readonly object syncRoot;
        protected bool running;

        public ThreadlessFlow()
            : this(new UnboundedQueue<Event>())
        {
        }

        public ThreadlessFlow(IQueue<Event> queue)
            : base(new Binder())
        {
            this.queue = queue;
            syncRoot = new Object();
            running = false;
        }

        protected internal override void Feed(Event e)
        {
            queue.Enqueue(e);
        }

        public override void StartUp()
        {
            lock (syncRoot)
            {
                if (running)
                {
                    return;
                }

                SetUp();
                caseStack.SetUp(this);
                handlerChain = new List<Handler>();

                currentFlow = this;

                running = true;

                queue.Enqueue(new FlowStart());
            }
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

        public void TryDispatch()
        {
            Event e;
            if (queue.TryDequeue(out e))
            {
                Dispatch(e);
            }
        }
    }
}
