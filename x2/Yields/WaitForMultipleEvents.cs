// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;
using System.Collections.Generic;

using x2.Events;
using x2.Flows;

namespace x2.Yields
{
    // YieldInstruction that waits for multiple events infinitely.
    public class WaitForMultipleEvents : YieldInstruction
    {
        private readonly Coroutine coroutine;
        private readonly Event[] expected;
        private readonly IList<Event> actual;

        public WaitForMultipleEvents(Coroutine coroutine, params Event[] e)
        {
            this.coroutine = coroutine;
            expected = e;
            actual = new List<Event>();

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
                if (expected[i].IsEquivalent(e))
                {
                    Flow.Unbind(expected[i], OnEvent);
                    actual.Add(e);
                    break;
                }
            }

            if (actual.Count >= expected.Length)
            {
                coroutine.Context = actual;
                coroutine.Continue();
                coroutine.Context = null;
            }
        }
    }
}
