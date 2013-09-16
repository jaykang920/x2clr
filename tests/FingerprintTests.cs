using System;

using NUnit.Framework;

using x2;

namespace x2.Tests
{
    [TestFixture]
    public class FingerprintTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestNegativeLength()
        {
            Fingerprint fp = new Fingerprint(-1);
        }

        [Test]
        public void TestAccessors()
        {
            Fingerprint fp = new Fingerprint(33);

            Assert.Throws(typeof(IndexOutOfRangeException),
                () => { fp.Get(-1); });
            Assert.Throws(typeof(IndexOutOfRangeException),
                () => { fp.Get(33); });

            Assert.False(fp.Get(31));
            fp.Touch(31);
            Assert.True(fp.Get(31));
            fp.Wipe(31);
            Assert.False(fp.Get(31));

            Assert.False(fp.Get(32));
            fp.Touch(32);
            Assert.True(fp.Get(32));
            fp.Wipe(32);
            Assert.False(fp.Get(32));
        }

        [Test]
        public void TestCreation()
        {
            Fingerprint fp1 = new Fingerprint(1);
            Assert.AreEqual(1, fp1.Length);
            Assert.False(fp1.Get(0));
            
            Fingerprint fp2 = new Fingerprint(33);
            Assert.AreEqual(33, fp2.Length);
            for (int i = 0; i < 33; ++i)
            {
                Assert.False(fp2.Get(i));
            }
        }

        [Test]
        public void TestCopyCreation()
        {
            Fingerprint fp1 = new Fingerprint(65);
            fp1.Touch(32);
            Fingerprint fp2 = new Fingerprint(fp1);
            Assert.True(fp2.Get(32));

            // Ensure that the original block array is not shared
            fp1.Touch(64);
            Assert.False(fp2.Get(64));
        }

        [Test]
        public void TestClear()
        {
            // Length > 32
            Fingerprint fp = new Fingerprint(65);
            for (int i = 0; i < 65; ++i)
            {
                fp.Touch(i);
                Assert.True(fp.Get(i));
            }
            fp.Clear();
            for (int i = 0; i < 65; ++i)
            {
                Assert.False(fp.Get(i));
            }

            // Length <= 32
            var fp2 = new Fingerprint(6);
            for (int i = 0; i < 6; ++i)
            {
                fp2.Touch(i);
                Assert.True(fp2.Get(i));
            }
            fp.Clear();
            for (int i = 0; i < 6; ++i)
            {
                Assert.False(fp.Get(i));
            }
        }

