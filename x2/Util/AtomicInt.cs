// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Provides interlocked wrapper methods for an <c>int</c> value.
    /// </summary>
    public class AtomicInt
    {
        private int value;

        /// <summary>
        /// Initializes a new instance of the AtomicInt class with zero.
        /// </summary>
        public AtomicInt()
        {
        }

        /// <summary>
        /// Initializes a new instance of the AtomicInt class with the specified
        /// initial value.
        /// </summary>
        public AtomicInt(int value)
        {
            Set(value);
        }

        /// <summary>
        /// Atomically decreases the value and returns the result.
        /// </summary>
        public int Decrement()
        {
            return Interlocked.Decrement(ref value);
        }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        public int Get()
        {
            return value;
        }

        /// <summary>
        /// Atomically increases the value and returns the result.
        /// </summary>
        public int Increment()
        {
            return Interlocked.Increment(ref value);
        }

        /// <summary>
        /// Atomically resets the value as zero, and returns the original value.
        /// </summary>
        public int Reset()
        {
            return Set(0);
        }

        /// <summary>
        /// Atomically sets the value as the specified one, and returns the
        /// original value.
        /// </summary>
        public int Set(int value)
        {
            return Interlocked.Exchange(ref this.value, value);
        }
    }
}
