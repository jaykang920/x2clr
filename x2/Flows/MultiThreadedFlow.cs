// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2.Events;

namespace x2.Flows
{
    public class MultiThreadedFlow : Flow
    {
        protected readonly IQueue<Event> queue;
        protected readonly object syncRoot;
        protected readonly List<Thread> threads;
        protected int numThreads;

        public MultiThreadedFlow(IQueue<Event> queue, int numThreads)
            : base(new SynchronizedBinding())
        {
            this.queue = queue;
            syncRoot = new Object();
            threads = new List<Thread>();
            this.numThreads = numThreads;
        }

        protected internal override void Feed(Event e)
        {
            queue.Enqueue(e);
        }

        public override void StartUp()
        {
            lock (syncRoot)
            {
                if (threads.Count != 0)
                {
                    return;
                }
                SetUp();
                caseStack.SetUp();
                for (int i = 0; i < numThreads; ++i)
                {
                    Thread thread = new Thread(this.Run);
                    threads.Add(thread);
                    thread.Start();
                }
                queue.Enqueue(new FlowStartupEvent());
            }
        }

        public override void ShutDown()
        {
            lock (syncRoot)
            {
                if (threads.Count == 0)
                {
                    return;
                }
                queue.Close(new FlowShutdownEvent());
                foreach (Thread thread in threads)
                {
                    if (thread != null)
                    {
                        thread.Join();
                    }
                }
                threads.Clear();
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
