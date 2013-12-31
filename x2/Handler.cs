// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace x2
{
    /// <summary>
    /// Defines a method to handle events.
    /// </summary>
    public interface IHandler
    {
        void Invoke(Event e);
    }

    /// <summary>
    /// Abstract base class for concrete event handlers.
    /// </summary>
    public abstract class Handler : IHandler
    {
        public abstract Delegate Action { get; }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Handler other = (Handler)obj;
            return Action.Equals(other.Action);
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode();
        }

        public abstract void Invoke(Event e);
    }

    public class MethodHandler<T> : Handler
        where T : Event
    {
        protected readonly Action<T> action;

        public override Delegate Action { get { return action; } }

        public MethodHandler(Action<T> action)
        {
            this.action = action;
        }

        public override void Invoke(Event e)
        {
            action((T)e);
        }
    }

    public class CoroutineHandler<T> : Handler
        where T : Event
    {
        protected readonly Func<Coroutine, T, IEnumerator> action;

        public override Delegate Action { get { return action; } }

        public CoroutineHandler(Func<Coroutine, T, IEnumerator> action)
        {
            this.action = action;
        }

        public override void Invoke(Event e)
        {
            Coroutine coroutine = new Coroutine();
            coroutine.Start(action(coroutine, (T)e));
        }
    }

    public class ConditionalMethodHandler<T> : MethodHandler<T>
        where T : Event
    {
        private readonly Predicate<T> predicate;

        public ConditionalMethodHandler(Action<T> action, Predicate<T> predicate)
            : base(action)
        {
            this.predicate = predicate;
        }

        /*
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
            {
                return false;
            }

            var other = (ConditionalMethodHandler<T>)obj;
            return predicate.Equals(other.predicate);
        }

        public override int GetHashCode()
        {
            Hash hash = new Hash(Hash.Seed);
            hash.Update(base.GetHashCode());
            hash.Update(predicate.GetHashCode());
            return hash.Code;
        }
        */

        public override void Invoke(Event e)
        {
            if (predicate((T)e))
            {
                base.Invoke((T)e);
            }
        }
    }

    public class ConditionalCoroutineHandler<T> : CoroutineHandler<T>
        where T : Event
    {
        private readonly Predicate<T> predicate;

        public ConditionalCoroutineHandler(Func<Coroutine, T, IEnumerator> action,
            Predicate<T> predicate) : base(action)
        {
            this.predicate = predicate;
        }

        /*
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
            {
                return false;
            }

            var other = (ConditionalCoroutineHandler<T>)obj;
            return predicate.Equals(other.predicate);
        }

        public override int GetHashCode()
        {
            Hash hash = new Hash(Hash.Seed);
            hash.Update(base.GetHashCode());
            hash.Update(predicate.GetHashCode());
            return hash.Code;
        }
        */

        public override void Invoke(Event e)
        {
            if (predicate((T)e))
            {
                base.Invoke((T)e);
            }
        }
    }
}
