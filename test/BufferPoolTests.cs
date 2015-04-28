using System;

using NUnit.Framework;

using x2;

namespace x2.Tests
{
    [TestFixture]
    public class BufferPoolTests
    {
        [Test]
        public void TestAcquireRelease()
        {
            byte[] b = BufferPool.Acquire(12);
            Assert.AreNotSame(null, b);
            Assert.AreEqual(1 << 12, b.Length);

            BufferPool.Release(12, b);

            byte[] b1 = BufferPool.Acquire(12);
            Assert.AreNotSame(null, b1);
            Assert.AreEqual(1 << 12, b1.Length);

            Assert.AreSame(b, b1);

            BufferPool.Release(12, b1);
        }

        [Test]
        [ExpectedException(ExpectedException=typeof(ArgumentOutOfRangeException))]
        public void TestUnderAcquire()
        {
            BufferPool.Acquire(0);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentOutOfRangeException))]
        public void TestOverAcquire()
        {
            BufferPool.Acquire(32);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentOutOfRangeException))]
        public void TestUnderRelease()
        {
            BufferPool.Release(0, null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentOutOfRangeException))]
        public void TestOverRelease()
        {
            BufferPool.Release(0, null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void TestNullRelease()
        {
            BufferPool.Release(12, null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void TestInvalidArgRelease()
        {
            byte[] b = new byte[1 << 4 - 1];
            BufferPool.Release(4, b);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void TestInvalidRelease()
        {
            byte[] b = new byte[1 << 4];
            BufferPool.Release(4, b);
        }
    }
}
