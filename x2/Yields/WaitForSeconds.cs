// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;

using x2.Events;
using x2.Flows;

namespace x2.Yields
{
    public class WaitForSeconds : YieldInstruction
    {
        private Coroutine coroutine;

        public WaitForSeconds(Coroutine coroutine, int seconds)
        {
            this.coroutine = coroutine;
            TimeoutEvent e = new TimeoutEvent { Key = 1 };
            Flow.Bind(e, OnTimeoutEvent);
            TimeFlow.Token token = TimeFlow.Default.Reserve(new TimeSpan(0, 0, seconds), e);
        }

        public override object Current
        {
            get
            {
                return null;
            }
        }

        public override bool MoveNext()
        {
            return false;
        }

        void OnTimeoutEvent(TimeoutEvent e)
        {
            coroutine.Context = e;
            coroutine.Continue();
            coroutine.Context = null;
        }
    }
}
