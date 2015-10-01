// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    public class MultiThreadFlow : EventBasedFlow
    {
        protected List<Thread> threads;
        protected int numThreads;

        public MultiThreadFlow(int numThreads)
            : this(new UnboundedQueue<Event>(), numThreads)
        {
        }

        public MultiThreadFlow(IQueue<Event> queue, int numThreads)
            : base(queue)
        {
            threads = new List<Thread>();
            this.numThreads = numThreads;
        }

        public MultiThreadFlow(string name, int numThreads)
            : this(name, new UnboundedQueue<Event>(), numThreads)
        {
        }

        public MultiThreadFlow(string name, IQueue<Event> queue, int numThreads)
            : this(queue, numThreads)
        {
            this.name = name;
        }

        public override Flow Startup()
        {
            lock (syncRoot)
            {
                if (threads.Count == 0)
                {
                    Setup();
                    caseStack.Setup(this);
                    for (int i = 0; i < numThreads; ++i)
                    {
                        Thread thread = new Thread(Run);
                        thread.Name = String.Format("{0} {1}", name, i + 1);
                        threads.Add(thread);
                        thread.Start();
                    }
                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void Shutdown()
        {
            lock (syncRoot)
            {
                if (threads.Count == 0)
                {
                    return;
                }
                queue.Close(new FlowStop());
                foreach (Thread thread in threads)
                {
                    if (thread != null)
                    {
                        thread.Join();
                    }
                }
                threads.Clear();

                caseStack.Teardown(this);
                Teardown();
            }
        }

        private void Run()
        {
            currentFlow = this;
            equivalent = new EventEquivalent();
            handlerChain = new List<Handler>();

            while (true)
            {
                Event e = queue.Dequeue();
                if (Object.ReferenceEquals(e, null))
                {
                    break;
                }
                Dispatch(e);
            }

            handlerChain = null;
            equivalent = null;
            currentFlow = null;
        }
    }
}
