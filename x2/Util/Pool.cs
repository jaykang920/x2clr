// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace x2
{
    /// <summary>
    /// Minimal generic object pool.
    /// </summary>
    /// <typeparam name="T">
    /// The type of objects in the pool.
    /// </typeparam>
    public class Pool<T> where T : class
    {
        private Stack<T> store;
        private readonly int capacity;

        /// <summary>
        /// Gets the maximum number of objects that can be contained in the pool.
        /// </summary>
        public int Capacity { get { return capacity; } }

        /// <summary>
        /// Gets the number of objects contained in the pool.
        /// </summary>
        public int Count { get { return store.Count; } }

        /// <summary>
        /// Initializes a new instance of the Pool(T) class, without a capacity
        /// limit.
        /// </summary>
        public Pool()
        {
            store = new Stack<T>();
        }

        /// <summary>
        /// Initializes a new instance of the Pool(T) class, with the specified
        /// maximum capacity.
        /// </summary>
        /// <param name="capacity"></param>
        public Pool(int capacity)
        {
            store = new Stack<T>();
            this.capacity = capacity;
        }

        /// <summary>
        /// Tries to pop an object out of the pool.
        /// </summary>
        /// <returns>
        /// The object removed from the pool, or null if the pool is empty.
        /// </returns>
        public T Pop()
        {
            lock (store)
            {
                if (store.Count != 0)
                {
                    return store.Pop();
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to push the specified object into the pool.
        /// </summary>
        /// <remarks>
        /// If the pool has a non-zero capacity limit, the object may be dropped
        /// when the number of pooled objects reaches the capacity.
        /// </remarks>
        public void Push(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }

            lock (store)
            {
                if (capacity == 0 || store.Count < capacity)
                {
                    store.Push(item);
                }
            }
        }
    }

    /// <summary>
    /// Pool of 2^n length byte buffer blocks.
    /// </summary>
    public static class BufferPool
    {
        private const int maxSizeExponent = 28;  // 256M
        private const int minSizeExponent = 1;   // 2

        private static Pool<byte[]>[] pools;
        private static readonly int maxPoolSizeExponent = 0;

        static BufferPool()
        {
            Debug.Assert(maxPoolSizeExponent < 32);
            Debug.Assert(maxSizeExponent < 32);
            Debug.Assert(minSizeExponent > 0);
            Debug.Assert(minSizeExponent <= maxSizeExponent);

            pools = new Pool<byte[]>[maxSizeExponent - minSizeExponent + 1];
        }

        /// <summary>
        /// Acquires a byte buffer block of length 2^sizeExponent from the pool.
        /// </summary>
        public static byte[] Acquire(int sizeExponent)
        {
            if (sizeExponent < minSizeExponent ||
                sizeExponent > maxSizeExponent)
            {
                throw new ArgumentOutOfRangeException();
            }

            Pool<byte[]> pool;
            lock (pools)
            {
                int index = sizeExponent - minSizeExponent;
                pool = pools[index];
                if (pool == null)
                {
                    if (maxPoolSizeExponent == 0)
                    {
                        pool = new Pool<byte[]>();
                    }
                    else
                    {
                        int capacity =
                            (1 << maxPoolSizeExponent) / (1 << sizeExponent);
                        if (capacity < 1)
                        {
                            capacity = 1;
                        }
                        pool = new Pool<byte[]>(capacity);
                    }
                    pools[index] = pool;
                }
            }
            byte[] block = pool.Pop();
            if (block == null)
            {
                block = new byte[1 << sizeExponent];
            }
            return block;
        }

        /// <summary>
        /// Releases the specified byte buffer block and pushes it back into the
        /// pool.
        /// </summary>
        public static void Release(int sizeExponent, byte[] block)
        {
            if (sizeExponent < minSizeExponent ||
                sizeExponent > maxSizeExponent)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (block == null)
            {
                throw new ArgumentNullException();
            }
            if (block.Length != (1 << sizeExponent))
            {
                throw new ArgumentException();
            }

            Pool<byte[]> pool;
            lock (pools)
            {
                pool = pools[sizeExponent - minSizeExponent];
            }

            if (pool == null)
            {
                throw new ArgumentException();
            }

            pool.Push(block);
        }
    }

    /// <summary>
    /// Compact pool of consecutive Int32 values in a finite range.
    /// </summary>
    public class RangedInt32Pool
    {
        private bool advancing;
        private int minValue;
        private int maxValue;
        private int offset;
        private BitArray bitArray;

        /// <summary>
        /// Gets the number of consecutive integers handled by this pool.
        /// </summary>
        public int Length { get { return bitArray.Length; } }

        /// <summary>
        /// Initializes a new instance of the RangedInt32Pool class, containing
        /// integers of range [0, maxValue].
        /// </summary>
        public RangedInt32Pool(int maxValue)
            : this (0, maxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RangedInt32Pool class with the
        /// specified circulation behavior, containing integers of range
        /// [0, maxValue].
        /// </summary>
        public RangedInt32Pool(int maxValue, bool advancing)
            : this(0, maxValue, advancing)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RangedInt32Pool class, containing
        /// integers of range [minValue, maxValue].
        /// </summary>
        public RangedInt32Pool(int minValue, int maxValue)
            : this (minValue, maxValue, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RangedInt32Pool class with the
        /// specified circulation behavior, containing integers of range
        /// [minValue, maxValue].
        /// </summary>
        public RangedInt32Pool(int minValue, int maxValue, bool advancing)
        {
            this.advancing = advancing;
            Debug.Assert(maxValue < Int32.MaxValue);
            Debug.Assert(minValue <= maxValue);
            this.minValue = minValue;
            this.maxValue = maxValue;
            bitArray = new BitArray(maxValue - minValue + 1);
        }

        /// <summary>
        /// Gets the next available value from the pool.
        /// </summary>
        /// <returns></returns>
        public int Acquire()
        {
            int index = offset;
            for (int i = 0, length = Length; i < length; ++i, ++index)
            {
                if (index >= length)
                {
                    index = 0;
                }
                if (!bitArray[index])
                {
                    bitArray.Set(index, true);
                    if (advancing)
                    {
                        offset = index + 1;
                        if (offset >= length)
                        {
                            offset = 0;
                        }
                    }
                    return (minValue + index);
                }
            }
            throw new OutOfResourceException();
        }
        
        /// <summary>
        /// Marks the specified value as used in the pool.
        /// </summary>
        public bool Claim(int value)
        {
            int index = value - minValue;
            if (bitArray[index])
            {
                return false;
            }
            bitArray.Set(index, true);
            return true;
        }

        /// <summary>
        /// Returns the specified value to the pool.
        /// </summary>
        public void Release(int value)
        {
            int index = value - minValue;
            if (bitArray[index])
            {
                bitArray.Set(index, false);
            }
        }
    }
}
