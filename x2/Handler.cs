// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace x2
{
    public delegate void HandlerMethod<TEvent>(TEvent e)
        where TEvent : Event;

    internal delegate void IHM<TTarget, TEvent>(TTarget target, TEvent e)
        where TEvent : Event;

    public delegate void InstanceHandlerMethod<TEvent>(object target, TEvent e)
        where TEvent : Event;

    public abstract class Handler : IComparable<Handler>
    {
        protected readonly MethodInfo methodInfo;

        protected Handler(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public static Handler Create<TEvent>(HandlerMethod<TEvent> handler)
            where TEvent : Event
        {
            if (handler.Target == null)
            {
                return new StaticMethodHandler<TEvent>(handler);
            }
            else
            {
                return new InstanceMethodHandler<TEvent>(handler);
            }
        }

        public static Handler Create<TTarget, TEvent>(TTarget target,
                                                      HandlerMethod<TEvent> handler)
            where TTarget : class
            where TEvent : Event
        {
            return new IMH<TTarget, TEvent>(target, handler);
        }

        public int CompareTo(Handler other)
        {
            long token = GetToken();
            long otherToken = other.GetToken();
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

        private long GetToken()
        {
            return (long)methodInfo.Module.MetadataToken << 32 + methodInfo.MetadataToken;
        }

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
            return methodInfo.Equals(other.methodInfo);
        }

        public override int GetHashCode()
        {
            return methodInfo.GetHashCode();
        }

        public void Combine(Handler other)
        {
        }

        public abstract void Invoke(Event e);

        public bool Remove(Handler other)
        {
            return true;
        }
    }

    public class IMH<TTarget, TEvent> : Handler
        where TTarget : class
        where TEvent : Event
    {
        private IHM<TTarget, TEvent> handler;
        private readonly WeakReference weakReference;

        public IMH(TTarget target, HandlerMethod<TEvent> handler)
            : base(handler.Method)
        {
            this.handler = (IHM<TTarget, TEvent>)Delegate.CreateDelegate(
                typeof(IHM<TTarget, TEvent>), handler.Method);
            weakReference = new WeakReference(target);
        }

        public override void Invoke(Event e)
        {
            TTarget target = weakReference.Target as TTarget;
            if (target != null)
            {
                handler(target, (TEvent)e);
            }
        }
    }

    public class InstanceMethodHandler<TEvent> : Handler
        where TEvent : Event
    {
        private InstanceHandlerMethod<TEvent> handler;
        private object target;

        public InstanceMethodHandler(HandlerMethod<TEvent> handler)
            : base(handler.Method)
        {
            this.handler = (InstanceHandlerMethod<TEvent>)Delegate.CreateDelegate(
                typeof(InstanceHandlerMethod<TEvent>), handler.Method);
            this.target = handler.Target;
        }

        public override void Invoke(Event e)
        {
            handler(target, (TEvent)e);
        }
    }

    public class StaticMethodHandler<TEvent> : Handler
        where TEvent : Event
    {
        private HandlerMethod<TEvent> handler;

        public StaticMethodHandler(HandlerMethod<TEvent> handler)
            : base(handler.Method)
        {
            this.handler = handler;
        }

        public override void Invoke(Event e)
        {
            handler((TEvent)e);
        }
    }
}
