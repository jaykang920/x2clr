using System;

using NUnit.Framework;

using x2;
using x2.Transforms;

namespace x2.Tests
{
    [TestFixture]
    public class CipherTests
    {
        [Test]
        public void TestTransform()
        {
            Cipher cipher1 = new Cipher();
            Cipher cipher2 = new Cipher();

            byte[] bytes1 = cipher1.InitializeHandshake();
            byte[] bytes2 = cipher2.InitializeHandshake();
            bytes1 = cipher2.Handshake(bytes1);
            bytes2 = cipher1.Handshake(bytes2);
            cipher1.FinalizeHandshake(bytes1);
            cipher2.FinalizeHandshake(bytes2);

            Buffer buffer = new Buffer(12);

            string text = new String('x', 5300);
            Assert.AreEqual(5300, text.Length);
            buffer.Write(1);
            buffer.Write(text);
            buffer.Shrink(1);

            cipher1.Transform(buffer, (int)buffer.Length);
            cipher2.InverseTransform(buffer, (int)buffer.Length);

            buffer.Rewind();

            string result;
            buffer.Read(out result);

            Assert.AreEqual(text, result);
        }
    }
}
