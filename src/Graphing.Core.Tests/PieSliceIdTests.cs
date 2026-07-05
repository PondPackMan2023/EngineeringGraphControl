using System;
using Graphing.Core.Pie.Presentation;
using NUnit.Framework;

namespace Graphing.Core.Tests
{
    [TestFixture]
    public class PieSliceIdTests
    {
        [Test]
        public void PieSliceId_StoresValue()
        {
            var id = new PieSliceId("slice-1");

            Assert.That(id.Value, Is.EqualTo("slice-1"));
        }

        [Test]
        public void PieSliceId_ThrowsForNullValue()
        {
            Assert.That(
                () => new PieSliceId(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void PieSliceId_ThrowsForEmptyValue()
        {
            Assert.That(
                () => new PieSliceId(string.Empty),
                Throws.ArgumentException);
        }

        [Test]
        public void PieSliceId_ThrowsForWhitespaceOnlyValue()
        {
            Assert.That(
                () => new PieSliceId("   "),
                Throws.ArgumentException);
        }

        [Test]
        public void PieSliceId_EqualsUsingValue()
        {
            var id1 = new PieSliceId("same");
            var id2 = new PieSliceId("same");

            Assert.That(id1.Equals(id2), Is.True);
        }

        [Test]
        public void PieSliceId_NotEqualsUsingDifferentValues()
        {
            var id1 = new PieSliceId("first");
            var id2 = new PieSliceId("second");

            Assert.That(id1.Equals(id2), Is.False);
        }

        [Test]
        public void PieSliceId_EqualityOperator()
        {
            var id1 = new PieSliceId("same");
            var id2 = new PieSliceId("same");

            Assert.That(id1 == id2, Is.True);
        }

        [Test]
        public void PieSliceId_InequalityOperator()
        {
            var id1 = new PieSliceId("first");
            var id2 = new PieSliceId("second");

            Assert.That(id1 != id2, Is.True);
        }

        [Test]
        public void PieSliceId_SameValuesHaveSameHashCode()
        {
            var id1 = new PieSliceId("same");
            var id2 = new PieSliceId("same");

            Assert.That(id1.GetHashCode(), Is.EqualTo(id2.GetHashCode()));
        }

        [Test]
        public void PieSliceId_DifferentValuesHaveDifferentHashCodes()
        {
            var id1 = new PieSliceId("first");
            var id2 = new PieSliceId("second");

            // Note: Hash code collision is possible but extremely unlikely for different short strings
            Assert.That(id1.GetHashCode(), Is.Not.EqualTo(id2.GetHashCode()));
        }

        [Test]
        public void PieSliceId_ToStringReturnsValue()
        {
            var id = new PieSliceId("test-id");

            Assert.That(id.ToString(), Is.EqualTo("test-id"));
        }
    }
}
