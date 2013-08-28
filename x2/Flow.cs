// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

using x2.Events;

namespace x2
{
    public abstract class Flow
    {
        [ThreadStatic]
        protected static Flow currentFlow;

        [ThreadStatic]
        protected static List<IHandler> handlerChain;

        protected readonly Binder binder;

        protected readonly CaseStack caseStack;

        private readonly HubSet hubSet;

        public/*internal*/ static Flow CurrentFlow
        {
            get { return currentFlow; }
            set { currentFlow = value; }
        }

        public static void Bind<T>(T e, Action<T> handler)
            where T : Event
        {
            currentFlow.Subscribe(e, handler);
        }

        public static void Unbind<T>(T e, Action<T> handler)
            where T : Event
        {
            currentFlow.Unsubscribe(e, handler);
        }

        protected Flow(Binder binder)
        {
            this.binder = binder;
            caseStack = new CaseStack();
            hubSet = new HubSet();
        }

        public static void Post(Event e)
        {
            currentFlow.hubSet.Post(e);
        }

        public static void PostAway(Event e)
        {
            currentFlow.hubSet.Post(e, currentFlow);
        }

        /// <summary>
        /// Starts all the flows attached to all the hubs in the current process.
        /// </summary>
        public static void StartAll()
        {
            Hub.StartAllFlows();
        }

        /// <summary>
        /// Stops all the flows attached to all the hubs in the current process.
        /// </summary>
        public static void StopAll()
        {
            Hub.StopAllFlows();
        }

        public void Unbind(Event e, IHandler handler)
        {
        }

        protected void Publish(Event e)
        {
            hubSet.Post(e);
        }

        protected void PublishAway(Event e)
        {
            hubSet.Post(e, currentFlow);
        }

        public void Subscribe<T>(T e, Action<T> handler)
            where T : Event
        {
            binder.Bind(e, new Handler<T>(handler));
        }

        public void Unsubscribe<T>(T e, Action<T> handler)
            where T : Event
        {
            binder.Unbind(e, new Handler<T>(handler));
        }

        public abstract void StartUp();
        public abstract void ShutDown();

        public void AttachTo(Hub hub)
        {
            if (hub.AttachInternal(this))
            {
                hubSet.Add(hub);
            }
        }

        public void DetachFrom(Hub hub)
        {
            if (hub.DetachInternal(this))
            {
                hubSet.Remove(hub);
            }
        }

        public void DetachFromAll()
        {
            hubSet.Clear(this);
        }

        public void Add(ICase c)
        {
            caseStack.Add(c);
        }

        public void Remove(ICase c)
        {
            caseStack.Remove(c);
        }

        protected internal abstract void Feed(Event e);

        protected void Dispatch(Event e)
        {
            int chainLength = binder.BuildHandlerChain(e, handlerChain);
            if (chainLength == 0)
            {
                // unhandled event
                return;
            }
            foreach (var handler in handlerChain)
            {
                handler.Invoke(e);
            }
            handlerChain.Clear();
        }

        protected virtual void SetUp()
        {
            Subscribe(new FlowStart(), OnFlowStart);
            Subscribe(new FlowStop(), OnFlowStop);
        }

        protected virtual void TearDown()
        {
            Unsubscribe(new FlowStop(), OnFlowStop);
            Unsubscribe(new FlowStart(), OnFlowStart);
        }

        protected virtual void OnStart() {}

        protected virtual void OnStop() {}

        private void OnFlowStart(FlowStart e)
        {
            OnStart();
        }

        private void OnFlowStop(FlowStop e)
        {
            OnStop();
        }

        private class HubSet
        {
            private List<Hub> hubs = new List<Hub>();
            private ReaderWriterLock rwlock = new ReaderWriterLock();

            internal void Add(Hub hub)
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    if (!hubs.Contains(hub))
                    {
                        hubs.Add(hub);
                    }
                }
                finally
                {
                    rwlock.ReleaseWriterLock();
                }
            }

            internal void Remove(Hub hub)
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    hubs.Remove(hub);
                }
                finally
                {
                    rwlock.ReleaseWriterLock();
                }
            }

            internal void Clear(Flow flow)
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    foreach (Hub hub in hubs)
                    {
                        hub.Detach(flow);
                    }
                    hubs.Clear();
                }
                finally
                {
                    rwlock.ReleaseWriterLock();
                }
            }

            internal void Post(Event e)
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    foreach (Hub hub in hubs)
                    {
                        hub.Post(e);
                    }
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }

            internal void Post(Event e, Flow except)
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                try
                {
                    foreach (Hub hub in hubs)
                    {
                        hub.Post(e, except);
                    }
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }
        }
    }
}
