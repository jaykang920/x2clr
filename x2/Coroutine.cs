// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2
{
    // x2 Coroutine and YieldInstructions will not work in MultiThreadedFlow!

    public abstract class YieldInstruction : IEnumerator
    {
        // Alias of MoveNext()
        public bool Continue()
        {
            return MoveNext();
        }

        // IEnumerator interface implementation
        public abstract object Current { get; }
        public abstract bool MoveNext();
        public void Reset() { }  // not supported
    }

    public class Coroutine : YieldInstruction
    {
        private IEnumerator routine;
        private bool running;
        private bool started;

        public object Context { get; set; }

        public override object Current
        {
            get { return (running ? routine.Current : null); }
        }

        public Coroutine()
        {
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

        public IEnumerator Start(IEnumerator routine)
        {
            this.routine = routine;
            running = (routine != null);
            MoveNext();
            return this;
        }

        public override bool MoveNext()
        {
            if (!started)
            {
                started = true;
                return routine.MoveNext();
            }

            if (running)
            {
                if (routine.Current.GetType().IsSubclassOf(typeof(Coroutine)))
                {
                    Coroutine coroutine = (Coroutine)routine.Current;
                    if (coroutine.running)
                    {
                        return true;
                    }

                }
                else if (routine.Current.GetType().IsSubclassOf(typeof(YieldInstruction)))
                {
                    YieldInstruction instruction = (YieldInstruction)routine.Current;
                    if (instruction.Continue())
                    {
                        return true;
                    }
                }
                if (routine.MoveNext())
                {
                    return true;
                }
                running = false;
            }
            return false;
        }
    }
}
