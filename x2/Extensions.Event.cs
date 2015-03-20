// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    // Extensions.Event
    public static partial class Extensions
    {
        public static Event AsResponse(this Event self, Event request)
        {
            if (request._Handle != 0)
            {
                self._Handle = request._Handle;
            }
            return self;
        }

        public static Event InChannel(this Event self, string channel)
        {
            self._Channel = channel;
            return self;
        }
    }
}
