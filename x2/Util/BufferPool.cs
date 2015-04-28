// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2
{
    /// <summary>
    /// Static pool of 2^n length byte buffer blocks.
    /// </summary>
    public static class BufferPool
    {
        private const int maxSizeExponent = 20;  // 1MB
        private const int minSizeExponent = 4;   // 16-byte

        private static Pool<byte[]>[] pools;
        private static readonly int maxPoolSizeExponent = 0;

        static BufferPool()
        {
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
