// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// A reduced form of the ArraySegment(byte) struct, assuming a known
    /// <c>count</c> value.
    /// </summary>
    public struct Segment
    {
        private byte[] array;
        private int offset;

        public Segment(byte[] array, int offset)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            this.array = array;
            this.offset = offset;
        }

        public byte[] Array { get { return array; } }

        public int Offset { get { return offset; } } 

        public override int GetHashCode()
        {
            return array.GetHashCode() ^ offset;
        }

        public override bool Equals(Object obj)
        {
            if (obj is Segment)
            {
                return Equals((Segment)obj);
            }
            return false;
        }

        public bool Equals(Segment obj)
        {
            return obj.array == array && obj.offset == offset;
        }

        public static bool operator ==(Segment x, Segment y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Segment x, Segment y)
        {
            return !(x == y);
        }
    } 

    /// <summary>
    /// Manages a single large buffer block as if it's a pool of smaller
    /// fixed-length (2^n bytes) segments.
    /// </summary>
    public sealed class SegmentedBuffer
    {
        private static int chunkSize = Config.ChunkSize;
        private static int segmentSize = Config.SegmentSize;

        private byte[] buffer;
        private int currentOffset;
        private Stack<int> freeOffsetPool;

        private object syncRoot = new Object();

        public SegmentedBuffer()
        {
            freeOffsetPool = new Stack<int>();
            buffer = new byte[chunkSize];
        }

        public bool Acquire(ref Segment segment)
        {
            lock (freeOffsetPool)
            {
                if (freeOffsetPool.Count > 0)
                {
                    segment = new Segment(buffer, freeOffsetPool.Pop());
                    return true;
                }
            }

            int offset;
            lock (syncRoot)
            {
                if ((chunkSize - segmentSize) < currentOffset)
                {
                    return false;
                }

                offset = currentOffset;
                currentOffset += segmentSize;
            }
            segment = new Segment(buffer, offset);
            return true;
        }

        public bool Release(Segment segment)
        {
            if (segment.Array != buffer)
            {
                return false;
            }
            lock (freeOffsetPool)
            {
                freeOffsetPool.Push(segment.Offset);
            }
            return true;
        }
    }

    /// <summary>
    /// Manages a pool of fixed-length (2^n) byte array segments.
    /// </summary>
    public sealed class SegmentPool
    {
        private static List<SegmentedBuffer> pools;

        private static ReaderWriterLockSlim rwlock;

        static SegmentPool()
        {
            pools = new List<SegmentedBuffer>();
            rwlock = new ReaderWriterLockSlim();

            using (new WriteLock(rwlock))
            {
                pools.Add(new SegmentedBuffer());
            }
        }

        public static Segment Acquire()
        {
            Segment result = new Segment();
            SegmentedBuffer pool;
            using (new UpgradeableReadLock(rwlock))
            {
                for (int i = 0, count = pools.Count; i < count; ++i)
                {
                    if (pools[i].Acquire(ref result))
                    {
                        return result;
                    }
                }
                pool = new SegmentedBuffer();
                using (new WriteLock(rwlock))
                {
                    pools.Add(pool);
                }
            }
            pool.Acquire(ref result);
            return result;
        }

        public static void Release(Segment segment)
        {
            using (new ReadLock(rwlock))
            {
                for (int i = 0, count = pools.Count; i < count; ++i)
                {
                    if (pools[i].Release(segment))
                    {
                        return;
                    }
                }
            }
        }
    }
}
