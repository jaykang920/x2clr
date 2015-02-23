using System;
using System.IO;

using NUnit.Framework;

using x2;
//using x2.Serializers;

namespace x2.Tests
{
    /*
    [TestFixture]
    public class SerializerTests
    {
        [Test]
        public void TestFloat32()
        {
            using (var stream = new MemoryStream())
            {
                float f;
                Serializer serializer = new BinarySerializer(stream);

                // Boundary value tests

                serializer.Write(0.0F);
                serializer.Write(Single.Epsilon);
                serializer.Write(Single.MinValue);
                serializer.Write(Single.MaxValue);
                serializer.Write(Single.NegativeInfinity);
                serializer.Write(Single.PositiveInfinity);
                serializer.Write(Single.NaN);

                stream.Seek(0, SeekOrigin.Begin);

                serializer.Read(out f);
                Assert.AreEqual(0.0F, f);
                serializer.Read(out f);
                Assert.AreEqual(Single.Epsilon, f);
                serializer.Read(out f);
                Assert.AreEqual(Single.MinValue, f);
                serializer.Read(out f);
                Assert.AreEqual(Single.MaxValue, f);
                serializer.Read(out f);
                Assert.AreEqual(Single.NegativeInfinity, f);
                serializer.Read(out f);
                Assert.AreEqual(Single.PositiveInfinity, f);
                serializer.Read(out f);
                Assert.AreEqual(Single.NaN, f);

                stream.SetLength(0);

                // Intermediate value tests

                serializer.Write(0.001234F);
                serializer.Write(8765.4321F);

                stream.Seek(0, SeekOrigin.Begin);

                serializer.Read(out f);
                Assert.AreEqual(0.001234F, f);
                serializer.Read(out f);
                Assert.AreEqual(8765.4321F, f);
            }
        }

        [Test]
        public void TestFloat64()
        {
            using (var stream = new MemoryStream())
            {
                double d;
                Serializer serializer = new BinarySerializer(stream);

                // Boundary value tests

                serializer.Write(0.0);
                serializer.Write(Double.Epsilon);
                serializer.Write(Double.MinValue);
                serializer.Write(Double.MaxValue);
                serializer.Write(Double.NegativeInfinity);
                serializer.Write(Double.PositiveInfinity);
                serializer.Write(Double.NaN);

                stream.Seek(0, SeekOrigin.Begin);

                serializer.Read(out d);
                Assert.AreEqual(0.0, d);
                serializer.Read(out d);
                Assert.AreEqual(Double.Epsilon, d);
                serializer.Read(out d);
                Assert.AreEqual(Double.MinValue, d);
                serializer.Read(out d);
                Assert.AreEqual(Double.MaxValue, d);
                serializer.Read(out d);
                Assert.AreEqual(Double.NegativeInfinity, d);
                serializer.Read(out d);
                Assert.AreEqual(Double.PositiveInfinity, d);
                serializer.Read(out d);
                Assert.AreEqual(Double.NaN, d);

                stream.SetLength(0);

                // Intermediate value tests

                serializer.Write(0.001234);
                serializer.Write(8765.4321);

                stream.Seek(0, SeekOrigin.Begin);

                serializer.Read(out d);
                Assert.AreEqual(0.001234, d);
                serializer.Read(out d);
                Assert.AreEqual(8765.4321, d);
            }
        }

        [Test]
        public void TestVariableLengthInt32()
        {
            using (var stream = new MemoryStream())
            {
                int i, bytes;
                Serializer serializer = new BinarySerializer(stream);

                // Boundary value tests

                serializer.Write(0);
                serializer.Write(-1);
                serializer.Write(Int32.MaxValue);
                serializer.Write(Int32.MinValue);

                stream.Seek(0, SeekOrigin.Begin);

                bytes = serializer.Read(out i);
                Assert.AreEqual(1, bytes);
                Assert.AreEqual(0, i);

                bytes = serializer.Read(out i);
                Assert.AreEqual(1, bytes);
                Assert.AreEqual(-1, i);

                bytes = serializer.Read(out i);
                Assert.AreEqual(5, bytes);
                Assert.AreEqual(Int32.MaxValue, i);

                bytes = serializer.Read(out i);
                Assert.AreEqual(5, bytes);
                Assert.AreEqual(Int32.MinValue, i);

                stream.SetLength(0);

                // Intermediate value tests

                serializer.Write(0x00003f80 >> 1);  // 2
                serializer.Write(0x001fc000 >> 1);  // 3
                serializer.Write(0x0fe00000 >> 1);  // 4

                stream.Seek(0, SeekOrigin.Begin);

                bytes = serializer.Read(out i);
                Assert.AreEqual(2, bytes);
                Assert.AreEqual(0x00003f80 >> 1, i);

                bytes = serializer.Read(out i);
                Assert.AreEqual(3, bytes);
                Assert.AreEqual(0x001fc000 >> 1, i);

                bytes = serializer.Read(out i);
                Assert.AreEqual(4, bytes);
                Assert.AreEqual(0x0fe00000 >> 1, i);
            }
        }

        [Test]
        public void TestVariableLengthInt64()
        {
            using (var stream = new MemoryStream())
            {
                long l, bytes;
                Serializer serializer = new BinarySerializer(stream);

                // Boundary value tests

                serializer.Write(0L);
                serializer.Write(-1L);
                serializer.Write(Int64.MaxValue);
                serializer.Write(Int64.MinValue);

                stream.Seek(0, SeekOrigin.Begin);

                bytes = serializer.Read(out l);
                Assert.AreEqual(1, bytes);
                Assert.AreEqual(0L, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(1, bytes);
                Assert.AreEqual(-1L, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(10, bytes);
                Assert.AreEqual(Int64.MaxValue, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(10, bytes);
                Assert.AreEqual(Int64.MinValue, l);

                stream.SetLength(0);

                // Intermediate value tests

                serializer.Write(0x00003f80L >> 1);  // 2
                serializer.Write(0x001fc000L >> 1);  // 3
                serializer.Write(0x0fe00000L >> 1);  // 4
                serializer.Write(0x00000007f0000000L >> 1);  // 5
                serializer.Write(0x000003f800000000L >> 1);  // 6
                serializer.Write(0x0001fc0000000000L >> 1);  // 7
                serializer.Write(0x00fe000000000000L >> 1);  // 8
                serializer.Write(0x7f00000000000000L >> 1);  // 9

                stream.Seek(0, SeekOrigin.Begin);

                bytes = serializer.Read(out l);
                Assert.AreEqual(2, bytes);
                Assert.AreEqual(0x00003f80L >> 1, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(3, bytes);
                Assert.AreEqual(0x001fc000L >> 1, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(4, bytes);
                Assert.AreEqual(0x0fe00000L >> 1, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(5, bytes);
                Assert.AreEqual(0x00000007f0000000L >> 1, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(6, bytes);
                Assert.AreEqual(0x000003f800000000L >> 1, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(7, bytes);
                Assert.AreEqual(0x0001fc0000000000L >> 1, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(8, bytes);
                Assert.AreEqual(0x00fe000000000000L >> 1, l);

                bytes = serializer.Read(out l);
                Assert.AreEqual(9, bytes);
                Assert.AreEqual(0x7f00000000000000L >> 1, l);
            }
        }
    }
    */
}
