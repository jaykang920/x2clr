// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;

using x2.Events;
using x2.Flows;

namespace x2.Yields
{
    // YieldInstruction that waits for the specified time in seconds.
    public class WaitForSeconds : YieldInstruction
    {
        private readonly Coroutine coroutine;
        private readonly Binder.Token binderToken;

        public WaitForSeconds(Coroutine coroutine, float seconds)
        {
            this.coroutine = coroutine;
            TimeoutEvent e = new TimeoutEvent { Key = this };
            binderToken = Flow.Bind(e, OnTimeoutEvent);
            TimeFlow.Token token = TimeFlow.Default.Reserve(e, seconds);
        }

        public override object Current { get { return null; } }

        public override bool MoveNext()
        {
            return false;
        }

        void OnTimeoutEvent(TimeoutEvent e)
        {
            Flow.Unbind(binderToken);

            coroutine.Context = e;
            coroutine.Continue();
            coroutine.Context = null;
        }
    }
}
