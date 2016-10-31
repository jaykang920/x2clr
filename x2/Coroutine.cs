// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2
{
    // [NOTE] x2 Coroutine(Yield) will not work in MultiThreadFlow!

    /// <summary>
    /// This thin wrapper of IEnumerator serves as an iterator on which x2
    /// coroutine works.
    /// </summary>
    public abstract class Yield : IEnumerator
    {
        // Alias of MoveNext()
        public bool Continue()
        {
            return MoveNext();
        }

        // IEnumerator interface implementation
        public virtual object Current { get { return null; } }
        public virtual bool MoveNext() { return false; }
        public void Reset()
        {
            throw new NotSupportedException();  // not supported
        }
    }

    /// <summary>
    /// Provides the core programming interface for x2 coroutines.
    /// </summary>
    public class Coroutine
    {
        private IEnumerator routine;
        private bool running;
        private bool started;
        private Coroutine parent;

        public object Result { get; set; }

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
            if (!Continue())
            {
                if (parent != null)
                {
                    // Indirectly chain into the parent coroutine.
                    new WaitForNothing(parent, Result);
                }
            }
        }

        public bool Continue()
        {
            if (!running)
            {
                return false;
            }

            if (routine.MoveNext())
            {
                started = true;
                return true;
            }

            running = false;

            if (started && parent != null)
            {
                // Chain into the parent coroutine.
                parent.Result = Result;
                parent.Continue();
            }

            return false;
        }

        // Inline coroutine activation methods (used with 'yield return' statements)

        /// <summary>
        /// Waits for the specified time in seconds.
        /// </summary>
        public Yield WaitForSeconds(double seconds)
        {
            return new WaitForSeconds(this, seconds);
        }

        /// <summary>
        /// Waits for a single event until the default timeout.
        /// </summary>
        public Yield WaitForSingleEvent(Event e)
        {
            return new WaitForSingleEvent(this, e);
        }

        /// <summary>
        /// Waits for a single event until the specified timeout in seconds.
        /// </summary>
        public Yield WaitForSingleEvent(Event e, double seconds)
        {
            return new WaitForSingleEvent(this, e, seconds);
        }

        /// <summary>
        /// Posts the request and waits for a single response until default timeout.
        /// </summary>
        public Yield WaitForSingleResponse(Event request, Event response)
        {
            return new WaitForSingleResponse(this, request, response);
        }

        /// <summary>
        /// Posts the request and waits for a single response until the specified
        /// timeout in seconds.
        /// </summary>
        public Yield WaitForSingleResponse(Event request, Event response,
            double seconds)
        {
            return new WaitForSingleResponse(this, request, response, seconds);
        }

        /// <summary>
        /// Waits for multiple events until the default timeout.
        /// </summary>
        public Yield WaitForMultipleEvents(params Event[] e)
        {
            return new WaitForMultipleEvents(this, e);
        }

        /// <summary>
        /// Waits for multiple events until the specified timeout in seconds.
        /// </summary>
        public Yield WaitForMultipleEvents(double seconds, params Event[] e)
        {
            return new WaitForMultipleEvents(this, seconds, e);
        }

        /// <summary>
        /// Posts the requests and waits for multiple responses until default
        /// timeout.
        /// </summary>
        public Yield WaitForMultipleResponses(Event[] requests,
            params Event[] responses)
        {
            return new WaitForMultipleResponses(this, requests, responses);
        }

        /// <summary>
        /// Posts the requests and waits for multiple responses until the
        /// specified timeout in seconds.
        /// </summary>
        public Yield WaitForMultipleResponse(Event[] requests, double seconds,
            params Event[] responses)
        {
            return new WaitForMultipleResponses(this, requests, seconds, responses);
        }

        /// <summary>
        /// Waits for the completion of another coroutine.
        /// </summary>
        public Yield WaitForCompletion(Func<Coroutine, IEnumerator> func)
        {
            return new WaitForCompletion(this, func);
        }

        /// <summary>
        /// Waits for the completion of another coroutine with a single
        /// additional argument.
        /// </summary>
        public Yield WaitForCompletion<T>(Func<Coroutine, T, IEnumerator> func,
            T arg)
        {
            return new WaitForCompletion<T>(this, func, arg);
        }

        /// <summary>
        /// Waits for the completion of another coroutine with two additional
        /// arguments.
        /// </summary>
        public Yield WaitForCompletion<T1, T2>(
            Func<Coroutine, T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
        {
            return new WaitForCompletion<T1, T2>(this, func, arg1, arg2);
        }

        /// <summary>
        /// Waits for the completion of another coroutine with three additional
        /// arguments.
        /// </summary>
        public Yield WaitForCompletion<T1, T2, T3>(
            Func<Coroutine, T1, T2, T3, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3)
        {
            return new WaitForCompletion<T1, T2, T3>(this, func, arg1, arg2, arg3);
        }
    }
}
