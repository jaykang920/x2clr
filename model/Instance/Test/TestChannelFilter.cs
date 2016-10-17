using System;
using NUnit.Framework;
using Events.Tests;

namespace Test
{
    [TestFixture]
    public class TestChannelFilter
    {
        [Test]
        public void TestFiltering()
        {
            var filter = new Server.Core.ChannelFilter();

            filter.Add(1, "Instance");

            var e2 = new SampleEvent2();

            filter.Process(e2);

            Assert.IsTrue(e2._Channel == "Instance");

            filter.Add(2, "Sample");

            filter.Process(e2);

            Assert.IsTrue(e2._Channel == "Sample");
        }
    }
}
