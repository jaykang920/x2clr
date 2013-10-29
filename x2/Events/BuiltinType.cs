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
        LinkRetry = -4,
        LinkClose = -5,
        LinkSessionConnected = -6,
        LinkSessionDisconnected = -7,
        
        // TimeFlow events
        TimeoutEvent = -8,
        PeriodicEvent = -9,
    }
}
