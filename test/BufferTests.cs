using System;

using NUnit.Framework;

using x2;

namespace x2clr.test
{
    [TestFixture]
    public class BufferTests
    {
        [Test]
        public void TestBufferCreation()
        {
            var buf = new x2.Buffer();

            Assert.IsTrue(IsPowerOfTwo(buf.BlockSize));
            Assert.IsTrue(buf.IsEmpty);
            Assert.IsTrue(buf.Length == 0);
            Assert.IsTrue(buf.Capacity > 0);
            Assert.IsTrue(buf.Position == 0);
        }

        private static bool IsPowerOfTwo(int x)
        {
            return x >= 1 && ((x & (~x + 1)) == x);
        }

        [Test]
        public void TestBufferBasicPerformance()
        {
            // CopyTo 
            
            //  
        }
    }
}
