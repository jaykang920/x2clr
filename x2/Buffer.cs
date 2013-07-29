// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    /// <summary>
    /// A variable-length byte buffer class that provides methods to read/write 
    /// primitive data types as byte sequences in a platform-independent way.
    /// </summary>
    /// <remarks>
    /// All the members of this class are not thread-safe.
    /// </remarks>
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

        public int Length
        {
            get { return (back - front); }
        }

        public int Position
        {
            get { return position; }
            set
            {
                if (value < front || back < value)
                {
                    throw new IndexOutOfRangeException();
                }
                position = value;
                int blockIndex = position >> blockSizeExponent;
                if (blockIndex >= blocks.Count)
                {
                    blockIndex = blocks.Count - 1;
                }
                if (blockIndex != currentBlockIndex)
                {
                    currentBlockIndex = blockIndex;
                    currentBlock = blocks[currentBlockIndex];
                }
            }
        }

        private int BlockSize
        {
            get { return (1 << blockSizeExponent); }
        }

        private int Capacity
        {
            get { return (BlockSize * blocks.Count); }
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

            blocks.Add(AcquireBlock(BlockSize));

            currentBlockIndex = 0;
            currentBlock = blocks[currentBlockIndex];
            position = 0;
            back = 0;
            front = 0;

            marker = -1;
        }

        ~Buffer() { }

        public static int Write(byte[] buffer, int value)
        {
            if (buffer.Length < 4)
            {
                throw new ArgumentException();
            }
            buffer[0] = (byte)(value >> 24);
            buffer[1] = (byte)(value >> 16);
            buffer[2] = (byte)(value >> 8);
            buffer[3] = (byte)value;
            return 4;
        }

        public static int WriteUInt29(byte[] buffer, int value)
        {
            if ((value & 0xFFFFFF80) == 0)
            {
                if (buffer.Length < 1)
                {
                    throw new ArgumentException();
                }
                buffer[0] = (byte)value;
                return 1;
            }
            else if ((value & 0xFFFFC000) == 0)
            {
                if (buffer.Length < 2)
                {
                    throw new ArgumentException();
                }
                buffer[0] = (byte)((value >> 7) | 0x80);
                buffer[1] = (byte)(value & 0x7F);
                return 2;
            }
            else if ((value & 0xFFE00000) == 0)
            {
                if (buffer.Length < 3)
                {
                    throw new ArgumentException();
                }
                buffer[0] = (byte)((value >> 14) | 0x80);
                buffer[1] = (byte)((value >> 7) | 0x80);
                buffer[2] = (byte)(value & 0x7F);
                return 3;
            }
            else if ((value & 0xC0000000) == 0)
            {
                if (buffer.Length < 4)
                {
                    throw new ArgumentException();
                }
                buffer[0] = (byte)((value >> 22) | 0x80);
                buffer[1] = (byte)((value >> 15) | 0x80);
                buffer[2] = (byte)((value >> 8) | 0x80);
                buffer[3] = (byte)value;
                return 4;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public void ListOccupiedSegments(IList<ArraySegment<byte>> blockList)
        {
            int frontIndex = front >> blockSizeExponent;
            int frontOffset = front & remainderMask;
            int backIndex = back >> blockSizeExponent;
            int backOffset = back & remainderMask;
            if (frontIndex == backIndex)
            {
                blockList.Add(new ArraySegment<byte>(blocks[frontIndex], frontOffset,
                                                     backOffset - frontOffset));
                return;
            }
            blockList.Add(new ArraySegment<byte>(blocks[frontIndex], frontOffset,
                                                 BlockSize - frontOffset));
            for (int i = frontIndex + 1; i < backIndex; ++i)
            {
                blockList.Add(new ArraySegment<byte>(blocks[i]));
            }
            if (backOffset != 0)
            {
                blockList.Add(new ArraySegment<byte>(blocks[backIndex], 0, backOffset));
            }
        }

        public void ListAvailableSegments(IList<ArraySegment<byte>> blockList)
        {
            if ((Capacity - back) < BlockSize)
            {
                blocks.Add(AcquireBlock(BlockSize));
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
            value = (ReadByte() != 0);
        }

        public void Read(out sbyte value)
        {
            value = (sbyte)ReadByte();
        }

        public void Read(out byte value)
        {
            value = ReadByte();
        }

        public void Read(out byte[] value)
        {
            int length;
            ReadUInt29(out length);
            CheckLengthToRead(length);
            value = new byte[length];
            int blockIndex = position >> blockSizeExponent;
            int srcOffset = position & remainderMask;
            int bytesToCopy, bytesCopied = 0;
            while (bytesCopied < length)
            {
                bytesToCopy = Math.Min(BlockSize - srcOffset, length - bytesCopied);
                System.Buffer.BlockCopy(blocks[blockIndex++], srcOffset,
                  value, bytesCopied, bytesToCopy);
                srcOffset = 0;
                bytesCopied += bytesToCopy;
            }
            Position = position + length;
        }

        public void Read(out short value)
        {
            CheckLengthToRead(2);
            value = GetByte();
            value = (short)((value << 8) | GetByte());
        }

        public void Read(out int value)
        {
            CheckLengthToRead(4);
            value = GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
            value = (value << 8) | GetByte();
        }

        public void Read(out long value)
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

        public void Read(out string value)
        {
            int length;
            ReadUInt29(out length);
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
                            throw new System.IO.InvalidDataException();
                        }
                        c2 = (char)GetByte();
                        if ((c2 & 0xC0) != 0x80)
                        {
                            throw new System.IO.InvalidDataException();
                        }
                        stringBuilder.Append((char)(((c & 0x1F) << 6) | (c2 & 0x3F)));
                        break;
                    case 14:
                        // 1110 xxxx  10xx xxxx  10xx xxxx
                        bytesRead += 3;
                        if (bytesRead > length)
                        {
                            throw new System.IO.InvalidDataException();
                        }
                        c2 = (char)GetByte();
                        c3 = (char)GetByte();
                        if (((c2 & 0xC0) != 0x80) || ((c3 & 0xC0) != 0x80))
                        {
                            throw new System.IO.InvalidDataException();
                        }
                        stringBuilder.Append((char)(((c & 0x0F) << 12) |
                          ((c2 & 0x3F) << 6) | ((c3 & 0x3F) << 0)));
                        break;
                    default:
                        // 10xx xxxx  1111 xxxx
                        throw new System.IO.InvalidDataException();
                }
            }
            value = stringBuilder.ToString();
        }

        public byte ReadByte()
        {
            CheckLengthToRead(1);
            return GetByte();
        }

        public int ReadUInt29(out int value)
        {
            CheckLengthToRead(1);
            int i;
            byte b = GetByte();
            if ((b & 0x80) == 0)
            {
                value = b;
                return 1;
            }
            CheckLengthToRead(1);
            i = (b & 0x7F) << 7;
            b = GetByte();
            if ((b & 0x80) == 0)
            {
                value = (i | b);
                return 2;
            }
            CheckLengthToRead(1);
            i = (i | (b & 0x7F)) << 7;
            b = GetByte();
            if ((b & 0x80) == 0)
            {
                value = (i | b);
                return 3;
            }
            CheckLengthToRead(1);
            i = (i | (b & 0x7F)) << 8;
            value = (i | GetByte());
            return 4;
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

        public byte this[int index]
        {
            get
            {
                index += front;
                if (index < 0 || back <= index)
                {
                    throw new IndexOutOfRangeException();
                }
                return blocks[index >> blockSizeExponent][index & remainderMask];
            }
            set
            {
                index += front;
                if (index < 0 || back <= index)
                {
                    throw new IndexOutOfRangeException();
                }
                blocks[index >> blockSizeExponent][index & remainderMask] = value;
            }
        }

        public void Trim()
        {
            int index, count;
            if (marker >= 0 && position < marker)
            {
                position = marker;
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
                foreach (byte[] block in blocksToRemove)
                {
                    ReleaseBlock(block);
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
            WriteUInt29(count);
            EnsureCapacityToWrite(count);
            int blockIndex = position >> blockSizeExponent;
            int dstOffset = position & remainderMask;
            int bytesToCopy, bytesCopied = 0;
            while (bytesCopied < count)
            {
                bytesToCopy = Math.Min(BlockSize - dstOffset, count - bytesCopied);
                System.Buffer.BlockCopy(value, offset + bytesCopied,
                  blocks[blockIndex++], dstOffset, bytesToCopy);
                dstOffset = 0;
                bytesCopied += bytesToCopy;
            }
            Position = position + count;
        }

        public void Write(short value)
        {
            EnsureCapacityToWrite(2);
            PutByte((byte)(value >> 8));
            PutByte((byte)value);
        }

        public void Write(int value)
        {
            EnsureCapacityToWrite(4);
            PutByte((byte)(value >> 24));
            PutByte((byte)(value >> 16));
            PutByte((byte)(value >> 8));
            PutByte((byte)value);
        }

        public void Write(long value)
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

        public void Write(string value)
        {
            int length = 0;
            foreach (char c in value)
            {
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
            WriteUInt29(length);
            EnsureCapacityToWrite(length);
            foreach (char c in value)
            {
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

        public void WriteUInt29(int value)
        {
            // 0x00000000 - 0x0000007F : 0xxxxxxx
            // 0x00000080 - 0x00003FFF : 1xxxxxxx 0xxxxxxx
            // 0x00004000 - 0x001FFFFF : 1xxxxxxx 1xxxxxxx 0xxxxxxx
            // 0x00200000 - 0x3FFFFFFF : 1xxxxxxx 1xxxxxxx 1xxxxxxx xxxxxxxx
            // 0x40000000 - 0xFFFFFFFF : throw range exception

            if ((value & 0xFFFFFF80) == 0)
            {
                EnsureCapacityToWrite(1);
                PutByte((byte)value);
            }
            else if ((value & 0xFFFFC000) == 0)
            {
                EnsureCapacityToWrite(2);
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)(value & 0x7F));
            }
            else if ((value & 0xFFE00000) == 0)
            {
                EnsureCapacityToWrite(3);
                PutByte((byte)((value >> 14) | 0x80));
                PutByte((byte)((value >> 7) | 0x80));
                PutByte((byte)(value & 0x7F));
            }
            else if ((value & 0xC0000000) == 0)
            {
                EnsureCapacityToWrite(4);
                PutByte((byte)((value >> 22) | 0x80));
                PutByte((byte)((value >> 15) | 0x80));
                PutByte((byte)((value >> 8) | 0x80));
                PutByte((byte)value);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private static byte[] AcquireBlock(int blockSize)
        {
            // No pooling at this point
            return new byte[blockSize];
        }

        private static void ReleaseBlock(byte[] block)
        {
            // No pooling at this point
        }

        private void CheckLengthToRead(int numBytes)
        {
            int limit = (marker >= 0 ? marker : back);
            if ((position + numBytes) > limit)
            {
                throw new IndexOutOfRangeException();
            }
        }

        private void EnsureCapacityToWrite(int numBytes)
        {
            int required = position + numBytes;
            while (required > Capacity)
            {
                blocks.Add(AcquireBlock(BlockSize));
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
    }
}
