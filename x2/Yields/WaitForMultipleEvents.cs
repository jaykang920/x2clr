// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;

using x2.Events;
using x2.Flows;

namespace x2.Yields
{
    /// <summary>
    /// YieldInstruction that waits for multiple events.
    /// </summary>
    public class WaitForMultipleEvents : YieldInstruction
    {
        private Coroutine coroutine;
        private Event[] expected, actual;
        private int count;

        public WaitForMultipleEvents(Coroutine coroutine, params Event[] e)
        {
            this.coroutine = coroutine;
            expected = e;
            actual = new Event[expected.Length];

            for (int i = 0; i < expected.Length; ++i)
            {
                Flow.Bind(expected[i], OnEvent);
            }
        }

        public override object Current { get { return null; } }

        public override bool MoveNext()
        {
            return false;
        }

        void OnEvent(Event e)
        {
            for (int i = 0; i < expected.Length; ++i)
            {
                if (actual[i] == null && expected[i].IsEquivalent(e))
                {
                    Flow.Unbind(expected[i], OnEvent);
                    actual[i] = e;
                    ++count;
                    break;
                }
            }

            if (count >= expected.Length)
            {
                coroutine.Context = actual;
                coroutine.Continue();
                coroutine.Context = null;
            }
        }
    }

    /// <summary>
    /// YieldInstruction that posts requests and waits for multiple responses.
    /// </summary>
    public class WaitForMultipleResponses : WaitForMultipleEvents
    {
        public WaitForMultipleResponses(Coroutine coroutine, Event[] requests, params Event[] responses)
            : base(coroutine, responses)
        {
            for (int i = 0; i < requests.Length; ++i)
            {
                requests[i].Post();
            }
        }
    }
}
