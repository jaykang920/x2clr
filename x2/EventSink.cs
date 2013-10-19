// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace x2
{
    public class EventSink
    {
        private readonly IList<Binder.Token> bindings;

        /// An EventSink-derived class should be instantiated in a thread of a single
        /// specific Flow. And an object instance of any EventSink-derived class 
        /// should not be shared by two or more different flows. These are 
        /// constraints by design.
        private WeakReference flow;

        public EventSink()
        {
            bindings = new List<Binder.Token>();
            flow = new WeakReference(Flow.CurrentFlow);
        }

        ~EventSink()
        {
            if (bindings.Count > 0)
            {
                CleanUp();
            }
        }

        internal void AddBinding(Binder.Token binderToken)
        {
            bindings.Add(binderToken);
        }

        internal void RemoveBinding(Binder.Token binderToken)
        {
            bindings.Remove(binderToken);
        }

        protected void CleanUp()
        {
            Flow owner = flow.Target as Flow;
            if (owner == null)
            {
                return;
            }
            foreach (var binding in bindings)
            {
                owner.Unbind(binding);
            }
            bindings.Clear();
        }
    }
}
