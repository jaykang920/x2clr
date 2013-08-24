// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace x2
{
    public class EventSink : IDisposable
    {
        private readonly List<KeyValuePair<Event, IHandler>> bindings;
        /// An EventSink-derived class should be instantiated in a thread of a single
        /// specific Flow. And an object instance of any EventSink-derived class 
        /// should not be shared by two or more different flows. These are 
        /// constraints by design.
        private WeakReference flow;

        public EventSink()
        {
            bindings = new List<KeyValuePair<Event, IHandler>>();
            flow = new WeakReference(Flow.CurrentFlow);
        }

        ~EventSink()
        {
            UnbindAll();
        }

        internal void AddBinding(Event e, IHandler handler)
        {
            bindings.Add(new KeyValuePair<Event, IHandler>(e, handler));
        }

        internal void RemoveBinding(Event e, IHandler handler)
        {
            bindings.Remove(new KeyValuePair<Event, IHandler>(e, handler));
        }

        protected void UnbindAll()
        {
            Flow owner = flow.Target as Flow;
            if (owner == null)
            {
                return;
            }
            foreach (var binding in bindings)
            {
                owner.Unbind(binding.Key, binding.Value);
            }
            bindings.Clear();
        }

        public void Dispose()
        {
            UnbindAll();

            GC.SuppressFinalize(this);
        }
    }
}
