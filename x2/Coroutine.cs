// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;

namespace x2
{
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
        public void Reset() { }  // not supported in iterator block
    }

    public class Coroutine : YieldInstruction
    {
        private IEnumerator routine;
        private bool running;
        private bool started;

        private Coroutine(IEnumerator routine)
        {
            this.routine = routine;
            running = (routine != null);
        }

        public override object Current
        {
            get { return (running ? routine.Current : null); }
        }

        public static Coroutine Start(IEnumerator routine)
        {
            Coroutine coroutine = new Coroutine(routine);
            coroutine.Continue();
            return coroutine;
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
                else
                {
                    if (routine.MoveNext())
                    {
                        return true;
                    }
                }
                running = false;
            }
            return false;
        }
    }
}
