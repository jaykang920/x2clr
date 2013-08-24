// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace x2
{
    public interface IHandler : IComparable<IHandler>
    {
        MethodInfo Method { get; }
        int Token { get; }

        void Invoke(Event e);
    }

    public struct MethodHandler<T> : IHandler
        where T : Event
    {
        private readonly Action<T> action;
        private readonly int token;

        public MethodInfo Method { get { return action.Method; } }
        public int Token { get { return token; } }

        public MethodHandler(Action<T> action)
        {
            this.action = action;
            token = action.Method.MetadataToken;
        }

        public int CompareTo(IHandler other)
        {
            if (Token == other.Token)
            {
                return 0;
            }
            else if (Token < other.Token)
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
            if (obj == null || typeof(IHandler).IsAssignableFrom(obj.GetType()))
            {
                return false;
            }
            IHandler other = (IHandler)obj;
            return Method.Equals(other.Method);
        }

        public override int GetHashCode()
        {
            return Method.GetHashCode();
        }

        public void Invoke(Event e)
        {
            action((T)e);
        }
    }

    public struct InstanceMethodHandler<T, U> : IHandler
        where T : Event
        where U : class
    {
        private readonly Action<T> action;
        private readonly int token;

        public MethodInfo Method { get { return action.Method; } }
        public int Token { get { return token; } }

        public InstanceMethodHandler(Action<T> action, U target)
        {
            this.action = (Action<T>)Delegate.CreateDelegate(
                typeof(Action<T>), target, action.Method.Name);
            token = action.Method.MetadataToken;
        }

        public int CompareTo(IHandler other)
        {
            if (Token == other.Token)
            {
                return 0;
            }
            else if (Token < other.Token)
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
            if (obj == null || typeof(IHandler).IsAssignableFrom(obj.GetType()))
            {
                return false;
            }
            IHandler other = (IHandler)obj;
            return Method.Equals(other.Method);
        }

        public override int GetHashCode()
        {
            return Method.GetHashCode();
        }

        public void Invoke(Event e)
        {
            action((T)e);
        }
    }
}
