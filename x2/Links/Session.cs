// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

using x2.Flows;
using x2.Queues;

namespace x2.Links
{
    public class Session<T, U>
    {
        public T Id { get; set; }
        public Link.Session<U> LinkSession { get; set; }

        public void Send(Event e)
        {
            x2.Buffer buffer = new x2.Buffer(12);
            e.Serialize(buffer);
            LinkSession.Send(buffer);
        }
    }
}
