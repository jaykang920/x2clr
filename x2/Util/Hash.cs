// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2
{
    /// <summary>
    /// Internal utility struct for Hash code generation.
    /// </summary>
    /// Note that this struct is mutable.
    public struct Hash
    {
        public const int Seed = 17;

        public int Code;

        public Hash(int seed)
        {
            Code = seed;
        }

        public static int Update(int seed, bool value)
        {
            return ((seed << 5) + seed) ^ (value ? 2 : 1);
        }

        public static int Update(int seed, sbyte value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, byte value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, short value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, ushort value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, int value)
        {
            return ((seed << 5) + seed) ^ value;
        }

        public static int Update(int seed, uint value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, long value)
        {
            return ((seed << 5) + seed) ^ (int)(value ^ (value >> 32));
        }

        public static int Update(int seed, ulong value)
        {
            return ((seed << 5) + seed) ^ (int)(value ^ (value >> 32));
        }

        public static int Update(int seed, float value)
        {
            return Update(seed, System.BitConverter.DoubleToInt64Bits((double)value));
        }

        public static int Update(int seed, double value)
        {
            return Update(seed, System.BitConverter.DoubleToInt64Bits(value));
        }

        public static int Update(int seed, string value)
        {
            return ((seed << 5) + seed) ^ (value != null ? value.GetHashCode() : 0);
        }

        public static int Update(int seed, DateTime value)
        {
            return Update(seed, value.Ticks);
        }

        public static int Update<T>(int seed, T value) where T : class
        {
            return ((seed << 5) + seed) ^ (value != null ? value.GetHashCode() : 0);
        }

        public void Update(bool value)
        {
            Code = Update(Code, value);
        }

        public void Update(sbyte value)
        {
            Code = Update(Code, value);
        }

        public void Update(byte value)
        {
            Code = Update(Code, value);
        }

        public void Update(short value)
        {
            Code = Update(Code, value);
        }

        public void Update(ushort value)
        {
            Code = Update(Code, value);
        }

        public void Update(int value)
        {
            Code = Update(Code, value);
        }

        public void Update(uint value)
        {
            Code = Update(Code, value);
        }

        public void Update(long value)
        {
            Code = Update(Code, value);
        }

        public void Update(ulong value)
        {
            Code = Update(Code, value);
        }

        public void Update(float value)
        {
            Code = Update(Code, value);
        }

        public void Update(double value)
        {
            Code = Update(Code, value);
        }

        public void Update(string value)
        {
            Code = Update(Code, value);
        }

        public void Update(DateTime value)
        {
            Code = Update(Code, value);
        }

        public void Update<T>(T value) where T : class
        {
            Code = Update(Code, value);
        }
    }
}
