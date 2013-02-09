// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2 {
  public class Hash {
    public const int Seed = 17;

    private int code;

    public int Code {
      get { return code; }
    }

    public Hash(int seed) {
      code = seed;
    }

    public static int Update(int seed, bool value) {
      return ((seed << 5) + seed) ^ (value ? 2 : 1);
    }

    public static int Update(int seed, int value) {
      return ((seed << 5) + seed) ^ value;
    }

    public static int Update(int seed, uint value) {
      return ((seed << 5) + seed) ^ (int)value;
    }

    public static int Update(int seed, long value) {
      return ((seed << 5) + seed) ^ (int)(value ^ (value >> 32));
    }

    public static int Update(int seed, ulong value) {
      return ((seed << 5) + seed) ^ (int)(value ^ (value >> 32));
    }

    public static int Update(int seed, float value) {
      return Update(seed, (double)value);
    }

    public static int Update(int seed, double value) {
      long bits = System.BitConverter.DoubleToInt64Bits(value);
      return ((seed << 5) + seed) ^ (int)((ulong)bits >> 20);
    }

    public static int Update(int seed, string value) {
      return ((seed << 5) + seed) ^ (value != null ? value.GetHashCode() : 0);
    }

    public void Update(bool value) {
      code = Update(code, value);
    }

    public void Update(int value) {
      code = Update(code, value);
    }

    public void Update(uint value) {
      code = Update(code, value);
    }

    public void Update(long value) {
      code = Update(code, value);
    }

    public void Update(ulong value) {
      code = Update(code, value);
    }

    public void Update(float value) {
      code = Update(code, value);
    }

    public void Update(double value) {
      code = Update(code, value);
    }

    public void Update(string value) {
      code = Update(code, value);
    }

    public void Update<T>(T obj) {
      code = obj.GetHashCode();
    }
  }
}
