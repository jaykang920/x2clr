using System;

using NUnit.Framework;

using x2;

namespace x2clr.test
{
    [TestFixture]
    public class CapoTests
    {
        [Test]
        public void TestCreation()
        {
            var fp = new Fingerprint(7);
            
            var window = new Capo<bool>(fp, 0);
            Assert.AreEqual(7, window.Length);

            window = new Capo<bool>(fp, 3);
            Assert.AreEqual(4, window.Length);

            // Invalid offset initialization attempt never throws
            Assert.DoesNotThrow(() => { window = new Capo<bool>(fp, 7); });
        }

        [Test]
        public void TestCapoing()
        {
            var fp = new Fingerprint(8);
            fp.Touch(2);
            fp.Touch(4);

            // Capo just displaces Fingerprint index with a provided offset. 

            var window = new Capo<bool>(fp, 3);
            Assert.False(window[0]);
            Assert.True(window[1]);
            Assert.False(window[2]);

            fp.Wipe(4);
            Assert.False(window[1]);

            // Out-of-range indexing never throws
            Assert.False(window[-8]);
            Assert.False(window[8]);
        }
    }
}
