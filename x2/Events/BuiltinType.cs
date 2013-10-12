// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2.Events
{
    public enum BuiltinType
    {
        // Internal event types
        FlowStart = -1,
        FlowStop = -2,
        LinkSessionConnected = -3,
        LinkSessionDisconnected = -4,
        // TimeFlow
        TimeoutEvent = -5,
        PeriodicEvent = -6,
    }
}
