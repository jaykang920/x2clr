// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

// Contributed by keedongpark

#if NET40
using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Performance optimized version for server side only. 
    /// Client (Unity) still needs to use SingleThreadFlow
    /// </summary>
    public class ConcurrentThreadFlow : ConcurrentQueueFlow
    {
        protected Thread thread;

        public ConcurrentThreadFlow()
        {
            thread = null;
        }

        public ConcurrentThreadFlow(string name)
            : this()
        {
            this.name = name;
        }

        public override Flow Startup()
        {
            lock (syncRoot)
            {
                if (thread == null)
                {
                    Setup();
                    caseStack.Setup(this);
                    thread = new Thread(Run);
                    thread.Name = name;
                    thread.Start();
                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void Shutdown()
        {
            lock (syncRoot)
            {
                if (thread == null)
                {
                    return;
                }
                queue.Enqueue(new FlowStop());
                thread.Join();
                thread = null;

                caseStack.Teardown(this);
                Teardown();
            }
        }

        private void Run()
        {
            currentFlow = this;
            equivalent = new EventEquivalent();
            handlerChain = new List<Handler>();
            Event evt;

            while (true)
            {
                if (!queue.TryDequeue(out evt))
                {
                    Thread.Sleep(1);

                    continue;
                }


                if (evt.GetTypeId() == FlowStop.TypeId)
                {
                    break;
                }

                Dispatch(evt);
            }

            handlerChain = null;
            equivalent = null;
            currentFlow = null;
        }
    }
}
#endif
