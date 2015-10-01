﻿// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2
{
    /// <summary>
    /// A variable-length byte buffer class whose capacity is limited to a
    /// multiple of a power of 2.
    /// </summary>
    public class Buffer : IDisposable
    {
        private static int blockSizeExponent = Config.SegmentSizeExponent;
        private static int remainderMask = ~(~0 << blockSizeExponent);

        private List<Segment> blocks;

        private Segment currentBlock;
        private int currentBlockIndex;

        private int position;
        private int back;
        private int front;

        private int marker;

        // buffer room control
        private const int minLevel = 0;
        private const int maxLevel = 3;
        private int level;

        /// <summary>
        /// Gets the block size in bytes.
        /// </summary>
        public int BlockSize
        {
            get { return (1 << blockSizeExponent); }
        }

        /// <summary>
        /// Gets the maximum capacity of the buffer.
        /// </summary>
        public int Capacity
        {
            get { return (BlockSize * blocks.Count); }
        }

        /// <summary>
        /// Checks whether the buffer is empty (i.e. whether its length is 0).
        /// </summary>
        public bool IsEmpty
        {
            get { return (front == back); }
        }

        /// <summary>
        /// Gets the length of the buffered bytes.
        /// </summary>
        public long Length
        {
            get { return (long)(back - front); }
        }

        /// <summary>
        /// Gets or sets the current zero-based position.
        /// </summary>
        public long Position
        {
            get
            {
                return (long)(position - front);
            }
            set
            {
                int adjusted = (int)value + front;
                if (adjusted < front || back < adjusted)
                {
                    throw new IndexOutOfRangeException();
                }
                position = adjusted;
                int blockIndex = position >> blockSizeExponent;
                if ((blockIndex != 0) && ((position & remainderMask) == 0))
                {
                    --blockIndex;
                }
                if (blockIndex != currentBlockIndex)
                {
                    currentBlockIndex = blockIndex;
                    currentBlock = blocks[currentBlockIndex];
                }
            }
        }

        /// <summary>
        /// Gets the first segment of this buffer.
        /// </summary>
        public Segment FirstSegment
        {
            get
            {
                if (blocks.Count < 1)
                {
                    return new Segment();
                }
                return blocks[0];
            }
        }

        public Buffer()
        {
            if (blockSizeExponent < 0 || 31 < blockSizeExponent)
            {
                throw new ArgumentOutOfRangeException();
            }
            blocks = new List<Segment>();

            blocks.Add(SegmentPool.Acquire());

            currentBlockIndex = 0;
            currentBlock = blocks[currentBlockIndex];
            position = 0;
            back = 0;
            front = 0;

            marker = -1;
        }

        /// <summary>
        /// Destructor to return blocks to the pool
        /// </summary>
        ~Buffer()
        {
            CleanUp();
        }

        /// <summary>
        /// Implments IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        public void CopyFrom(byte[] buffer, int offset, int length)
        {
            EnsureCapacityToWrite(length);
            int blockIndex = position >> blockSizeExponent;
            int dstOffset = position & remainderMask;
            int bytesToCopy, bytesCopied = 0;
            while (bytesCopied < length)
            {
                bytesToCopy = Math.Min(BlockSize - dstOffset, length - bytesCopied);
                Segment block = blocks[blockIndex++];
                System.Buffer.BlockCopy(buffer, offset + bytesCopied,
                  block.Array, block.Offset + dstOffset, bytesToCopy);
                dstOffset = 0;
                bytesCopied += bytesToCopy;
            }
            Position = Position + length;
        }

        private void CopyTo(byte[] buffer, int offset, int length)
        {
            int blockIndex = offset >> blockSizeExponent;
            int srcOffset = offset & remainderMask;
            int bytesToCopy, bytesCopied = 0;
            while (bytesCopied < length)
            {
                bytesToCopy = Math.Min(BlockSize - srcOffset, length - bytesCopied);
                Segment block = blocks[blockIndex++];
                System.Buffer.BlockCopy(block.Array, block.Offset + srcOffset,
                  buffer, bytesCopied, bytesToCopy);
                srcOffset = 0;
                bytesCopied += bytesToCopy;
            }
        }

        public void ListOccupiedSegments(IList<ArraySegment<byte>> blockList)
        {
            ListSegments(blockList, front, back);
        }

        public void ListStartingSegments(IList<ArraySegment<byte>> blockList, int length)
        {
            ListSegments(blockList, front, front + length);
        }

        public void ListEndingSegments(IList<ArraySegment<byte>> blockList, int length)
        {
            ListSegments(blockList, back - length, back);
        }

        private void ListSegments(IList<ArraySegment<byte>> blockList, int begin, int end)
        {
            int beginIndex = begin >> blockSizeExponent;
            int beginOffset = begin & remainderMask;
            int endIndex = end >> blockSizeExponent;
            int endOffset = end & remainderMask;
            if (beginIndex == endIndex)
            {
                blockList.Add(new ArraySegment<byte>(
                    blocks[beginIndex].Array,
                    blocks[beginIndex].Offset + beginOffset,
                    endOffset - beginOffset));
                return;
            }
            blockList.Add(new ArraySegment<byte>(
                blocks[beginIndex].Array,
                blocks[beginIndex].Offset + beginOffset,
                BlockSize - beginOffset));
            for (int i = beginIndex + 1; i < endIndex; ++i)
            {
                blockList.Add(new ArraySegment<byte>(
                    blocks[i].Array,
                    blocks[i].Offset,
                    BlockSize));
            }
            if (endOffset != 0)
            {
                blockList.Add(new ArraySegment<byte>(
                    blocks[endIndex].Array,
                    blocks[endIndex].Offset,
                    endOffset));
            }
        }

        public void ListAvailableSegments(IList<ArraySegment<byte>> blockList)
        {
            if (back < Capacity)
            {
                if (level > minLevel) { --level; }
            }
            else
            {
                if (level < maxLevel) { ++level; }
            }
            int roomFactor = 1 << level;
            int numWholeBlocks = (Capacity - back) >> blockSizeExponent;
            if (numWholeBlocks < roomFactor)
            {
                int count = (roomFactor - numWholeBlocks);
                for (int i = 0; i < count; ++i)
                {
                    blocks.Add(SegmentPool.Acquire());
                }
            }

            int backIndex = back >> blockSizeExponent;
            int backOffset = back & remainderMask;
            blockList.Add(new ArraySegment<byte>(
                blocks[backIndex].Array,
                blocks[backIndex].Offset + backOffset,
                BlockSize - backOffset));
            for (int i = backIndex + 1, count = blocks.Count; i < count; ++i)
            {
                blockList.Add(new ArraySegment<byte>(
                    blocks[i].Array,
                    blocks[i].Offset,
                    BlockSize));
            }
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            CheckLengthToRead(count);
            CopyTo(buffer, position, count);
            Position = Position + count;
        }

        public void Reset()
        {
            Position = 0;
            back = front;
        }

        /// <summary>
        /// Alias of (Position = 0).
        /// </summary>
        public void Rewind()
        {
            Position = 0;
        }

        public void Shrink(int numBytes)
        {
            if ((front + numBytes) > back)
            {
                throw new ArgumentOutOfRangeException();
            }
            front += numBytes;
            if (position < front)
            {
                Position = 0;
            }
        }

        public void Stretch(int numBytes)
        {
            if ((back + numBytes) > Capacity)
            {
                throw new ArgumentOutOfRangeException();
            }
            back += numBytes;
        }

        public byte[] ToArray()
        {
            byte[] array = new byte[Length];
            CopyTo(array, front, (int)Length);
            return array;
        }

        /// <summary>
        /// Gets or sets the byte at the specified index.
        /// </summary>
        public byte this[int index]
        {
            get
            {
                index += front;
                Segment block = blocks[index >> blockSizeExponent];
                return block.Array[block.Offset + (index & remainderMask)];
            }
            set
            {
                index += front;
                Segment block = blocks[index >> blockSizeExponent];
                block.Array[block.Offset + (index & remainderMask)] = value;
            }
        }

        public void Trim()
        {
            int index, count;
            if (marker >= 0)
            {
                if (position < marker)
                {
                    Position = (marker - front);
                }
                marker = -1;
            }
            if (position == back)
            {
                index = 1;
                count = blocks.Count - 1;
                front = back = 0;
            }
            else
            {
                index = 0;
                count = position >> blockSizeExponent;
                if (count >= blocks.Count)
                {
                    count = blocks.Count - 1;
                }
                back -= BlockSize * count;
                front = position & remainderMask;
            }
            if (count > 0)
            {
                int roomFactor = 1 << level;
                List<Segment> blocksToRemove = blocks.GetRange(index, count);
                blocks.RemoveRange(index, count);
                for (int i = 0; i < blocksToRemove.Count; ++i)
                {
                    Segment block = blocksToRemove[i];
                    if (i < roomFactor)
                    {
                        blocks.Add(block);
                    }
                    else
                    {
                        SegmentPool.Release(blocksToRemove[i]);
                    }
                }
            }
            Position = 0;
        }

        public void MarkToRead(int lengthToRead)
        {
            if ((front + lengthToRead) > back)
            {
                throw new ArgumentOutOfRangeException();
            }
            marker = front + lengthToRead;
        }

        public void Write(byte[] value, int offset, int count)
        {
            //WriteVariable(count);
            CopyFrom(value, offset, count);
        }

        public void CheckLengthToRead(int numBytes)
        {
            int limit = (marker >= 0 ? marker : back);
            if ((position + numBytes) > limit)
            {
                Log.Warn("front={0} pos={1} back={2} marker={3} numBytes={4}", front, position, back, marker, numBytes);

                throw new System.IO.EndOfStreamException();
            }
        }

        public void EnsureCapacityToWrite(int numBytes)
        {
            int required = position + numBytes;
            while (required >= Capacity)
            {
                blocks.Add(SegmentPool.Acquire());
            }
            if (required > back)
            {
                back = required;
            }
        }

        public byte GetByte()
        {
            BlockFeed();
            return currentBlock.Array[currentBlock.Offset + (position++ & remainderMask)];
        }

        public void PutByte(byte value)
        {
            BlockFeed();
            currentBlock.Array[currentBlock.Offset + (position++ & remainderMask)] = value;
        }

        private void BlockFeed()
        {
            if (((position & remainderMask) == 0) &&
                ((position & ~remainderMask) != 0))
            {
                currentBlock = blocks[++currentBlockIndex];
            }
        }

        private void CleanUp()
        {
            if (blocks.Count == 0)
            {
                return;
            }
            for (int i = 0, count = blocks.Count; i < count; ++i)
            {
                SegmentPool.Release(blocks[i]);
            }
            blocks.Clear();
            currentBlock = new Segment();
        }
    }
}
