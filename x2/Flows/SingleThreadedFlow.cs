// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2.Events;
using x2.Queues;

namespace x2.Flows
{
    public class SingleThreadedFlow : EventBasedFlow
    {
        protected Thread thread;

        public SingleThreadedFlow()
            : this(new UnboundedQueue<Event>())
        {
        }
        
        public SingleThreadedFlow(IQueue<Event> queue)
            : base(queue)
        {
            thread = null;
        }

        public SingleThreadedFlow(string name)
            : this(name, new UnboundedQueue<Event>())
        {
        }

        public SingleThreadedFlow(string name, IQueue<Event> queue)
            : this(queue)
        {
            this.name = name;
        }

        public override Flow StartUp()
        {
            lock (syncRoot)
            {
                if (thread == null)
                {
                    SetUp();
                    caseStack.SetUp(this);
                    thread = new Thread(this.Run);
                    thread.Name = name;
                    thread.Start();
                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void ShutDown()
        {
            lock (syncRoot)
            {
                if (thread == null)
                {
                    return;
                }
                queue.Close(new FlowStop());
                thread.Join();
                thread = null;

                caseStack.TearDown(this);
                TearDown();
            }
        }
    }
}
