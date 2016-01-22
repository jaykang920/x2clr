// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2
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

        void OnTimeout(TimeoutEvent e)
        {
            Flow.Unbind(token);

            coroutine.Context = e;
            coroutine.Continue();
        }
    }

    public class WaitForNothing : YieldInstruction
    {
        private readonly Coroutine coroutine;
        private readonly object context;
        private readonly Binder.Token token;

        public WaitForNothing(Coroutine coroutine) : this(coroutine, null)
        {
        }

        public WaitForNothing(Coroutine coroutine, object context)
        {
            this.coroutine = coroutine;
            this.context = context;
            TimeoutEvent e = new TimeoutEvent { Key = this };
            token = Flow.Bind(e, OnTimeout);
            Hub.Post(e);
        }

        void OnTimeout(TimeoutEvent e)
        {
            Flow.Unbind(token);

            coroutine.Context = context;
            coroutine.Continue();
        }
    }
}
