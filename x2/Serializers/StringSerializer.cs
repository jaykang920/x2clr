// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    /// <summary>
    /// Default text string serializer.
    /// </summary>
    public class StringSerializer : VerboseSerializer
    {
        private StringBuilder stringBuilder;

        public StringBuilder StringBuilder { get { return stringBuilder; } }

        public StringSerializer()
        {
            stringBuilder = new StringBuilder();
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        // Name-value pair writers for primitive types
        public override void Write(string name, bool value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, byte value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, sbyte value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, short value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, int value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, long value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, float value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, double value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, string value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, DateTime value)
        {
            WriteName(name);
            Write(value);
        }

        private void WriteName(string name)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(name);
            stringBuilder.Append('=');
        }

        // Name-value pair writers for composite types
        public override void Write(string name, byte[] value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write(string name, List<int> value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write<T>(string name, List<T> value)
        {
            WriteName(name);
            Write(value);
        }
        public override void Write<T>(string name, T value)
        {
            WriteName(name);
            Write(value);
        }

        // Value writers for primitive types
        public override void Write(bool value)
        {
            stringBuilder.Append(value);
        }
        public override void Write(byte value)
        {
            stringBuilder.Append(value);
        }
        public override void Write(sbyte value)
        {
            stringBuilder.Append(value);
        }
        public override void Write(short value)
        {
            stringBuilder.Append(value);
        }
        public override void Write(int value)
        {
            stringBuilder.Append(value);
        }
        public override void Write(long value)
        {
            stringBuilder.Append(value);
        }
        public override void Write(float value)
        {
            stringBuilder.Append(value);
        }
        public override void Write(double value)
        {
            stringBuilder.Append(value);
        }
        public override void Write(string value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('"');
            stringBuilder.Append(value.Replace("\"", "\\\""));
            stringBuilder.Append('"');
        }
        public override void Write(DateTime value)
        {
            stringBuilder.Append(value.ToString());
        }

        // Value writers for composite types
        public override void Write(byte[] value)
        {

        }
        public override void Write(List<int> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public override void Write<T>(List<T> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                stringBuilder.Append(' ');
                Write(value[i]);
            }
            stringBuilder.Append(" ]");
        }
        public override void Write<T>(T value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append(value.GetTypeTag().RuntimeType.Name);
            stringBuilder.Append(" {");
            value.Serialize(this);
            stringBuilder.Append(" }");
        }
    }
}
