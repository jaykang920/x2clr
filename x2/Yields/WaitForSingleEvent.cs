// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;

using x2.Events;
using x2.Flows;

namespace x2.Yields
{
    // YieldInstruction that waits for a single event infinitely.
    public class WaitForSingleEvent : YieldInstruction
    {
        private readonly Coroutine coroutine;
        private readonly Binder.Token token;

        public WaitForSingleEvent(Coroutine coroutine, Event e)
        {
            this.coroutine = coroutine;
            token = Flow.Bind(e, OnEvent);
        }

        public override object Current { get { return null; } }

        public override bool MoveNext()
        {
            return false;
        }

        void OnEvent(Event e)
        {
            Flow.Unbind(token);

            coroutine.Context = e;
            coroutine.Continue();
            coroutine.Context = null;
        }
    }
}
