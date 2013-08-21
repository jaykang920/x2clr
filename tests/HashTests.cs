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
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update(2);
            hash2.Update(2);
            hash3.Update(-2);
            Assert.AreEqual(hash1.Code, hash2.Code);
            Assert.AreNotEqual(hash1.Code, hash3.Code);
        }

        [Test]
        public void TestUInt()
        {
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update((uint)4);
            hash2.Update((uint)4);
            hash3.Update((uint)8);
            Assert.AreEqual(hash1.Code, hash2.Code);
            Assert.AreNotEqual(hash1.Code, hash3.Code);
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
        }

        [Test]
        public void TestObject()
        {
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Object obj = new Object();
            int hashCode = obj.GetHashCode();
            hash1.Update(hashCode);
            hash2.Update(obj);
            Assert.AreEqual(hash1.Code, hash2.Code);
        }
    }
}
