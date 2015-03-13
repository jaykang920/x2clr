// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    /// <summary>
    /// Text string serializer.
    /// </summary>
    public abstract class StringSerializer : VerboseSerializer
    {
        protected StringBuilder stringBuilder;

        protected StringSerializer(StringBuilder stringBuilder)
        {
            this.stringBuilder = stringBuilder;
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

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
        public abstract void Write<T>(string name, List<T> value) where T : Cell;
        public abstract void Write<T>(string name, T value) where T : Cell;

        // Value writers for primitive types
        public void Write(bool value)
        {
            stringBuilder.Append(value);
        }
        public void Write(byte value)
        {
            stringBuilder.Append(value);
        }
        public void Write(sbyte value)
        {
            stringBuilder.Append(value);
        }
        public void Write(short value)
        {
            stringBuilder.Append(value);
        }
        public void Write(int value)
        {
            stringBuilder.Append(value);
        }
        public void Write(long value)
        {
            stringBuilder.Append(value);
        }
        public void Write(float value)
        {
            stringBuilder.Append(value);
        }
        public void Write(double value)
        {
            stringBuilder.Append(value);
        }
        public void Write(string value)
        {
            stringBuilder.Append(value);
        }
        public void Write(DateTime value)
        {
            stringBuilder.Append(value.ToString());
        }

        // Value writers for composite types
        public abstract void Write(byte[] value);
        public abstract void Write(List<int> value);
        public abstract void Write<T>(List<T> value) where T : Cell;
        public abstract void Write<T>(T value) where T : Cell;
    }
}
