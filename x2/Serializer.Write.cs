﻿// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace x2
{
    // Serializer.Write
    public sealed partial class Serializer
    {
        // Overloaded Write for primitive types

        /// <summary>
        /// Encodes a boolean value into the underlying stream.
        /// </summary>
        public void Write(bool value)
        {
            buffer.EnsureCapacityToWrite(1);
            buffer.PutByte((byte)(value ? 1 : 0));
        }

        /// <summary>
        /// Encodes a single byte into the underlying stream.
        /// </summary>
        public void Write(byte value)
        {
            buffer.EnsureCapacityToWrite(1);
            buffer.PutByte(value);
        }

        /// <summary>
        /// Encodes an 8-bit signed integer into the underlying stream.
        /// </summary>
        public void Write(sbyte value)
        {
            buffer.EnsureCapacityToWrite(1);
            buffer.PutByte((byte)value);
        }

        /// <summary>
        /// Encodes a 16-bit signed integer into the underlying stream.
        /// </summary>
        public void Write(short value)
        {
            buffer.EnsureCapacityToWrite(2);
            buffer.PutByte((byte)(value >> 8));
            buffer.PutByte((byte)value);
        }

        /// <summary>
        /// Encodes a 32-bit signed integer into the underlying stream.
        /// </summary>
        public void Write(int value)
        {
            // Zigzag encoding
            WriteVariable((uint)((value << 1) ^ (value >> 31)));
        }

        /// <summary>
        /// Encodes a 64-bit signed integer into the underlying stream.
        /// </summary>
        public void Write(long value)
        {
            // Zigzag encoding
            WriteVariable((ulong)((value << 1) ^ (value >> 63)));
        }

        /// <summary>
        /// Encodes a 32-bit floating-point number into the underlying stream.
        /// </summary>
        public void Write(float value)
        {
            WriteFixedBigEndian(
                BitConverter.ToInt32(System.BitConverter.GetBytes(value), 0));
        }

        /// <summary>
        /// Encodes a 64-bit floating-point number into the underlying stream.
        /// </summary>
        public void Write(double value)
        {
            WriteFixedBigEndian(
                BitConverter.ToInt64(System.BitConverter.GetBytes(value), 0));
        }

        /// <summary>
        /// Encodes a text string into the underlying stream.
        /// </summary>
        public void Write(string value)
        {
            // UTF-8 encoding
            int length = GetEncodedLengthUTF8(value);
            WriteVariableNonnegative(length);
            if (length == 0)
            {
                return;
            }
            buffer.EnsureCapacityToWrite(length);
            for (int i = 0, count = value.Length; i < count; ++i)
            {
                var c = value[i];

                if ((c & 0xff80) == 0)
                {
                    buffer.PutByte((byte)c);
                }
                else if ((c & 0xf800) != 0)
                {
                    buffer.PutByte((byte)(0xe0 | ((c >> 12) & 0x0f)));
                    buffer.PutByte((byte)(0x80 | ((c >> 6) & 0x3f)));
                    buffer.PutByte((byte)(0x80 | ((c >> 0) & 0x3f)));
                }
                else
                {
                    buffer.PutByte((byte)(0xc0 | ((c >> 6) & 0x1f)));
                    buffer.PutByte((byte)(0x80 | ((c >> 0) & 0x3f)));
                }
            }
        }

        /// <summary>
        /// Encodes a datetime value into the underlying stream.
        /// </summary>
        public void Write(DateTime value)
        {
            long usecs = (value.Ticks - 621355968000000000) / 10;
            WriteFixedBigEndian(usecs);
        }

        // Overloaded Write for composite types

        /// <summary>
        /// Encodes an array of bytes into the underlying buffer.
        /// </summary>
        public void Write(byte[] value)
        {
            int length = Object.ReferenceEquals(value, null) ? 0 : value.Length;
            WriteVariableNonnegative(length);
            buffer.Write(value, 0, length);
        }

        /// <summary>
        /// Encodes an ordered list of Int32 values into the underlying stream.
        /// </summary>
        public void Write(List<int> value)
        {
            int count = Object.ReferenceEquals(value, null) ? 0 : value.Count;
            WriteVariableNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                Write(value[i]);
            }
        }

        /// <summary>
        /// Encodes an ordered list of Int64 values into the underlying stream.
        /// </summary>
        public void Write(List<long> value)
        {
            int count = Object.ReferenceEquals(value, null) ? 0 : value.Count;
            WriteVariableNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                Write(value[i]);
            }
        }

        /// <summary>
        /// Encodes an ordered list of 32-bit floating-point values into the
        /// underlying stream.
        /// </summary>
        public void Write(List<float> value)
        {
            int count = Object.ReferenceEquals(value, null) ? 0 : value.Count;
            WriteVariableNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                Write(value[i]);
            }
        }

        /// <summary>
        /// Encodes an ordered list of DateTime values into the underlying stream.
        /// </summary>
        public void Write(List<DateTime> value)
        {
            int count = Object.ReferenceEquals(value, null) ? 0 : value.Count;
            WriteVariableNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                Write(value[i]);
            }
        }

        /// <summary>
        /// Encodes an ordered list of Int32 lists into the underlying stream.
        /// </summary>
        public void Write(List<List<int>> value)
        {
            int count = Object.ReferenceEquals(value, null) ? 0 : value.Count;
            WriteVariableNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                Write(value[i]);
            }
        }

        /// <summary>
        /// Encodes an ordered list of Cell-derived objects into the underlying
        /// stream.
        /// </summary>
        public void Write<T>(List<T> value) where T : Cell
        {
            int count = Object.ReferenceEquals(value, null) ? 0 : value.Count;
            WriteVariableNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                Write(value[i]);
            }
        }

        /// <summary>
        /// Encodes a Cell-derived objects into the underlying stream.
        /// </summary>
        public void Write<T>(T value) where T : Cell
        {
            bool isNull = Object.ReferenceEquals(value, null);
            int length = isNull ? 0 : value.GetEncodedLength();
            WriteVariableNonnegative(length);
            if (!isNull)
            {
                value.Serialize(this);
            }
        }

        // Wrtie helper methods

        /// <summary>
        /// Encodes a 32-bit signed integer into the underlying stream,
        /// by fixed-width big-endian byte order.
        /// </summary>
        private void WriteFixedBigEndian(int value)
        {
            buffer.EnsureCapacityToWrite(4);
            buffer.PutByte((byte)(value >> 24));
            buffer.PutByte((byte)(value >> 16));
            buffer.PutByte((byte)(value >> 8));
            buffer.PutByte((byte)value);
        }

        /// <summary>
        /// Encodes a 64-bit signed integer into the underlying stream,
        /// by fixed-width big-endian byte order.
        /// </summary>
        private void WriteFixedBigEndian(long value)
        {
            buffer.EnsureCapacityToWrite(8);
            buffer.PutByte((byte)(value >> 56));
            buffer.PutByte((byte)(value >> 48));
            buffer.PutByte((byte)(value >> 40));
            buffer.PutByte((byte)(value >> 32));
            buffer.PutByte((byte)(value >> 24));
            buffer.PutByte((byte)(value >> 16));
            buffer.PutByte((byte)(value >> 8));
            buffer.PutByte((byte)value);
        }

        /// <summary>
        /// Encodes a 32-bit signed integer into the underlying stream,
        /// by fixed-width little-endian byte order.
        /// </summary>
        private void WriteFixedLittleEndian(int value)
        {
            buffer.EnsureCapacityToWrite(4);
            buffer.PutByte((byte)value);
            buffer.PutByte((byte)(value >> 8));
            buffer.PutByte((byte)(value >> 16));
            buffer.PutByte((byte)(value >> 24));
        }

        /// <summary>
        /// Encodes a 64-bit signed integer into the underlying stream,
        /// by fixed-width little-endian byte order.
        /// </summary>
        private void WriteFixedLittleEndian(long value)
        {
            buffer.EnsureCapacityToWrite(8);
            buffer.PutByte((byte)value);
            buffer.PutByte((byte)(value >> 8));
            buffer.PutByte((byte)(value >> 16));
            buffer.PutByte((byte)(value >> 24));
            buffer.PutByte((byte)(value >> 32));
            buffer.PutByte((byte)(value >> 40));
            buffer.PutByte((byte)(value >> 48));
            buffer.PutByte((byte)(value >> 56));
        }

        /// <summary>
        /// Encodes a 32-bit unsigned integer into the underlying stream,
        /// with unsigned LEB128 encoding.
        /// </summary>
        public void WriteVariable(uint value)
        {
            do
            {
                buffer.EnsureCapacityToWrite(1);
                byte b = (byte)(value & 0x7f);
                value >>= 7;
                if (value != 0)
                {
                    b |= 0x80;
                }
                buffer.PutByte(b);
            } while (value != 0);
        }

        public static int WriteVariable(byte[] buffer, uint value)
        {
            int i = 0;
            do
            {
                byte b = (byte)(value & 0x7f);
                value >>= 7;
                if (value != 0)
                {
                    b |= 0x80;
                }
                buffer[i++] = b;
            } while (value != 0);
            return i;
        }

        /// <summary>
        /// Encodes a 64-bit unsigned integer into the underlying stream,
        /// with unsigned LEB128 encoding.
        /// </summary>
        public void WriteVariable(ulong value)
        {
            do
            {
                buffer.EnsureCapacityToWrite(1);
                byte b = (byte)(value & 0x7f);
                value >>= 7;
                if (value != 0)
                {
                    b |= 0x80;
                }
                buffer.PutByte(b);
            } while (value != 0);
        }

        /// <summary>
        /// Encodes a 32-bit non-negative integer into the underlying stream.
        /// </summary>
        public void WriteVariableNonnegative(int value)
        {
            if (value < 0) { throw new ArgumentOutOfRangeException(); }
            WriteVariable((uint)value);
        }
    }
}
