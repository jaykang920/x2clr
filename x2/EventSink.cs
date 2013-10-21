// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace x2
{
    public class EventSink
    {
        private readonly WeakReference flow;
        private readonly IList<Binder.Token> bindings;

        /// An EventSink-derived class should be instantiated in a thread of a single
        /// specific Flow. And an object instance of any EventSink-derived class 
        /// should not be shared by two or more different flows. These are 
        /// constraints by design.

        public EventSink(Flow flow)
        {
            bindings = new List<Binder.Token>();
            this.flow = new WeakReference(flow);
        }

        ~EventSink()
        {
            if (bindings.Count > 0)
            {
                CleanUp();
            }
        }

        public  void Bind<T>(T e, Action<T> handler)
            where T : Event
        {
            Flow target = flow.Target as Flow;
            if (target != null)
            {
                target.Subscribe(e, handler);
            }
        }

        public void Bind<T>(T e, Func<T, Coroutine, IEnumerator> handler)
            where T : Event
        {
            Flow target = flow.Target as Flow;
            if (target != null)
            {
                target.Subscribe(e, handler);
            }
        }

        public void Unbind<T>(T e, Action<T> handler)
            where T : Event
        {
            Flow target = flow.Target as Flow;
            if (target != null)
            {
                target.Unsubscribe(e, handler);
            }
        }

        public void Unbind<T>(T e, Func<T, Coroutine, IEnumerator> handler)
            where T : Event
        {
            Flow target = flow.Target as Flow;
            if (target != null)
            {
                target.Unsubscribe(e, handler);
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

        public void CleanUp()
        {
            Flow target = flow.Target as Flow;
            if (target == null)
            {
                return;
            }
            foreach (var binding in bindings)
            {
                target.Unbind(binding);
            }
            bindings.Clear();
        }
    }
}
