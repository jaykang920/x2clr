// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2.Events
{
    // Internal event types
    public enum BuiltinType
    {
        FlowStart = -1,
        FlowStop = -2,

        // Link events
        LinkOpen = -3,
        LinkClose = -4,
        LinkSessionConnected = -5,
        LinkSessionDisconnected = -6,
        
        // TimeFlow events
        TimeoutEvent = -7,
        PeriodicEvent = -8,
    }
}
