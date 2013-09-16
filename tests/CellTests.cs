using System;

using NUnit.Framework;

using x2;

namespace x2.Tests
{
    [TestFixture]
    public class CellTests
    {
        [Test]
        public void TestCreation()
        {
            Cell cell1 = new SampleCell1();
            Cell cell2 = new SampleCell2();
            Cell cell3 = new SampleCell3();
            Cell cell4 = new SampleCell4();

            // Static construction

            var tag = ((Cell)cell1).GetTypeTag();
            Assert.IsNull(tag.Base);
            Assert.AreEqual(cell1.GetType(), tag.RuntimeType);
            Assert.AreEqual(2, tag.NumProps);
            Assert.AreEqual(0, tag.Offset);

            tag = ((Cell)cell2).GetTypeTag();
            Assert.AreEqual(cell1.GetTypeTag(), tag.Base);
            Assert.AreEqual(cell2.GetType(), tag.RuntimeType);
            Assert.AreEqual(1, tag.NumProps);
            Assert.AreEqual(2, tag.Offset);

            tag = ((Cell)cell3).GetTypeTag();
            Assert.AreEqual(cell1.GetTypeTag(), tag.Base);
            Assert.AreEqual(cell3.GetType(), tag.RuntimeType);
            Assert.AreEqual(1, tag.NumProps);
            Assert.AreEqual(2, tag.Offset);

            tag = ((Cell)cell4).GetTypeTag();
            Assert.AreEqual(cell2.GetTypeTag(), tag.Base);
            Assert.AreEqual(cell4.GetType(), tag.RuntimeType);
            Assert.AreEqual(1, tag.NumProps);
            Assert.AreEqual(3, tag.Offset);

            // Fingerprint length
            Assert.AreEqual(2, cell1.GetFingerprint().Length);
            Assert.AreEqual(3, cell2.GetFingerprint().Length);
            Assert.AreEqual(3, cell3.GetFingerprint().Length);
            Assert.AreEqual(4, cell4.GetFingerprint().Length);
        }

        [Test]
        public void TestEquality()
        {
            var cell1 = new SampleCell1 {
                Foo = 1, Bar = "bar"
            };
            var cell2 = new SampleCell1 {
                Foo = 1, Bar = "bar"
            };
            var cell3 = new SampleCell1 {
                Foo = 1, Bar = "foo"
            };
            var cell4 = new SampleCell2 {
                Foo = 1, Bar = "bar"
            };
            var cell5 = new SampleCell3 {
                Foo = 1, Bar = "bar"
            };
            var cell6 = new SampleCell4 {
                Foo = 1, Bar = "bar"
            };
            var cell7 = new SampleCell4() {
                Foo = 1, Bar = "bar"
            };

            Assert.True(cell1.Equals(cell2));
            Assert.True(cell2.Equals(cell1));
            Assert.False(cell1.Equals(cell3));
            Assert.False(cell3.Equals(cell1));

            Assert.False(cell1.Equals(cell4));
            Assert.False(cell1.Equals(cell5));
            Assert.False(cell1.Equals(cell6));
            Assert.False(cell1.Equals(cell7));

            Assert.True(cell6.Equals(cell7));
        }
    }
}
