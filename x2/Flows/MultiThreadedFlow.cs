// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2.Events;
using x2.Queues;

namespace x2.Flows
{
    public class MultiThreadedFlow : EventBasedFlow
    {
        protected readonly List<Thread> threads;
        protected int numThreads;

        public MultiThreadedFlow(int numThreads)
            : this(new UnboundedQueue<Event>(), numThreads)
        {
        }

        public MultiThreadedFlow(IQueue<Event> queue, int numThreads)
            : base(queue, new SynchronizedBinding())
        {
            threads = new List<Thread>();
            this.numThreads = numThreads;
        }

        public MultiThreadedFlow(string name, int numThreads)
            : this(name, new UnboundedQueue<Event>(), numThreads)
        {
        }

        public MultiThreadedFlow(string name, IQueue<Event> queue, int numThreads)
            : this(queue, numThreads)
        {
            this.name = name;
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
                caseStack.SetUp(this);
                for (int i = 0; i < numThreads; ++i)
                {
                    Thread thread = new Thread(this.Run);
                    thread.Name = String.Format("{0} {1}", name, i + 1);
                    threads.Add(thread);
                    thread.Start();
                }
                queue.Enqueue(new FlowStart());
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
                queue.Close(new FlowStop());
                foreach (Thread thread in threads)
                {
                    if (thread != null)
                    {
                        thread.Join();
                    }
                }
                threads.Clear();

                caseStack.TearDown(this);
                TearDown();
            }
        }
    }
}