        [Test]
        public void TestComparison()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);
            Fingerprint fp3 = new Fingerprint(64);
            Fingerprint fp4 = new Fingerprint(66);

            // Length first

            Assert.Less(fp3.CompareTo(fp1), 0);
            Assert.Greater(fp1.CompareTo(fp3), 0);
            fp3.Touch(2);
            Assert.Less(fp3.CompareTo(fp1), 0);
            Assert.Greater(fp1.CompareTo(fp3), 0);

            Assert.Greater(fp4.CompareTo(fp2), 0);
            Assert.Less(fp2.CompareTo(fp4), 0);
            fp2.Touch(64);
            Assert.Greater(fp4.CompareTo(fp2), 0);
            Assert.Less(fp2.CompareTo(fp4), 0);
            fp2.Wipe(64);

            // Bits second
            Assert.AreEqual(0, fp1.CompareTo(fp2));

            fp1.Touch(31);
            Assert.Greater(fp1.CompareTo(fp2), 0);
            Assert.Less(fp2.CompareTo(fp1), 0);

            fp2.Touch(32);
            Assert.Less(fp1.CompareTo(fp2), 0);
            Assert.Greater(fp2.CompareTo(fp1), 0);

            fp1.Touch(32);
            Assert.Greater(fp1.CompareTo(fp2), 0);
            Assert.Less(fp2.CompareTo(fp1), 0);

            fp2.Touch(31);
            Assert.AreEqual(0, fp1.CompareTo(fp2));
            Assert.AreEqual(0, fp2.CompareTo(fp1));

            fp2.Touch(64);
            Assert.Less(fp1.CompareTo(fp2), 0);
            Assert.Greater(fp2.CompareTo(fp1), 0);
        }

        [Test]
        public void TestEquality()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);
            Fingerprint fp3 = new Fingerprint(64);
            Fingerprint fp4 = new Fingerprint(66);

            // Reference first
            Assert.True(fp1.Equals(fp1));
            Assert.True(fp2.Equals(fp2));
            Assert.True(fp3.Equals(fp3));
            Assert.True(fp4.Equals(fp4));

            // Type second
            Assert.False(fp1.Equals(new Object()));

            // Length third

            Assert.False(fp3.Equals(fp1));
            Assert.False(fp1.Equals(fp3));

            Assert.False(fp4.Equals(fp2));
            Assert.False(fp2.Equals(fp4));

            // Bits forth

            Assert.True(fp1.Equals(fp2));
            Assert.True(fp2.Equals(fp1));

            fp1.Touch(32);
            Assert.False(fp1.Equals(fp2));
            Assert.False(fp2.Equals(fp1));

            fp2.Touch(32);
            Assert.True(fp1.Equals(fp2));
            Assert.True(fp2.Equals(fp1));

            // Length <= 32
            var fp5 = new Fingerprint(7);
            var fp6 = new Fingerprint(7);
            fp5.Touch(0);
            Assert.False(fp5.Equals(fp6));
            fp6.Touch(0);
            Assert.True(fp5.Equals(fp6));
        }

        [Test]
        public void TestHashing()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);
            Fingerprint fp3 = new Fingerprint(64);
            Fingerprint fp4 = new Fingerprint(66);

            Assert.AreEqual(fp1.GetHashCode(), fp2.GetHashCode());
            Assert.AreNotEqual(fp1.GetHashCode(), fp3.GetHashCode());
            Assert.AreNotEqual(fp2.GetHashCode(), fp4.GetHashCode());

            fp1.Touch(32);
            Assert.AreNotEqual(fp1.GetHashCode(), fp2.GetHashCode());
            fp2.Touch(32);
            Assert.AreEqual(fp1.GetHashCode(), fp2.GetHashCode());
        }

        [Test]
        public void TestEquivalence()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);
            Fingerprint fp3 = new Fingerprint(64);
            Fingerprint fp4 = new Fingerprint(66);

            // Reference first
            Assert.True(fp1.IsEquivalent(fp1));
            Assert.True(fp2.IsEquivalent(fp2));
            Assert.True(fp3.IsEquivalent(fp3));
            Assert.True(fp4.IsEquivalent(fp4));

            // Length second

            Assert.True(fp3.IsEquivalent(fp1));
            Assert.False(fp1.IsEquivalent(fp3));

            Assert.False(fp4.IsEquivalent(fp2));
            Assert.True(fp2.IsEquivalent(fp4));

            // Bits third

            Assert.True(fp1.IsEquivalent(fp2));
            Assert.True(fp2.IsEquivalent(fp1));

            fp1.Touch(32);
            Assert.False(fp1.IsEquivalent(fp2));
            Assert.True(fp2.IsEquivalent(fp1));

            fp2.Touch(32);
            Assert.True(fp1.IsEquivalent(fp2));
            Assert.True(fp2.IsEquivalent(fp1));

            fp2.Touch(31);
            Assert.True(fp1.IsEquivalent(fp2));
            Assert.False(fp2.IsEquivalent(fp1));

            fp4.Touch(31);
            fp4.Touch(32);
            fp4.Touch(33);
            Assert.True(fp2.IsEquivalent(fp4));
            Assert.False(fp4.IsEquivalent(fp2));
        }

        [Test]
        public void TestSerialization()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);

            fp1.Touch(0);
            fp1.Touch(2);
            fp1.Touch(31);
            fp1.Touch(32);

            Assert.False(fp2.Equals(fp1));

            Buffer buffer = new Buffer(12);
            fp1.Dump(buffer);
            buffer.Rewind();
            fp2.Load(buffer);

            Assert.True(fp2.Equals(fp1));
        }
    }
}
