using System;

using NUnit.Framework;

using x2;

namespace x2.Tests
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void TestLoading()
        {
            Config.Load();
        }
    }
}
