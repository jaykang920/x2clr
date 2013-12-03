using System;

using NUnit.Framework;

using x2;

namespace x2.Tests
{
    [TestFixture]
    public class BufferTests
    {
        [Test]
        public void TestFloat32()
        {
            var buffer = new Buffer(1);

            // Boundary value tests

            buffer.Write(0.0F);
            buffer.Write(Single.Epsilon);
            buffer.Write(Single.MinValue);
            buffer.Write(Single.MaxValue);
            buffer.Write(Single.NegativeInfinity);
            buffer.Write(Single.PositiveInfinity);
            buffer.Write(Single.NaN);

            buffer.Rewind();

            float f;

            buffer.Read(out f);
            Assert.AreEqual(0.0F, f);
            buffer.Read(out f);
            Assert.AreEqual(Single.Epsilon, f);
            buffer.Read(out f);
            Assert.AreEqual(Single.MinValue, f);
            buffer.Read(out f);
            Assert.AreEqual(Single.MaxValue, f);
            buffer.Read(out f);
            Assert.AreEqual(Single.NegativeInfinity, f);
            buffer.Read(out f);
            Assert.AreEqual(Single.PositiveInfinity, f);
            buffer.Read(out f);
            Assert.AreEqual(Single.NaN, f);

            buffer.Trim();

            // Intermediate value tests

            buffer.Write(0.001234F);
            buffer.Write(8765.4321F);

            buffer.Rewind();

            buffer.Read(out f);
            Assert.AreEqual(0.001234F, f);
            buffer.Read(out f);
            Assert.AreEqual(8765.4321F, f);
        }

        [Test]
        public void TestFloat64()
        {
            var buffer = new Buffer(2);

            // Boundary value tests

            buffer.Write(0.0D);
            buffer.Write(Double.Epsilon);
            buffer.Write(Double.MinValue);
            buffer.Write(Double.MaxValue);
            buffer.Write(Double.NegativeInfinity);
            buffer.Write(Double.PositiveInfinity);
            buffer.Write(Double.NaN);

            buffer.Rewind();

            double d;

            buffer.Read(out d);
            Assert.AreEqual(0.0D, d);
            buffer.Read(out d);
            Assert.AreEqual(Double.Epsilon, d);
            buffer.Read(out d);
            Assert.AreEqual(Double.MinValue, d);
            buffer.Read(out d);
            Assert.AreEqual(Double.MaxValue, d);
            buffer.Read(out d);
            Assert.AreEqual(Double.NegativeInfinity, d);
            buffer.Read(out d);
            Assert.AreEqual(Double.PositiveInfinity, d);
            buffer.Read(out d);
            Assert.AreEqual(Double.NaN, d);

            buffer.Trim();

            // Intermediate value tests

            buffer.Write(0.00123456789D);
            buffer.Write(98765.0004321D);

            buffer.Rewind();

            buffer.Read(out d);
            Assert.AreEqual(0.00123456789D, d);
            buffer.Read(out d);
            Assert.AreEqual(98765.0004321D, d);
        }

        [Test]
        public void TestVariableLengthInt32()
        {
            var buffer = new Buffer(2);

            // Boundary value tests

            buffer.WriteVariable(0);
            buffer.WriteVariable(-1);
            buffer.WriteVariable(Int32.MaxValue);
            buffer.WriteVariable(Int32.MinValue);

            buffer.Rewind();

            int i, bytes;

            bytes = buffer.ReadVariable(out i);
            Assert.AreEqual(1, bytes);
            Assert.AreEqual(0, i);

            bytes = buffer.ReadVariable(out i);
            Assert.AreEqual(1, bytes);
            Assert.AreEqual(-1, i);

            bytes = buffer.ReadVariable(out i);
            Assert.AreEqual(5, bytes);
            Assert.AreEqual(Int32.MaxValue, i);

            bytes = buffer.ReadVariable(out i);
            Assert.AreEqual(5, bytes);
            Assert.AreEqual(Int32.MinValue, i);

            buffer.Trim();

            // Intermediate value tests

            buffer.WriteVariable(0x00003f80 >> 1);  // 2
            buffer.WriteVariable(0x001fc000 >> 1);  // 3
            buffer.WriteVariable(0x0fe00000 >> 1);  // 4

            buffer.Rewind();

            bytes = buffer.ReadVariable(out i);
            Assert.AreEqual(2, bytes);
            Assert.AreEqual(0x00003f80 >> 1, i);

            bytes = buffer.ReadVariable(out i);
            Assert.AreEqual(3, bytes);
            Assert.AreEqual(0x001fc000 >> 1, i);

            bytes = buffer.ReadVariable(out i);
            Assert.AreEqual(4, bytes);
            Assert.AreEqual(0x0fe00000 >> 1, i);
        }

        [Test]
        public void TestVariableLengthInt64()
        {
            var buffer = new Buffer(2);

            // Boundary value tests

            buffer.WriteVariable(0L);
            buffer.WriteVariable(-1L);
            buffer.WriteVariable(Int64.MaxValue);
            buffer.WriteVariable(Int64.MinValue);

            buffer.Rewind();

            long l, bytes;

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(1, bytes);
            Assert.AreEqual(0L, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(1, bytes);
            Assert.AreEqual(-1L, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(10, bytes);
            Assert.AreEqual(Int64.MaxValue, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(10, bytes);
            Assert.AreEqual(Int64.MinValue, l);

            buffer.Trim();

            // Intermediate value tests

            buffer.WriteVariable(0x00003f80L >> 1);  // 2
            buffer.WriteVariable(0x001fc000L >> 1);  // 3
            buffer.WriteVariable(0x0fe00000L >> 1);  // 4
            buffer.WriteVariable(0x00000007f0000000L >> 1);  // 5
            buffer.WriteVariable(0x000003f800000000L >> 1);  // 6
            buffer.WriteVariable(0x0001fc0000000000L >> 1);  // 7
            buffer.WriteVariable(0x00fe000000000000L >> 1);  // 8
            buffer.WriteVariable(0x7f00000000000000L >> 1);  // 9

            buffer.Rewind();

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(2, bytes);
            Assert.AreEqual(0x00003f80L >> 1, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(3, bytes);
            Assert.AreEqual(0x001fc000L >> 1, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(4, bytes);
            Assert.AreEqual(0x0fe00000L >> 1, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(5, bytes);
            Assert.AreEqual(0x00000007f0000000L >> 1, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(6, bytes);
            Assert.AreEqual(0x000003f800000000L >> 1, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(7, bytes);
            Assert.AreEqual(0x0001fc0000000000L >> 1, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(8, bytes);
            Assert.AreEqual(0x00fe000000000000L >> 1, l);

            bytes = buffer.ReadVariable(out l);
            Assert.AreEqual(9, bytes);
            Assert.AreEqual(0x7f00000000000000L >> 1, l);
        }
    }
}
