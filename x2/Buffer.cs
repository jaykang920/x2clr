// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    /// <summary>
    /// A variable-length byte buffer class that provides methods to read/write
    /// primitive data types from/to byte sequences in a platform-independent way.
    /// </summary>
    /// The buffer length is limited to a multiple of a power of 2.
    public class Buffer
    {
        private readonly List<byte[]> blocks;
        private readonly int blockSizeExponent;
        private readonly int remainderMask;

        private byte[] currentBlock;
        private int currentBlockIndex;
        private int position;
        private int back;
        private int front;

        private int marker;

        private int BlockSize
        {
            get { return (1 << blockSizeExponent); }
        }

        private int Capacity
        {
            get { return (BlockSize * blocks.Count); }
        }

        public bool IsEmpty
        {
            get { return (front == back); }
        }

        public long Length
        {
            get { return (long)(back - front); }
        }

        public long Position
        {
            get
            {
                return (long)(position - front);
                //return position;
            }
            set
            {
                int v = (int)value;
                v += front;

                if (v < front || back < v)
                {
                    throw new IndexOutOfRangeException();
                }
                position = v;
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

        public Buffer(int blockSizeExponent)
        {
            if (blockSizeExponent < 0 || 31 < blockSizeExponent)
            {
                throw new ArgumentOutOfRangeException();
            }
            blocks = new List<byte[]>();
            this.blockSizeExponent = blockSizeExponent;
            remainderMask = ~(~0 << blockSizeExponent);

            blocks.Add(BufferPool.Acquire(blockSizeExponent));

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
        /// Encodes a variable-length 32-bit non-negative integer into the given
        /// buffer, and returns the number of resultant bytes.
        /// </summary>
        public static int WriteVariable(byte[] buffer, int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            return WriteVariable(buffer, (uint)value);
        }

        /// <summary>
        /// Encodes a variable-length 32-bit unsigned integer into the given
        /// buffer, and returns the number of resultant bytes.
        /// </summary>
        public static int WriteVariable(byte[] buffer, uint value)
        {
            if ((value & 0xffffff80) == 0)
            {
                if (buffer.Length < 1)
                {
                    throw new ArgumentException();
                }
                buffer[0] = (byte)value;
                return 1;
            }

            if ((value & 0xffffc000) == 0)
            {
                if (buffer.Length < 2)
                {
                    throw new ArgumentException();
                }
                buffer[0] = (byte)(value | 0x80);
                buffer[1] = (byte)((value >> 7) & 0x7f);
                return 2;
            }

            if ((value & 0xffe00000) == 0)
            {
                if (buffer.Length < 3)
                {
                    throw new ArgumentException();
                }
                buffer[0] = (byte)(value | 0x80);
                buffer[1] = (byte)((value >> 7) | 0x80);
                buffer[2] = (byte)((value >> 14) & 0x7f);
                return 3;
            }

            if ((value & 0xf0000000) == 0)
            {
                if (buffer.Length < 4)
                {
                    throw new ArgumentException();
                }
                buffer[0] = (byte)(value | 0x80);
                buffer[1] = (byte)((value >> 7) | 0x80);
                buffer[2] = (byte)((value >> 14) | 0x80);
                buffer[3] = (byte)((value >> 21) & 0x7f);
                return 4;
            }

            if (buffer.Length < 5)
            {
                throw new ArgumentException();
            }
            buffer[0] = (byte)(value | 0x80);
            buffer[3] = (byte)((value >> 7) | 0x80);
            buffer[2] = (byte)((value >> 14) | 0x80);
            buffer[3] = (byte)((value >> 21) | 0x80);
            buffer[4] = (byte)((value >> 28) & 0x0f);
            return 5;
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
                System.Buffer.BlockCopy(buffer, offset + bytesCopied,
                  blocks[blockIndex++], dstOffset, bytesToCopy);
                dstOffset = 0;
                bytesCopied += bytesToCopy;
            }
            Position = position + length;
        }

        private void CopyTo(byte[] buffer, int offset, int length)
        {
            int blockIndex = offset >> blockSizeExponent;
            int srcOffset = offset & remainderMask;
            int bytesToCopy, bytesCopied = 0;
            while (bytesCopied < length)
            {
                bytesToCopy = Math.Min(BlockSize - srcOffset, length - bytesCopied);
                System.Buffer.BlockCopy(blocks[blockIndex++], srcOffset,
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
                blockList.Add(new ArraySegment<byte>(blocks[beginIndex], beginOffset,
                                                     endOffset - beginOffset));
                return;
            }
            blockList.Add(new ArraySegment<byte>(blocks[beginIndex], beginOffset,
                                                 BlockSize - beginOffset));
            for (int i = beginIndex + 1; i < endIndex; ++i)
            {
                blockList.Add(new ArraySegment<byte>(blocks[i]));
            }
            if (endOffset != 0)
            {
                blockList.Add(new ArraySegment<byte>(blocks[endIndex], 0, endOffset));
            }
        }

        public void ListAvailableSegments(IList<ArraySegment<byte>> blockList)
        {
            if ((Capacity - back) < BlockSize)
            {
                blocks.Add(BufferPool.Acquire(blockSizeExponent));
            }
            int backIndex = back >> blockSizeExponent;
            int backOffset = back & remainderMask;
            blockList.Add(new ArraySegment<byte>(blocks[backIndex], backOffset,
                                                 BlockSize - backOffset));
            for (int i = backIndex + 1; i < blocks.Count; ++i)
            {
                blockList.Add(new ArraySegment<byte>(blocks[i]));
            }
        }

        public void Read(out bool value)
        {
            value = (ReadByte() > 0);
        }

        public void Read(out sbyte value)
        {
            value = (sbyte)ReadByte();
        }

        public void Read(out byte value)
        {
            value = (byte)ReadByte();
        }

        public void Read(out byte[] value)
        {
            int length;
            ReadVariable(out length);
            CheckLengthToRead(length);
            value = new byte[length];
            CopyTo(value, position, length);
            Position = position + length;
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            CheckLengthToRead(count);
            CopyTo(buffer, position, count);
            Position = position + count;
        }

        public void Read(out short value)
        {
            CheckLengthToRead(2);
            value = GetByte();
            value = (short)((value << 8) | GetByte());
        }

        /// <summary>
        /// Decode variable-length 32-bit signed integer from this buffer.
        /// </summary>
        public int Read(out int value)
        {
            // Zigzag decoding
            uint u;
            int bytes = ReadVariable(out u);
            value = (int)((int)(u >> 1) ^ -((int)u & 1));
            return bytes;
        }

        /// <summary>
        /// Decode variable-length 64-bit signed integer from this buffer.
        /// </summary>
        public int Read(out long value)
        {
            // Zigzag decoding
            ulong u;
            int bytes = ReadVariable(out u);
            value = (long)((long)(u >> 1) ^ -((long)u & 1));
            return bytes;
        }

        public void ReadFixed(out int value)
        {
            CheckLengthToRead(4);
            value = GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
        }

        public void ReadFixed(out long value)
        {
            CheckLengthToRead(8);
            value = GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
        }

        public void Read(out float value)
        {
            int i;
            ReadFixed(out i);
            value = System.BitConverter.ToSingle(System.BitConverter.GetBytes(i), 0);
        }

        public void Read(out double value)
        {
            long l;
            ReadFixed(out l);
            value = System.BitConverter.ToDouble(System.BitConverter.GetBytes(l), 0);
        }

        public void Read(out string value)
        {
            int length;
            ReadVariable(out length);
            if (length == 0)
            {
                value = String.Empty;
                return;
            }
            CheckLengthToRead(length);
            char c, c2, c3;
            int bytesRead = 0;
            StringBuilder stringBuilder = new StringBuilder(length);
            while (bytesRead < length)
            {
                c = (char)GetByte();
                switch (c >> 4)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        // 0xxxxxxx
                        ++bytesRead;
                        stringBuilder.Append(c);
                        break;
                    case 12:
                    case 13:
                        // 110x xxxx  10xx xxxx
                        bytesRead += 2;
                        if (bytesRead > length)
                        {
                            throw new Exception("Invalid UTF-8 stream");
                        }
                        c2 = (char)GetByte();
                        if ((c2 & 0xC0) != 0x80)
                        {
                            throw new Exception("Invalid UTF-8 stream");
                        }
                        stringBuilder.Append((char)(((c & 0x1F) << 6) | (c2 & 0x3F)));
                        break;
                    case 14:
                        // 1110 xxxx  10xx xxxx  10xx xxxx
                        bytesRead += 3;
                        if (bytesRead > length)
                        {
                            throw new Exception("Invalid UTF-8 stream");
                        }
                        c2 = (char)GetByte();
                        c3 = (char)GetByte();
                        if (((c2 & 0xC0) != 0x80) || ((c3 & 0xC0) != 0x80))
                        {
                            throw new Exception("Invalid UTF-8 stream");
                        }
                        stringBuilder.Append((char)(((c & 0x0F) << 12) |
                          ((c2 & 0x3F) << 6) | ((c3 & 0x3F) << 0)));
                        break;
                    default:
                        // 10xx xxxx  1111 xxxx
                        throw new Exception("Invalid UTF-8 stream");
                }
            }
            value = stringBuilder.ToString();
        }

        public void Read(out DateTime value)
        {
            long usecs;
            ReadFixed(out usecs);
            DateTime unixEpoch = new DateTime(621355968000000000);
            value = unixEpoch.AddTicks(usecs * 10);
        }

        /*
        public byte ReadByte()
        {
            CheckLengthToRead(1);
            return GetByte();
        }
        */

        /// <summary>
        /// Decodes variable-length 32-bit non-negative integer from this buffer.
        /// </summary>
        public int ReadVariable(out int value)
        {
            uint unsigned;
            int result = ReadVariable(out unsigned);
            if (unsigned > Int32.MaxValue)
            {
                throw new OverflowException();
            }
            value = (int)unsigned;
            return result;
        }

        /// <summary>
        /// Decodes variable-length 32-bit unsigned integer from this buffer.
        /// </summary>
        public int ReadVariable(out uint value)
        {
            value = 0U;
            int i, shift = 0;
            for (i = 0; i < 5; ++i)
            {
                CheckLengthToRead(1);
                byte b = GetByte();
                value |= (((uint)b & 0x7fU) << shift);
                if ((b & 0x80) == 0)
                {
                    break;
                }
                shift += 7;
            }
            return (i < 5 ? (i + 1) : 5);
        }

        /// <summary>
        /// Decode variable-length 64-bit unsigned integer from this buffer.
        /// </summary>
        public int ReadVariable(out ulong value)
        {
            value = 0UL;
            int i, shift = 0;
            for (i = 0; i < 10; ++i)
            {
                CheckLengthToRead(1);
                byte b = GetByte();
                value |= (((ulong)b & 0x7fU) << shift);
                if ((b & 0x80) == 0)
                {
                    break;
                }
                shift += 7;
            }
            return (i < 10 ? (i + 1) : 10);
        }

        public void Rewind()
        {
            Position = front;
        }

        public void Shrink(int numBytes)
        {
            if ((front + numBytes) > back)
            {
                throw new IndexOutOfRangeException();
            }
            front += numBytes;
            if (position < front)
            {
                Position = front;
            }
        }

        public void Stretch(int numBytes)
        {
            if ((back + numBytes) > Capacity)
            {
                throw new IndexOutOfRangeException();
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
                return blocks[index >> blockSizeExponent][index & remainderMask];
            }
            set
            {
                index += front;
                blocks[index >> blockSizeExponent][index & remainderMask] = value;
            }
        }

        public void Trim()
        {
            int index, count;
            if (marker >= 0)
            {
                if (position < marker)
                {
                    Position = marker;
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
                List<byte[]> blocksToRemove = blocks.GetRange(index, count);
                blocks.RemoveRange(index, count);
                for (int i = 0; i < blocksToRemove.Count; ++i)
                {
                    BufferPool.Release(blockSizeExponent, blocksToRemove[i]);
                }
            }
            Position = front;
        }

        public void MarkToRead(int lengthToRead)
        {
            if ((front + lengthToRead) > back)
            {
                throw new IndexOutOfRangeException();
            }
            marker = front + lengthToRead;
        }

        public void Write(bool value)
        {
            EnsureCapacityToWrite(1);
            byte b = (byte)(value ? 1 : 0);
            PutByte(b);
        }

        public void Write(sbyte value)
        {
            EnsureCapacityToWrite(1);
            PutByte((byte)value);
        }

        public void Write(byte value)
        {
            EnsureCapacityToWrite(1);
            PutByte(value);
        }

        public void Write(byte[] value)
        {
            Write(value, value.GetLowerBound(0), value.Length);
        }

        public void Write(byte[] value, int offset, int count)
        {
            WriteVariable(count);
            CopyFrom(value, offset, count);
        }

        public void Write(short value)
        {
            EnsureCapacityToWrite(2);
            PutByte((byte)(value >> 8));
            PutByte((byte)value);
        }

        /// <summary>
        /// Encode variable-length 32-bit signed integer into this buffer.
        /// </summary>
        public void Write(int value)
        {
            // Zigzag encoding
            WriteVariable((uint)((value << 1) ^ (value >> 31)));
        }

        /// <summary>
        /// Encode variable-length 64-bit signed integer into this buffer.
        /// </summary>
        public void Write(long value)
        {
            // Zigzag encoding
            WriteVariable((ulong)((value << 1) ^ (value >> 63)));
        }

        public void WriteFixed(int value)
        {
            EnsureCapacityToWrite(4);
            PutByte((byte)(value >> 24));
            PutByte((byte)(value >> 16));
            PutByte((byte)(value >> 8));
            PutByte((byte)value);
        }

        public void WriteFixed(long value)
        {
            EnsureCapacityToWrite(8);
            PutByte((byte)(value >> 56));
            PutByte((byte)(value >> 48));
            PutByte((byte)(value >> 40));
            PutByte((byte)(value >> 32));
            PutByte((byte)(value >> 24));
            PutByte((byte)(value >> 16));
            PutByte((byte)(value >> 8));
            PutByte((byte)value);
        }
        
        public void Write(float value)
        {
            WriteFixed(System.BitConverter.ToInt32(System.BitConverter.GetBytes(value), 0));
        }

        public void Write(double value)
        {
            WriteFixed(System.BitConverter.ToInt64(System.BitConverter.GetBytes(value), 0));
        }

        public void Write(string value)
        {
            int length = 0;
            for (int i = 0, count = value.Length; i < count; ++i)
            {
                var c = value[i];

                if ((c & 0xFF80) == 0)
                {
                    ++length;
                }
                else if ((c & 0xF800) != 0)
                {
                    length += 3;
                }
                else
                {
                    length += 2;
                }
            }
            WriteVariable(length);
            EnsureCapacityToWrite(length);
            for (int i = 0, count = value.Length; i < count; ++i)
            {
                var c = value[i];

                if ((c & 0xFF80) == 0)
                {
                    PutByte((byte)c);
                }
                else if ((c & 0xF800) != 0)
                {
                    PutByte((byte)(0xE0 | ((c >> 12) & 0x0F)));
                    PutByte((byte)(0x80 | ((c >> 6) & 0x3F)));
                    PutByte((byte)(0x80 | ((c >> 0) & 0x3F)));
                }
                else
                {
                    PutByte((byte)(0xC0 | ((c >> 6) & 0x1F)));
                    PutByte((byte)(0x80 | ((c >> 0) & 0x3F)));
                }
            }
        }

        public void Write(DateTime value)
        {
            long usecs = (value.Ticks - 621355968000000000) / 10;
            WriteFixed(usecs);
        }

        public void WriteVariable(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            WriteVariable((uint)value);
        }

        /// <summary>
        /// Encode variable-length 32-bit unsigned integer into this buffer.
        /// </summary>
        public void WriteVariable(uint value)
        {
            // 0x00000000 - 0x0000007f : 0xxxxxxx
            // 0x00000080 - 0x00003fff : 1xxxxxxx 0xxxxxxx
            // 0x00004000 - 0x001fffff : 1xxxxxxx 1xxxxxxx 0xxxxxxx
            // 0x00200000 - 0x0fffffff : 1xxxxxxx 1xxxxxxx 1xxxxxxx 0xxxxxxx
            // 0x10000000 - 0xffffffff : 1xxxxxxx 1xxxxxxx 1xxxxxxx 1xxxxxxx 0000xxxx

            if ((value & 0xffffff80) == 0)
            {
                EnsureCapacityToWrite(1);
                PutByte((byte)value);
                return;
            }
            
            if ((value & 0xffffc000) == 0)
            {
                EnsureCapacityToWrite(2);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) & 0x7f));
                return;
            }
            
            if ((value & 0xffe00000) == 0)
            {
                EnsureCapacityToWrite(3);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)((value >> 14) & 0x7f));
                return;
            }
            
            if ((value & 0xf0000000) == 0)
            {
                EnsureCapacityToWrite(4);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)((value >> 14) | 0x80));
                PutByte((byte)((value >> 21) & 0x7f));
                return;
            }
            
            EnsureCapacityToWrite(5);
            PutByte((byte)(value | 0x80));
            PutByte((byte)((value >> 7) | 0x80));
            PutByte((byte)((value >> 14) | 0x80));
            PutByte((byte)((value >> 21) | 0x80));
            PutByte((byte)((value >> 28) & 0x0f));
        }

        /// <summary>
        /// Encode variable-length 64-bit unsigned integer into this buffer.
        /// </summary>
        public void WriteVariable(ulong value)
        {
            if ((value & 0xffffffffffffff80L) == 0)
            {
                EnsureCapacityToWrite(1);
                PutByte((byte)value);
                return;
            }

            if ((value & 0xffffffffffffc000L) == 0)
            {
                EnsureCapacityToWrite(2);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) & 0x7f));
                return;
            }

            if ((value & 0xffffffffffe00000L) == 0)
            {
                EnsureCapacityToWrite(3);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)((value >> 14) & 0x7f));
                return;
            }

            if ((value & 0xfffffffff0000000L) == 0)
            {
                EnsureCapacityToWrite(4);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)((value >> 14) | 0x80));
                PutByte((byte)((value >> 21) & 0x7f));
                return;
            }

            if ((value & 0xfffffff800000000L) == 0)
            {
                EnsureCapacityToWrite(5);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)((value >> 14) | 0x80));
                PutByte((byte)((value >> 21) | 0x80));
                PutByte((byte)((value >> 28) & 0x7f));
                return;
            }

            if ((value & 0xfffffc0000000000L) == 0)
            {
                EnsureCapacityToWrite(6);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)((value >> 14) | 0x80));
                PutByte((byte)((value >> 21) | 0x80));
                PutByte((byte)((value >> 28) | 0x80));
                PutByte((byte)((value >> 35) & 0x7f));
                return;
            }

            if ((value & 0xfffe000000000000L) == 0)
            {
                EnsureCapacityToWrite(7);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)((value >> 14) | 0x80));
                PutByte((byte)((value >> 21) | 0x80));
                PutByte((byte)((value >> 28) | 0x80));
                PutByte((byte)((value >> 35) | 0x80));
                PutByte((byte)((value >> 42) & 0x7f));
                return;
            }

            if ((value & 0xff00000000000000L) == 0)
            {
                EnsureCapacityToWrite(8);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)((value >> 14) | 0x80));
                PutByte((byte)((value >> 21) | 0x80));
                PutByte((byte)((value >> 28) | 0x80));
                PutByte((byte)((value >> 35) | 0x80));
                PutByte((byte)((value >> 42) | 0x80));
                PutByte((byte)((value >> 49) & 0x7f));
                return;
            }

            if ((value & 0x8000000000000000L) == 0)
            {
                EnsureCapacityToWrite(9);
                PutByte((byte)(value | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)((value >> 14) | 0x80));
                PutByte((byte)((value >> 21) | 0x80));
                PutByte((byte)((value >> 28) | 0x80));
                PutByte((byte)((value >> 35) | 0x80));
                PutByte((byte)((value >> 42) | 0x80));
                PutByte((byte)((value >> 49) | 0x80));
                PutByte((byte)((value >> 56) & 0x7f));
                return;
            }

            EnsureCapacityToWrite(10);
            PutByte((byte)(value | 0x80));
            PutByte((byte)((value >> 7) | 0x80));
            PutByte((byte)((value >> 14) | 0x80));
            PutByte((byte)((value >> 21) | 0x80));
            PutByte((byte)((value >> 28) | 0x80));
            PutByte((byte)((value >> 35) | 0x80));
            PutByte((byte)((value >> 42) | 0x80));
            PutByte((byte)((value >> 49) | 0x80));
            PutByte((byte)((value >> 56) | 0x80));
            PutByte((byte)((value >> 63) & 0x01));
        }

        private void CheckLengthToRead(int numBytes)
        {
            int limit = (marker >= 0 ? marker : back);
            if ((position + numBytes) > limit)
            {
                Log.Warn("front={0} pos={1} back={2} marker={3} numBytes={4}", front, position, back, marker, numBytes);

                throw new IndexOutOfRangeException();
            }
        }

        private void EnsureCapacityToWrite(int numBytes)
        {
            int required = position + numBytes;
            while (required >= Capacity)
            {
                blocks.Add(BufferPool.Acquire(blockSizeExponent));
            }
            if (required > back)
            {
                back = required;
            }
        }

        private byte GetByte()
        {
            BlockFeed();
            return currentBlock[position++ & remainderMask];
        }

        private void PutByte(byte value)
        {
            BlockFeed();
            currentBlock[position++ & remainderMask] = value;
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
                BufferPool.Release(blockSizeExponent, blocks[i]);
            }
            blocks.Clear();
            currentBlock = null;
        }

        // As a Stream mock
        public int ReadByte()
        {
            try
            {
                CheckLengthToRead(1);
            }
            catch
            {
                return -1;
            }
            return (int)GetByte();
        }

        public void WriteByte(byte value)
        {
            EnsureCapacityToWrite(1);
            PutByte(value);
        }
    }
}
