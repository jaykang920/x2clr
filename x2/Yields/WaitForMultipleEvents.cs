﻿// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2
{
    /// <summary>
    /// YieldInstruction that waits for multiple events.
    /// </summary>
    public class WaitForMultipleEvents : YieldInstruction
    {
        private readonly Coroutine coroutine;
        private readonly Event[] expected, actual;

        private readonly Binder.Token[] handlerTokens;
        private readonly Binder.Token timeoutToken;
        private readonly Timer.Token? timerToken;

        private int count;
        private int waitHandle;

        public WaitForMultipleEvents(Coroutine coroutine, params Event[] e)
            : this(coroutine, null, Config.Coroutine.DefaultTimeout, e)
        {
        }

        public WaitForMultipleEvents(Coroutine coroutine, double seconds,
                params Event[] e)
            : this(coroutine, null, seconds, e)
        {
        }

        protected WaitForMultipleEvents(Coroutine coroutine, Event[] requests,
            double seconds, params Event[] e)
        {
            this.coroutine = coroutine;

            if (!Object.ReferenceEquals(requests, null))
            {
                waitHandle = WaitHandlePool.Acquire();
                for (int i = 0, count = requests.Length; i < count; ++i)
                {
                    requests[i]._WaitHandle = waitHandle;
                }
                for (int i = 0, count = e.Length; i < count; ++i)
                {
                    e[i]._WaitHandle = waitHandle;
                }
            }

            expected = e;
            actual = new Event[expected.Length];

            handlerTokens = new Binder.Token[expected.Length];
            for (int i = 0; i < expected.Length; ++i)
            {
                handlerTokens[i] = Flow.Bind(expected[i], OnEvent);
            }

            if (seconds > 0)
            {
                TimeoutEvent timeoutEvent = new TimeoutEvent { Key = this };
                timeoutToken = Flow.Bind(timeoutEvent, OnTimeout);
                timerToken = TimeFlow.Default.Reserve(timeoutEvent, seconds);
            }
        }

        void OnEvent(Event e)
        {
            for (int i = 0; i < expected.Length; ++i)
            {
                if (actual[i] == null && expected[i].Equivalent(e))
                {
                    Flow.Unbind(handlerTokens[i]);
                    handlerTokens[i] = new Binder.Token();
                    actual[i] = e;
                    ++count;
                    break;
                }
            }

            if (count >= expected.Length)
            {
                if (timerToken.HasValue)
                {
                    TimeFlow.Default.Cancel(timerToken.Value);
                    Flow.Unbind(timeoutToken);
                }

                if (waitHandle != 0)
                {
                    WaitHandlePool.Release(waitHandle);
                }

                coroutine.Context = actual;
                coroutine.Continue();
                coroutine.Context = null;
            }
        }

        void OnTimeout(TimeoutEvent e)
        {
            for (int i = 0, count = actual.Length; i < count; ++i)
            {
                if (Object.ReferenceEquals(actual[i], null))
                {
                    Flow.Unbind(handlerTokens[i]);
                }
            }
            Flow.Unbind(timeoutToken);

            if (waitHandle != 0)
            {
                WaitHandlePool.Release(waitHandle);
            }

            Log.Error("WaitForMultipleEvents timeout for {0}", expected);

            coroutine.Context = actual;  // incomplete array indicates timeout
            coroutine.Continue();
        }
    }

    /// <summary>
    /// YieldInstruction that posts requests and waits for multiple responses.
    /// </summary>
    public class WaitForMultipleResponses : WaitForMultipleEvents
    {
        public WaitForMultipleResponses(Coroutine coroutine, Event[] requests,
                params Event[] responses)
            : this(coroutine, requests, Config.Coroutine.DefaultTimeout, responses)
        {
        }

        public WaitForMultipleResponses(Coroutine coroutine, Event[] requests,
                double seconds, params Event[] responses)
            : base(coroutine, requests, seconds, responses)
        {
            if (Object.ReferenceEquals(requests, null))
            {
                throw new ArgumentNullException();
            }
            for (int i = 0; i < requests.Length; ++i)
            {
                requests[i].Post();
            }
        }
    }
}
