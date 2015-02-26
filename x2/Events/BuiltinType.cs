// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2.Events
{
    // Internal event types
    public enum BuiltinType
    {
        // Flow events
        FlowStart = -1,
        FlowStop = -2,

        // TimeFlow events
        TimeoutEvent = -3,

        // Link events
        LinkSessionConnected = -4,
        LinkSessionDisconnected = -5,
#if SESSION_HANDOVER
        LinkSessionRecovered = -6,
#endif

        // -10 to -99 : reserved for links
    }
}
