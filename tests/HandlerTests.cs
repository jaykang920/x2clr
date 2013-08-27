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
        private IntBox intBox2;

        [TestFixtureSetUp]
        public void SetUp()
        {
            IntBox.StaticValue = 0;
            intBox1.Value = 0;
            intBox2.Value = 0;
        }

        [Test]
        public void TestStaticMethods()
        {
            var handler1 = new Handler<Event>(IntBox.StaticIncrement);
            var handler2 = new Handler<Event>(IntBox.StaticIncrement);
            var handler3 = new Handler<Event>(IntBox.StaticDecrement);

            // Properties
            Assert.True(handler1.Action.Equals(handler2.Action));
            Assert.False(handler2.Action.Equals(handler3.Action));

            // Invocation
            Event e = Event.New();
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

        [Test]
        public void TestInstanceMethods()
        {
            var handler1 = new Handler<Event>(intBox1.Increment);
            var handler2 = new Handler<Event>(intBox1.Increment);
            var handler3 = new Handler<Event>(intBox1.Decrement);
            var handler4 = new Handler<Event>(intBox2.Decrement);

            // Properties
            //Assert.True(handler1.Action.Equals(handler2.Action));
            Assert.False(handler2.Action.Equals(handler3.Action));
            Assert.False(handler3.Action.Equals(handler4.Action));

            // Invocation
            Event e = Event.New();
            handler1.Invoke(e);
            Assert.AreEqual(1, intBox1.Value);
            handler2.Invoke(e);
            Assert.AreEqual(2, intBox1.Value);
            handler3.Invoke(e);
            Assert.AreEqual(1, intBox1.Value);
            handler4.Invoke(e);
            Assert.AreEqual(1, intBox1.Value);
            Assert.AreEqual(-1, intBox2.Value);

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
            Assert.False(handler3.Equals(handler4));
        }
    }
}
