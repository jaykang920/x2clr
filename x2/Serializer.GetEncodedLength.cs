// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace x2
{
    // Serializer.GetEncodedLength
    public sealed partial class Serializer
    {
        // Overloaded GetEncodedLength for primitive types

        /// <summary>
        /// Gets the number of bytes required to encode the specified boolean
        /// value.
        /// </summary>
        public static int GetEncodedLength(bool value) { return 1; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified single
        /// byte.
        /// </summary>
        public static int GetEncodedLength(byte value) { return 1; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 8-bit
        /// signed integer.
        /// </summary>
        public static int GetEncodedLength(sbyte value) { return 1; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 16-bit
        /// signed integer.
        /// </summary>
        public static int GetEncodedLength(short value) { return 2; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 32-bit
        /// signed integer.
        /// </summary>
        public static int GetEncodedLength(int value)
        {
            return GetEncodedLengthVariable((uint)((value << 1) ^ (value >> 31)));
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 64-bit
        /// signed integer.
        /// </summary>
        public static int GetEncodedLength(long value)
        {
            return GetEncodedLengthVariable((ulong)((value << 1) ^ (value >> 63)));
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 32-bit
        /// floating-point number.
        /// </summary>
        public static int GetEncodedLength(float value) { return 4; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 64-bit
        /// floating-point number.
        /// </summary>
        public static int GetEncodedLength(double value) { return 8; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified text
        /// string.
        /// </summary>
        public static int GetEncodedLength(string value)
        {
            int length = GetEncodedLengthUTF8(value);
            return GetEncodedLengthVariableNonnegative(length) + length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified text
        /// string with UTF-8 encoding.
        /// </summary>
        private static int GetEncodedLengthUTF8(string value)
        {
            int length = 0;
            if (!Object.ReferenceEquals(value, null))
            {
                for (int i = 0, count = value.Length; i < count; ++i)
                {
                    char c = value[i];

                    if ((c & 0xff80) == 0) { ++length; }
                    else if ((c & 0xf800) != 0) { length += 3; }
                    else { length += 2; }
                }
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified datetime
        /// value.
        /// </summary>
        public static int GetEncodedLength(DateTime value) { return 8; }

        // Overloaded GetEncodedLength for composite types

        /// <summary>
        /// Gets the number of bytes required to encode the specified array of
        /// bytes.
        /// </summary>
        public static int GetEncodedLength(byte[] value)
        {
            int length = Object.ReferenceEquals(value, null) ? 0 : value.Length;
            return GetEncodedLengthVariableNonnegative(length) + length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of Int32 values.
        /// </summary>
        public static int GetEncodedLength(List<int> value)
        {
            int count = Object.ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetEncodedLengthVariableNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetEncodedLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of Cell-derived objects.
        /// </summary>
        public static int GetEncodedLength<T>(List<T> value) where T : Cell
        {
            int count = Object.ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetEncodedLengthVariableNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetEncodedLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified
        /// Cell-derived object.
        /// </summary>
        public static int GetEncodedLength<T>(T value) where T : Cell
        {
            int length = Object.ReferenceEquals(value, null) ?
                0 : value.GetEncodedLength();
            return GetEncodedLengthVariableNonnegative(length) + length;
        }

        // GetEncodedLength helper methods

        /// <summary>
        /// Gets the number of bytes required to encode the specified 32-bit
        /// unsigned integer with unsigned LEB128 encoding.
        /// </summary>
        private static int GetEncodedLengthVariable(uint value)
        {
            if ((value & 0xffffff80) == 0) { return 1; }
            if ((value & 0xffffc000) == 0) { return 2; }
            if ((value & 0xffe00000) == 0) { return 3; }
            if ((value & 0xf0000000) == 0) { return 4; }
            return 5;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 64-bit
        /// unsigned integer with unsigned LEB128 encoding.
        /// </summary>
        private static int GetEncodedLengthVariable(ulong value)
        {
            if ((value & 0xffffffffffffff80L) == 0) { return 1; }
            if ((value & 0xffffffffffffc000L) == 0) { return 2; }
            if ((value & 0xffffffffffe00000L) == 0) { return 3; }
            if ((value & 0xfffffffff0000000L) == 0) { return 4; }
            if ((value & 0xfffffff800000000L) == 0) { return 5; }
            if ((value & 0xfffffc0000000000L) == 0) { return 6; }
            if ((value & 0xfffe000000000000L) == 0) { return 7; }
            if ((value & 0xff00000000000000L) == 0) { return 8; }
            if ((value & 0x8000000000000000L) == 0) { return 9; }
            return 10;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 32-bit
        /// non-negative integer.
        /// </summary>
        public static int GetEncodedLengthVariableNonnegative(int value)
        {
            if (value < 0) { throw new ArgumentOutOfRangeException(); }
            return GetEncodedLengthVariable((uint)value);
        }
    }
}
