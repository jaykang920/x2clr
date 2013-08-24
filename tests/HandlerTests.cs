using System;

using NUnit.Framework;

using x2;

namespace x2.Tests
{
    [TestFixture]
    public class HandlerTests
    {
        struct IntBox
        {
            public static int StaticValue { get; set; }
            public int Value { get; set; }

            public static void StaticIncrement(Event e)
            {
                ++StaticValue;
            }

            public static void StaticDecrement(Event e)
            {
                --StaticValue;
            }

            public void Increment(Event e)
            {
                ++Value;
            }

            public void Decrement(Event e)
            {
                --Value;
            }
        }

        private IntBox intBox1;
        private IntBox IntBox2;

        [TestFixtureSetUp]
        public void SetUp()
        {
            IntBox.StaticValue = 0;
            intBox1.Value = 0;
            IntBox2.Value = 0;
        }

        [Test]
        public void TestStaticMethodHandler()
        {
            Event e = Event.New();
            var handler1 = new MethodHandler<Event>(IntBox.StaticIncrement);
            var handler2 = new MethodHandler<Event>(IntBox.StaticIncrement);
            var handler3 = new MethodHandler<Event>(IntBox.StaticDecrement);

            // Properties
            Assert.True(handler1.Method == handler2.Method);
            Assert.False(handler2.Method == handler3.Method);
            Assert.True(handler1.Token == handler2.Token);
            Assert.False(handler2.Token == handler3.Token);

            // Invocation
            handler1.Invoke(e);
            Assert.AreEqual(1, IntBox.StaticValue);
            handler2.Invoke(e);
            Assert.AreEqual(2, IntBox.StaticValue);
            handler3.Invoke(e);
            Assert.AreEqual(1, IntBox.StaticValue);

            // IComparable
            Assert.AreEqual(0, handler1.CompareTo(handler2));
            Assert.AreNotEqual(0, handler2.CompareTo(handler3));
            if (handler2.CompareTo(handler3) > 0)
            {
                Assert.Less(handler3.CompareTo(handler2), 0);
            }
            else
            {
                Assert.Greater(handler3.CompareTo(handler2), 0);
            }

            // Equality
            Assert.True(handler1.Equals(handler2));
            Assert.False(handler2.Equals(handler3));
        }
    }
}
