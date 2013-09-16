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
            var cell1 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell2 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell3 = new SampleCell1 { Foo = 1, Bar = "foo" };
            var cell4 = new SampleCell2 { Foo = 1, Bar = "bar" };
            var cell5 = new SampleCell3 { Foo = 1, Bar = "bar" };
            var cell6 = new SampleCell4 { Foo = 1, Bar = "bar" };
            var cell7 = new SampleCell4 { Foo = 1, Bar = "bar" };

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

        [Test]
        public void TestHashing()
        {
            var cell1 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell2 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell3 = new SampleCell1 { Foo = 1, Bar = "foo" };
            var cell4 = new SampleCell2 { Foo = 1, Bar = "bar" };
            var cell5 = new SampleCell3 { Foo = 1, Bar = "bar" };
            var cell6 = new SampleCell4 { Foo = 1, Bar = "bar" };
            var cell7 = new SampleCell4 { Foo = 1, Bar = "bar" };

            Assert.AreEqual(cell1.GetHashCode(), cell2.GetHashCode());
            Assert.AreNotEqual(cell1.GetHashCode(), cell3.GetHashCode());

            Assert.AreEqual(cell1.GetHashCode(), cell4.GetHashCode());
            Assert.AreEqual(cell1.GetHashCode(), cell5.GetHashCode());
            Assert.AreEqual(cell1.GetHashCode(), cell6.GetHashCode());
            Assert.AreEqual(cell1.GetHashCode(), cell7.GetHashCode());

            Assert.AreEqual(cell6.GetHashCode(), cell7.GetHashCode());

            cell6.Quux = true;
            Assert.AreNotEqual(cell6.GetHashCode(), cell7.GetHashCode());
            cell7.Quux = true;
            Assert.AreEqual(cell6.GetHashCode(), cell7.GetHashCode());
        }

        [Test]
        public void TestEquivalence()
        {
            var cell1 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell2 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell3 = new SampleCell1 { Foo = 1, Bar = "foo" };
            var cell4 = new SampleCell2 { Foo = 1, Bar = "bar" };
            var cell5 = new SampleCell3 { Foo = 1, Bar = "bar" };
            var cell6 = new SampleCell4 { Foo = 1, Bar = "bar" };
            var cell7 = new SampleCell4 { Foo = 1, Bar = "bar" };

            // Reference first
            Assert.True(cell1.IsEquivalent(cell1));
            Assert.True(cell2.IsEquivalent(cell2));
            Assert.True(cell3.IsEquivalent(cell3));
            Assert.True(cell4.IsEquivalent(cell4));

            // Fingerprint second

            Assert.True(cell1.IsEquivalent(cell4));
            Assert.False(cell4.IsEquivalent(cell1));

            Assert.True(cell6.IsEquivalent(cell7));
            Assert.True(cell7.IsEquivalent(cell6));
            cell7.Quux = true;
            Assert.True(cell6.IsEquivalent(cell7));
            Assert.False(cell7.IsEquivalent(cell6));

            // Contents third

            cell4.Bar = "foo";
            Assert.False(cell1.IsEquivalent(cell4));
            Assert.False(cell4.IsEquivalent(cell1));

            cell6.Quux = false;
            Assert.False(cell6.IsEquivalent(cell7));
            Assert.False(cell7.IsEquivalent(cell6));
            cell6.Quux = true;
            Assert.True(cell6.IsEquivalent(cell7));
            Assert.True(cell7.IsEquivalent(cell6));
        }
    }
}
