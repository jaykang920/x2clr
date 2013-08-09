// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

using x2.Flows;
using x2.Queues;

namespace x2
{
    public abstract class Link : SingleThreadedFlow
    {
        protected const byte sentinel = 0x55;

        protected Link() : base(new UnboundedQueue<Event>()) {}

        public abstract void Close();

        public abstract class Session<T>
        {
            private readonly T handle;

            public T Handle { get { return handle; } }

            public Session(T handle)
            {
                this.handle = handle;
            }

            public abstract void Send(x2.Event e);
        }
    }
}
