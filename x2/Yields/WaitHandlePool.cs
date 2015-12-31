// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2
{
    public static class WaitHandlePool
    {
        private static RangedIntPool pool;

        static WaitHandlePool()
        {
            pool = new RangedIntPool(1, Config.Coroutine.MaxWaitHandles, true);
        }

        public static int Acquire()
        {
            return pool.Acquire();
        }

        public static void Release(int handle)
        {
            pool.Release(handle);
        }
    }
}
