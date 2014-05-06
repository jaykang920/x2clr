// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;

using x2.Events;
using x2.Flows;

namespace x2.Yields
{
    /// <summary>
    /// YieldInstruction that waits for the specified time in seconds.
    /// </summary>
    public class WaitForSeconds : YieldInstruction
    {
        private readonly Coroutine coroutine;
        private readonly Binder.Token token;

        public WaitForSeconds(Coroutine coroutine, double seconds)
        {
            this.coroutine = coroutine;
            TimeoutEvent e = new TimeoutEvent { Key = this };
            token = Flow.Bind(e, OnTimeout);
            TimeFlow.Default.Reserve(e, seconds);
        }

        public override object Current { get { return null; } }

        public override bool MoveNext()
        {
            return false;
        }

        void OnTimeout(TimeoutEvent e)
        {
            Flow.Unbind(token);

            coroutine.Context = e;
            coroutine.Continue();
            coroutine.Context = null;
        }
    }
}
