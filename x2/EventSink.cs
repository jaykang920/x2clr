// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace x2
{
    /// <summary>
    /// Cleanup helper base class for any event-consuming classes.
    /// </summary>
    /// <remarks>
    /// x2 event handlers are built on C# delegates. In case of an instance
    /// method delegate, it keeps a strong reference to the method target object.
    /// This means that when you bind an event with an instance method, the
    /// target object will never be garbage-collected as long as the handler
    /// delegate lives, resulting in undesirable memory leak.
    ///
    /// EventSink is here to handle the case. First, let your event-consuming
    /// class be derived from EventSink. When the object is no longer needed,
    /// explicitly call its Dispose() method to ensure that all the event
    /// bindings to the object are removed so that the object is properly
    /// garbage-collected.
    ///
    /// An EventSink object should be initialized with a single specific flow.
    /// And an object instance of any EventSink-derived class should never be
    /// shared by two or more different flows. These are constraints by design.
    /// </remarks>
    public class EventSink : IDisposable
    {
        private WeakReference flow;
        private readonly List<Binder.Token> bindings;
        private bool disposed;

        /// <summary>
        /// Gets or sets the flow which this EventSink belongs to.
        /// </summary>
        public Flow Flow
        {
            get { return flow.Target as Flow; }
            set
            {
                if (bindings.Count != 0)
                {
                    throw new InvalidOperationException();
                }
                flow = new WeakReference(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the EventSink class with the flow that
        /// runs the current thread.
        /// </summary>
        public EventSink() : this(Flow.CurrentFlow) { }

        /// <summary>
        /// Initializes a new instance of the EventSink class with the specified
        /// flow.
        /// </summary>
        public EventSink(Flow flow)
        {
            bindings = new List<Binder.Token>();
            this.flow = new WeakReference(flow);
        }

        /// <summary>
        /// Implments IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) { return; }

            CleanUp();

            disposed = true;
        }

        public void Bind<T>(T e, Action<T> handler)
            where T : Event
        {
            var flow = Flow;
            if (flow != null)
            {
                flow.Subscribe(e, handler);
            }
        }

        public void Bind<T>(T e, Action<T> handler, Predicate<T> predicate)
            where T : Event
        {
            var flow = Flow;
            if (flow != null)
            {
                flow.Subscribe(e, handler, predicate);
            }
        }

        public void Bind<T>(T e, Func<Coroutine, T, IEnumerator> handler)
            where T : Event
        {
            var flow = Flow;
            if (flow != null)
            {
                flow.Subscribe(e, handler);
            }
        }

        public void Bind<T>(T e, Func<Coroutine, T, IEnumerator> handler,
            Predicate<T> predicate)
            where T : Event
        {
            var flow = Flow;
            if (flow != null)
            {
                flow.Subscribe(e, handler, predicate);
            }
        }

        public void Unbind<T>(T e, Action<T> handler)
            where T : Event
        {
            var flow = Flow;
            if (flow != null)
            {
                flow.Unsubscribe(e, handler);
            }
        }

        public void Unbind<T>(T e, Action<T> handler, Predicate<T> predicate)
            where T : Event
        {
            var flow = Flow;
            if (flow != null)
            {
                flow.Unsubscribe(e, handler, predicate);
            }
        }

        public void Unbind<T>(T e, Func<Coroutine, T, IEnumerator> handler)
            where T : Event
        {
            var flow = Flow;
            if (flow != null)
            {
                flow.Unsubscribe(e, handler);
            }
        }

        public void Unbind<T>(T e, Func<Coroutine, T, IEnumerator> handler,
            Predicate<T> predicate)
            where T : Event
        {
            var flow = Flow;
            if (flow != null)
            {
                flow.Unsubscribe(e, handler, predicate);
            }
        }

        internal void AddBinding(Binder.Token binderToken)
        {
            lock (bindings)
            {
                bindings.Add(binderToken);
            }
        }

        internal void RemoveBinding(Binder.Token binderToken)
        {
            lock (bindings)
            {
                bindings.Remove(binderToken);
            }
        }

        // Releases all the handler bindings associated with this EventSink.
        private void CleanUp()
        {
            var flow = Flow;
            if (flow == null) { return; }

            lock (bindings)
            {
                if (bindings.Count == 0) { return; }
                
                for (int i = 0, count = bindings.Count; i < count; ++i)
                {
                    flow.Unsubscribe(bindings[i]);
                }

                bindings.Clear();

                this.flow.Target = null;
            }
        }
    }
}
