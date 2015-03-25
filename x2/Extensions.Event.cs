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
        /// <summary>
        /// Indicates that the event is the response of the specified one.
        /// </summary>
        public static T InResponseOf<T>(this T self, Event request) where T : Event
        {
            if (request._Handle != 0)
            {
                self._Handle = request._Handle;
            }
            return self;
        }

        /// <summary>
        /// Indicates that the event is associated with the specified hub
        /// channel.
        /// </summary>
        public static Event InChannel(this Event self, string channel)
        {
            self._Channel = channel;
            return self;
        }
    }
}
