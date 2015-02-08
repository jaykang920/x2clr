// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
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
        private readonly Stack<T> store;
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
        private const int minSizeExponent = 4;   // 16

        private static readonly Pool<byte[]>[] pools;
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
}
