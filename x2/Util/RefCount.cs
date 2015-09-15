// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Atomic reference counting helper class.
    /// </summary>
    public struct RefCount
    {
        private int value;

        /// <summary>
        /// Increases the reference count, and returns the result value.
        /// </summary>
        public int Increment()
        {
            return Interlocked.Increment(ref value);
        }

        /// <summary>
        /// Decreases the reference count, and returns the result value.
        /// </summary>
        public int Decrement()
        {
            return Interlocked.Decrement(ref value);
        }

        /// <summary>
        /// Resets the reference count as zero, and returns the original value.
        /// </summary>
        public int Reset()
        {
            return Interlocked.Exchange(ref value, 0);
        }
    }
}
