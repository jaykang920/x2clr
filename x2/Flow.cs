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

        private Hub hub;  // the hub to which this flow is attached

        public/*internal*/ static Flow CurrentFlow
        {
            get { return currentFlow; }
            set { currentFlow = value; }
        }

        /// <summary>
        /// Gets or sets the default exception handler for all flows.
        /// </summary>
        public static Action<Exception> DefaultExceptionHandler { get; set; }

        /// <summary>
        /// Gets or sets the exception handler for this flow.
        /// </summary>
        public Action<Exception> ExceptionHandler { get; set; }

        static Flow()
        {
            DefaultExceptionHandler = OnException;
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

            ExceptionHandler = DefaultExceptionHandler;
        }

        public static void Post(Event e)
        {
            currentFlow.hub.Post(e);
        }

        public static void PostAway(Event e)
        {
            currentFlow.hub.Post(e, currentFlow);
        }

        /// <summary>
        /// Starts all the flows attached to the hubs in the current process.
        /// </summary>
        public static void StartAll()
        {
            Hub.StartAllFlows();
        }

        /// <summary>
        /// Stops all the flows attached to the hubs in the current process.
        /// </summary>
        public static void StopAll()
        {
            Hub.StopAllFlows();
        }

        /// <summary>
        /// Default exception handler.
        /// </summary>
        private static void OnException(Exception e)
        {
            throw new Exception("", e);
        }

        public void Unbind(Event e, IHandler handler)
        {
        }

        protected void Publish(Event e)
        {
            hub.Post(e);
        }

        protected void PublishAway(Event e)
        {
            hub.Post(e, currentFlow);
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
                this.hub = hub;
            }
        }

        public void DetachFrom(Hub hub)
        {
            if (hub.DetachInternal(this))
            {
                this.hub = null;
            }
        }

        public Flow Add(ICase c)
        {
            caseStack.Add(c);
            return this;
        }

        public Flow Remove(ICase c)
        {
            caseStack.Remove(c);
            return this;
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
                try
                {
                    handler.Invoke(e);
                }
                catch (Exception ex)
                {
                    ExceptionHandler(ex);
                }
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
    }
}
