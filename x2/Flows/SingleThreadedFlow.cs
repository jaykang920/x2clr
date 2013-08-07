// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2.Events;

namespace x2.Flows
{
    public class SingleThreadedFlow : Flow
    {
        protected readonly IQueue<Event> queue;
        protected readonly object syncRoot;
        protected Thread thread;

        public SingleThreadedFlow(IQueue<Event> queue)
            : base(new Binder())
        {
            this.queue = queue;
            syncRoot = new Object();
            thread = null;
        }

        protected internal override void Feed(Event e)
        {
            queue.Enqueue(e);
        }

        public override void StartUp()
        {
            lock (syncRoot)
            {
                if (thread != null)
                {
                    return;
                }

                // Workaround to support Flow.Bind/Unbind
                currentFlow = this;

                SetUp();
                caseStack.SetUp();
                thread = new Thread(this.Run);
                thread.Start();
                queue.Enqueue(new FlowStartupEvent());
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
                queue.Close(new FlowShutdownEvent());
                thread.Join();
                thread = null;

                // Workaround to support Flow.Bind/Unbind
                currentFlow = this;

                caseStack.TearDown();
                TearDown();
            }
        }

        private void Run()
        {
            currentFlow = this;
            handlerChain = new List<Handler>();

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
