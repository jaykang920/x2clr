// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2
{
    /// <summary>
    /// Keepalive timer tick helper class.
    /// </summary>
    internal static class KeepaliveTicker
    {
        public const string Channel = "Keepalive";

        public static KeepaliveTick Event { get; private set; }

        static KeepaliveTicker()
        {
            Event = new KeepaliveTick { _Channel = Channel };
        }

        private static AtomicInt refCount = new AtomicInt();

        public static void ChangeRef(bool flag)
        {
            if (flag)
            {
                if (refCount.Increment() == 1)
                {
                    TimeFlow.Default.ReserveRepetition(Event,
                        new TimeSpan(0, 0, 5));

                    Log.Info("reserved keepalive ticks");
                }
            }
            else
            {
                if (refCount.Decrement() == 0)
                {
                    TimeFlow.Default.CancelRepetition(Event);

                    Log.Info("canceled keepalive ticks");
                }
            }
        }
    }
}
