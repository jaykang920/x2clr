using System;

using NUnit.Framework;

using x2;

namespace x2clr.test
{
    [TestFixture]
    public class ConfigTests
    {
#if XML_CONFIG
        [Test]
        public void TestLoading()
        {
            Config.Load();
        }
#endif
    }
}
