// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2
{
    // x2 Coroutine and YieldInstructions will not work in MultiThreadFlow!

    public abstract class YieldInstruction : IEnumerator
    {
        // Alias of MoveNext()
        public bool Continue()
        {
            return MoveNext();
        }

        // IEnumerator interface implementation
        public virtual object Current { get { return null; } }
        public virtual bool MoveNext() { return false; }
        public void Reset() { }  // not supported
    }

    public class Coroutine
    {
        private IEnumerator routine;
        private bool running;
        private bool started;

        private Coroutine parent;

        public object Context { get; set; }

        public Coroutine()
        {
        }

        public Coroutine(Coroutine parent)
        {
            this.parent = parent;
        }

        public static void Start(Func<Coroutine, IEnumerator> func)
        {
            Coroutine coroutine = new Coroutine();
            coroutine.Start(func(coroutine));
        }

        public static void Start<T>(Func<Coroutine, T, IEnumerator> func, T arg)
        {
            Coroutine coroutine = new Coroutine();
            coroutine.Start(func(coroutine, arg));
        }

        public static void Start<T1, T2>(Func<Coroutine, T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
        {
            Coroutine coroutine = new Coroutine();
            coroutine.Start(func(coroutine, arg1, arg2));
        }

        public static void Start<T1, T2, T3>(Func<Coroutine, T1, T2, T3, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3)
        {
            Coroutine coroutine = new Coroutine();
            coroutine.Start(func(coroutine, arg1, arg2, arg3));
        }

        public void Start(IEnumerator routine)
        {
            this.routine = routine;
            running = (routine != null);
            Continue();
        }

        public bool Continue()
        {
            if (!started)
            {
                started = true;
            }
            else if (!running)
            {
                return false;
            }

            if (routine.MoveNext())
            {
                return true;
            }

            running = false;

            if (parent != null)
            {
                parent.Continue();
            }

            return false;
        }
    }
}
