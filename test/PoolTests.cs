using System;

using NUnit.Framework;

using x2;

namespace x2.Tests
{
    [TestFixture]
    public class PoolTests
    {
        class Foo
        {
            public int bar;
        }

        [Test]
        public void TestCreation()
        {
            var p0 = new Pool<Foo>();
            Assert.AreEqual(0, p0.Capacity);
            Assert.AreEqual(0, p0.Count);

            var p1 = new Pool<Foo>(1);
            Assert.AreEqual(1, p1.Capacity);
            Assert.AreEqual(0, p1.Count);
        }

        [Test]
        [ExpectedException(ExpectedException=typeof(ArgumentNullException))]
        public void TestNullPush()
        {
            var p = new Pool<Foo>();
            p.Push(null);
        }

        [Test]
        public void TestPushPop()
        {
            var p0 = new Pool<Foo>();
            var p1 = new Pool<Foo>(1);

            var f = new Foo();
            var g = new Foo();

            p0.Push(f);
            Assert.AreEqual(1, p0.Count);
            p0.Push(g);
            Assert.AreEqual(2, p0.Count);

            p1.Push(f);
            Assert.AreEqual(1, p1.Count);
            // capacity overflow
            p1.Push(g);
            Assert.AreEqual(1, p1.Count);

            Foo g1 = p0.Pop();
            Assert.AreSame(g, g1);
            Foo f1 = p0.Pop();
            Assert.AreSame(f, f1);

            Foo f2 = p1.Pop();
            Assert.AreSame(f, f2);
            // pop underflow
            Foo g2 = p1.Pop();
            Assert.AreSame(null, g2);
        }
    }
}
