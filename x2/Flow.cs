// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using x2.Events;

namespace x2
{
    /// <summary>
    /// Represents an independent execution flow.
    /// </summary>
    public abstract class Flow
    {
        [ThreadStatic]
        protected static Flow currentFlow;

        [ThreadStatic]
        protected static List<Handler> handlerChain;

        [ThreadStatic]
        protected static Stopwatch stopwatch;

        protected readonly Binder binder;
        protected readonly CaseStack caseStack;
        protected string name;

        private Hub hub;

        public/*internal*/ static Flow CurrentFlow
        {
            get { return currentFlow; }
            set { currentFlow = value; }
        }

        /// <summary>
        /// Gets or sets the default exception handler for all flows.
        /// </summary>
        public static Action<Exception> DefaultExceptionHandler { get; set; }

        public static LogLevel DefaultQueueLengthLogLevel { get; set; }
        public static int DefaultQueueLengthLogThreshold { get; set; }  // in millisec

        public static LogLevel DefaultSlowHandlerLogLevel { get; set; }
        public static int DefaultSlowHandlerLogThreshold { get; set; }  // in millisec

        /// <summary>
        /// Gets or sets the exception handler for this flow.
        /// </summary>
        public Action<Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// Gets the name of this flow.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Gets the hub to which this flow is attached.
        /// </summary>
        public Hub Hub { get { return hub; } }

        public LogLevel QueueLengthLogLevel { get; set; }
        public int QueueLengthLogThreshold { get; set; }

        public LogLevel SlowHandlerLogLevel { get; set; }
        public int SlowHandlerLogThreshold { get; set; }

        static Flow()
        {
            DefaultExceptionHandler = OnException;

            DefaultQueueLengthLogLevel = LogLevel.Warning;
            DefaultQueueLengthLogThreshold = 100;

            DefaultSlowHandlerLogLevel = LogLevel.Warning;
            DefaultSlowHandlerLogThreshold = 100;
        }

        protected Flow(Binder binder)
        {
            this.binder = binder;
            caseStack = new CaseStack();
            name = GetType().Name;

            ExceptionHandler = DefaultExceptionHandler;

            QueueLengthLogLevel = DefaultQueueLengthLogLevel;
            QueueLengthLogThreshold = DefaultQueueLengthLogThreshold;

            SlowHandlerLogLevel = DefaultSlowHandlerLogLevel;
            SlowHandlerLogThreshold = DefaultSlowHandlerLogThreshold;
        }

        public static Binder.Token Bind<T>(T e, Action<T> action)
            where T : Event
        {
            return currentFlow.Subscribe(e, action);
        }

        public static Binder.Token Bind<T>(
            T e, Action<T> action, Predicate<T> predicate)
            where T : Event
        {
            return currentFlow.Subscribe(e, action, predicate);
        }

        public static Binder.Token Bind<T>(T e, Func<Coroutine, T, IEnumerator> routine)
            where T : Event
        {
            return currentFlow.Subscribe(e, routine);
        }

        public static Binder.Token Bind<T>(
            T e, Func<Coroutine, T, IEnumerator> routine, Predicate<T> predicate)
            where T : Event
        {
            return currentFlow.Subscribe(e, routine, predicate);
        }

        public static void Unbind<T>(T e, Action<T> action)
            where T : Event
        {
            currentFlow.Unsubscribe(e, action);
        }

        public static void Unbind<T>(
            T e, Action<T> action, Predicate<T> predicate)
            where T : Event
        {
            currentFlow.Unsubscribe(e, action, predicate);
        }

        public static void Unbind<T>(T e, Func<Coroutine, T, IEnumerator> routine)
            where T : Event
        {
            currentFlow.Unsubscribe(e, routine);
        }

        public static void Unbind<T>(
            T e, Func<Coroutine, T, IEnumerator> routine, Predicate<T> predicate)
            where T : Event
        {
            currentFlow.Unsubscribe(e, routine, predicate);
        }

        public static void Unbind(Binder.Token binderToken)
        {
            currentFlow.Unsubscribe(binderToken);
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
            throw new Exception(null, e);
        }

        public void Publish(Event e)
        {
            hub.Post(e);
        }

        protected void PublishAway(Event e)
        {
            hub.Post(e, currentFlow);
        }

        public Binder.Token Subscribe<T>(T e, Action<T> action)
            where T : Event
        {
            return binder.Bind(e, new MethodHandler<T>(action));
        }

        public Binder.Token Subscribe<T>(
            T e, Action<T> action, Predicate<T> predicate)
            where T : Event
        {
            return binder.Bind(e,
                new ConditionalMethodHandler<T>(action, predicate));
        }

        public Binder.Token Subscribe<T>(T e, Func<Coroutine, T, IEnumerator> routine)
            where T : Event
        {
            return binder.Bind(e, new CoroutineHandler<T>(routine));
        }

        public Binder.Token Subscribe<T>(
            T e, Func<Coroutine, T, IEnumerator> routine, Predicate<T> predicate)
            where T : Event
        {
            return binder.Bind(e,
                new ConditionalCoroutineHandler<T>(routine, predicate));
        }

        public void Unsubscribe<T>(T e, Action<T> handler)
            where T : Event
        {
            binder.Unbind(e, new MethodHandler<T>(handler));
        }

        public void Unsubscribe<T>(T e, Action<T> handler, Predicate<T> predicate)
            where T : Event
        {
            binder.Unbind(e,
                new ConditionalMethodHandler<T>(handler, predicate));
        }

        public void Unsubscribe<T>(T e, Func<Coroutine, T, IEnumerator> handler)
            where T : Event
        {
            binder.Unbind(e, new CoroutineHandler<T>(handler));
        }

        public void Unsubscribe<T>(
            T e, Func<Coroutine, T, IEnumerator> handler, Predicate<T> predicate)
            where T : Event
        {
            binder.Unbind(e,
                new ConditionalCoroutineHandler<T>(handler, predicate));
        }

        public void Unsubscribe(Binder.Token token)
        {
            binder.Unbind(token);
        }

        public abstract void StartUp();
        public abstract void ShutDown();

        public void AttachTo(Hub hub)
        {
            if ((object)this.hub != null && !Object.ReferenceEquals(this.hub, hub))
            {
                throw new InvalidOperationException();
            }
            if (hub.AttachInternal(this))
            {
                this.hub = hub;
            }
        }

        public void DetachFrom(Hub hub)
        {
            if ((object)this.hub == null || !Object.ReferenceEquals(this.hub, hub))
            {
                throw new InvalidOperationException();
            }
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

        public abstract void Feed(Event e);

        protected void Dispatch(Event e)
        {
            handlerChain.Clear();
            int chainLength = binder.BuildHandlerChain(e, handlerChain);
            if (chainLength == 0)
            {
                // unhandled event
                return;
            }
            for (int i = 0, count = handlerChain.Count; i < count; ++i)
            {
                var handler = handlerChain[i];
                try
                {
                    stopwatch.Reset();
                    stopwatch.Start();

                    handler.Invoke(e);
                    
                    stopwatch.Stop();
                    if (stopwatch.ElapsedMilliseconds >= SlowHandlerLogThreshold)
                    {
                        Log.Emit(SlowHandlerLogLevel,
                            "{0} slow handler {1}ms {2}.{3} on {4}", 
                            Name, stopwatch.ElapsedMilliseconds,
                            handler.Action.Method.DeclaringType,
                            handler.Action.Method.Name, e);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHandler(ex);
                }
            }
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
