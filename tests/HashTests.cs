using System;

using NUnit.Framework;

using x2;

namespace x2.Tests
{
    [TestFixture]
    public class HashTests
    {
        [Test]
        public void TestCreation()
        {
            // One-arg constructor With new
            Hash hash1 = new Hash(Hash.Seed);
            Assert.NotNull(hash1);
            Assert.AreEqual(hash1.Code, Hash.Seed);

            // Without new
            Hash hash2;
            hash2.Code = Hash.Seed;
            Assert.NotNull(hash2);
        }

        [Test]
        public void TestBool()
        {
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update(true);
            hash2.Update(true);
            hash3.Update(false);
            Assert.AreEqual(hash1.Code, hash2.Code);
            Assert.AreNotEqual(hash1.Code, hash3.Code);
        }

        [Test]
        public void TestInt()
        {
            // Signed
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update(2);
            hash2.Update(2);
            hash3.Update(-2);
            Assert.AreEqual(hash1.Code, hash2.Code);
            Assert.AreNotEqual(hash1.Code, hash3.Code);

            // Unsigned
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            Hash hash6 = new Hash(Hash.Seed);
            hash4.Update((uint)2);
            hash5.Update((uint)2);
            hash6.Update(unchecked((uint)-2));
            Assert.AreEqual(hash4.Code, hash5.Code);
            Assert.AreNotEqual(hash4.Code, hash6.Code);

            Assert.AreEqual(hash1.Code, hash4.Code);
            Assert.AreEqual(hash3.Code, hash6.Code);
        }

        [Test]
        public void TestLong()
        {
            // Signed
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update((long)2);
            hash2.Update((long)2);
            hash3.Update((long)-2);
            Assert.AreEqual(hash1.Code, hash2.Code);
            Assert.AreNotEqual(hash1.Code, hash3.Code);

            // Unsigned
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            Hash hash6 = new Hash(Hash.Seed);
            hash4.Update((ulong)2);
            hash5.Update((ulong)2);
            hash6.Update(unchecked((ulong)-2));
            Assert.AreEqual(hash4.Code, hash5.Code);
            Assert.AreNotEqual(hash4.Code, hash6.Code);

            Assert.AreEqual(hash1.Code, hash4.Code);
            Assert.AreEqual(hash3.Code, hash6.Code);
        }

        [Test]
        public void TestFloat()
        {
            // Single-precision
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update(0.01f);
            hash2.Update(0.01f);
            hash3.Update(-0.01f);
            Assert.AreEqual(hash1.Code, hash2.Code);
            Assert.AreNotEqual(hash1.Code, hash3.Code);

            // Double-precision
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            Hash hash6 = new Hash(Hash.Seed);
            hash4.Update((double)0.01f);
            hash5.Update((double)0.01f);
            hash6.Update((double)-0.01f);
            Assert.AreEqual(hash4.Code, hash5.Code);
            Assert.AreNotEqual(hash4.Code, hash6.Code);

            Assert.AreEqual(hash1.Code, hash4.Code);
            Assert.AreEqual(hash3.Code, hash6.Code);
        }

        [Test]
        public void TestString()
        {
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update("abcd");
            hash2.Update("abcd");
            hash3.Update("bcde");
            Assert.AreEqual(hash1.Code, hash2.Code);
            Assert.AreNotEqual(hash1.Code, hash3.Code);

            // Null reference handling
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            hash4.Update(0);
            hash5.Update((string)null);
            Assert.AreEqual(hash4.Code, hash5.Code);
        }

        [Test]
        public void TestObject()
        {
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            Object obj1 = new Object();
            Object obj2 = obj1;
            int hashCode = obj1.GetHashCode();
            hash1.Update(hashCode);
            hash2.Update(obj1);
            hash3.Update(obj2);
            Assert.AreEqual(hash1.Code, hash2.Code);
            Assert.AreEqual(hash1.Code, hash3.Code);

            // Null reference handling
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            hash4.Update(0);
            hash5.Update((object)null);
            Assert.AreEqual(hash4.Code, hash5.Code);
        }
    }
}
