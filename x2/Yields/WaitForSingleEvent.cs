﻿// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2
{
    /// <summary>
    /// YieldInstruction that waits for a single event.
    /// </summary>
    public class WaitForSingleEvent : YieldInstruction
    {
        private readonly Coroutine coroutine;

        private readonly Binder.Token handlerToken;
        private readonly Binder.Token timeoutToken;
        private readonly Timer.Token? timerToken;

        public WaitForSingleEvent(Coroutine coroutine, Event e)
            : this(coroutine, null, e, Config.DefaultWaitTimeout)
        {
        }

        public WaitForSingleEvent(Coroutine coroutine, Event e, double seconds)
            : this(coroutine, null, e, seconds)
        {
        }

        protected WaitForSingleEvent(Coroutine coroutine, Event request, Event e, double seconds)
        {
            this.coroutine = coroutine;

            if (!Object.ReferenceEquals(request, null))
            {
                int waitHandle = WaitHandlePool.Acquire();
                request._WaitHandle = waitHandle;
                e._WaitHandle = waitHandle;
            }

            handlerToken = Flow.Bind(e, OnEvent);
            if (seconds > 0)
            {
                TimeoutEvent timeoutEvent = new TimeoutEvent { Key = this };
                timeoutToken = Flow.Bind(timeoutEvent, OnTimeout);
                timerToken = TimeFlow.Default.Reserve(timeoutEvent, seconds);
            }
        }

        public override object Current { get { return null; } }

        public override bool MoveNext()
        {
            return false;
        }

        void OnEvent(Event e)
        {
            WaitHandlePool.Release(handlerToken.Key._WaitHandle);

            Flow.Unbind(handlerToken);

            if (timerToken.HasValue)
            {
                TimeFlow.Default.Cancel(timerToken.Value);
                Flow.Unbind(timeoutToken);
            }

            coroutine.Context = e;
            coroutine.Continue();
            coroutine.Context = null;
        }

        void OnTimeout(TimeoutEvent e)
        {
            WaitHandlePool.Release(handlerToken.Key._WaitHandle);

            Flow.Unbind(handlerToken);
            Flow.Unbind(timeoutToken);

            Log.Error("WaitForSingleEvent timeout for {0}", handlerToken.Key);

            coroutine.Context = null;  // indicates timeout
            coroutine.Continue();
        }
    }

    /// <summary>
    /// YieldInstruction that posts a request and waits for a single response.
    /// </summary>
    public class WaitForSingleResponse : WaitForSingleEvent
    {
        public WaitForSingleResponse(Coroutine coroutine, Event request,
                Event response)
            : this(coroutine, request, response, Config.DefaultWaitTimeout)
        {
        }

        public WaitForSingleResponse(Coroutine coroutine, Event request,
                Event response, double seconds)
            : base(coroutine, request, response, seconds)
        {
            if (Object.ReferenceEquals(request, null))
            {
                throw new ArgumentNullException();
            }
            request.Post();
        }
    }
}
