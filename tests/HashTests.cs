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
            Hash hash = new Hash(17);
            Assert.NotNull(hash);
        }
    }
}
