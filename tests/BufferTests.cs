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
