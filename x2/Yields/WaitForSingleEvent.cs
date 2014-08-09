// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;

using x2.Events;
using x2.Flows;

namespace x2.Yields
{
    /// <summary>
    /// YieldInstruction that waits for a single event.
    /// </summary>
    public class WaitForSingleEvent : YieldInstruction
    {
        private readonly Coroutine coroutine;
        private readonly Binder.Token handlerToken;
        private readonly Binder.Token timeoutToken;
        private readonly Timer.Token timerToken;

        public WaitForSingleEvent(Coroutine coroutine, Event e) : this(coroutine, e, 30.0)
        {
        }

        public WaitForSingleEvent(Coroutine coroutine, Event e, double seconds)
        {
            this.coroutine = coroutine;
            handlerToken = Flow.Bind(e, OnEvent);
            TimeoutEvent timeoutEvent = new TimeoutEvent { Key = this };
            timeoutToken = Flow.Bind(timeoutEvent, OnTimeout);
            timerToken = TimeFlow.Default.Reserve(timeoutEvent, seconds);
        }

        public override object Current { get { return null; } }

        public override bool MoveNext()
        {
            return false;
        }

        void OnEvent(Event e)
        {
            TimeFlow.Default.Cancel(timerToken);
            Flow.Unbind(timeoutToken);
            Flow.Unbind(handlerToken);

            coroutine.Context = e;
            coroutine.Continue();
            coroutine.Context = null;
        }

        void OnTimeout(TimeoutEvent e)
        {
            Flow.Unbind(timeoutToken);
            Flow.Unbind(handlerToken);

            coroutine.Context = null;  // indicates timeout
            coroutine.Continue();
        }
    }

    /// <summary>
    /// YieldInstruction that posts a request and waits for a single response.
    /// </summary>
    public class WaitForSingleResponse : WaitForSingleEvent
    {
        public WaitForSingleResponse(Coroutine coroutine, Event request, Event response)
            : base(coroutine, response)
        {
            request.Post();
        }

        public WaitForSingleResponse(Coroutine coroutine, Event request, Event response, double seconds)
            : base(coroutine, response, seconds)
        {
            request.Post();
        }
    }
}
