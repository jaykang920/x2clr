// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace x2
{
    public interface IHandler : IComparable<IHandler>
    {
        Delegate Action { get; }

        void Invoke(Event e);
    }

    public struct Handler<T> : IHandler
        where T : Event
    {
        private readonly Action<T> action;

        public Delegate Action { get { return action; } }

        public Handler(Action<T> action)
        {
            this.action = action;
        }

        public int CompareTo(IHandler other)
        {
            int token = action.Method.MetadataToken;
            int otherToken = other.Action.Method.MetadataToken;
            if (token == otherToken)
            {
                return 0;
            }
            else if (token < otherToken)
            {
                return -1;
            }
            return 1;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj == null ||
                typeof(IHandler).IsAssignableFrom(obj.GetType()) == false)
            {
                return false;
            }
            IHandler other = (IHandler)obj;
            return action.Equals(other.Action);
        }

        public override int GetHashCode()
        {
            return action.GetHashCode();
        }

        public void Invoke(Event e)
        {
            action((T)e);
        }

        public static bool operator ==(Handler<T> x, IHandler y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Handler<T> x, IHandler y)
        {
            return !x.Equals(y);
        }
    }

    public struct CoroutineHandler<T> : IHandler
        where T : Event
    {
        private readonly Func<T, Coroutine, IEnumerator> action;

        public Delegate Action { get { return action; } }

        public CoroutineHandler(Func<T, Coroutine, IEnumerator> action)
        {
            this.action = action;
        }

        public int CompareTo(IHandler other)
        {
            int token = action.Method.MetadataToken;
            int otherToken = other.Action.Method.MetadataToken;
            if (token == otherToken)
            {
                return 0;
            }
            else if (token < otherToken)
            {
                return -1;
            }
            return 1;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj == null || !(obj is IHandler))
            {
                return false;
            }
            IHandler other = (IHandler)obj;
            return action.Equals(other.Action);
        }

        public override int GetHashCode()
        {
            return action.GetHashCode();
        }

        public void Invoke(Event e)
        {
            Coroutine coroutine = new Coroutine();
            coroutine.Start(action((T)e, coroutine));
        }

        public static bool operator ==(CoroutineHandler<T> x, IHandler y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(CoroutineHandler<T> x, IHandler y)
        {
            return !x.Equals(y);
        }
    }
}
