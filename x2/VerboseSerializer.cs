// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2
{
    /// <summary>
    /// Defines methods to write name-value pairs into the backing object.
    /// </summary>
    public abstract class VerboseSerializer
    {
        // Name-value pair writers for primitive types
        public abstract void Write(string name, bool value);
        public abstract void Write(string name, byte value);
        public abstract void Write(string name, sbyte value);
        public abstract void Write(string name, short value);
        public abstract void Write(string name, int value);
        public abstract void Write(string name, long value);
        public abstract void Write(string name, float value);
        public abstract void Write(string name, double value);
        public abstract void Write(string name, string value);
        public abstract void Write(string name, DateTime value);

        // Name-value pair writers for composite types
        public abstract void Write(string name, byte[] value);
        public abstract void Write(string name, List<int> value);
        public abstract void Write(string name, List<List<int>> value);
        public abstract void Write<T>(string name, List<T> value) where T : Cell;
        public abstract void Write<T>(string name, T value) where T : Cell;

        // Value writers for primitive types
        public abstract void Write(bool value);
        public abstract void Write(byte value);
        public abstract void Write(sbyte value);
        public abstract void Write(short value);
        public abstract void Write(int value);
        public abstract void Write(long value);
        public abstract void Write(float value);
        public abstract void Write(double value);
        public abstract void Write(string value);
        public abstract void Write(DateTime value);

        // Value writers for composite types
        public abstract void Write(byte[] value);
        public abstract void Write(List<int> value);
        public abstract void Write(List<List<int>> value);
        public abstract void Write<T>(List<T> value) where T : Cell;
        public abstract void Write<T>(T value) where T : Cell;
    }
}
