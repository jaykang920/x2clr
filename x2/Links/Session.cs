// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace x2.Links
{
    public abstract class TransportSession<T>
    {
        private readonly T handle;

        public T Handle { get { return handle; } }

        public TransportSession(T handle)
        {
            this.handle = handle;
        }

        public abstract void Send(x2.Buffer buffer);
    }

    public class Session<T, U>
    {
        public T Id { get; set; }
        public TransportSession<U> TransportSession { get; set; }

        public void Send(Event e)
        {
            x2.Buffer buffer = new x2.Buffer(12);
            e.Serialize(buffer);
            TransportSession.Send(buffer);
        }
    }
}
