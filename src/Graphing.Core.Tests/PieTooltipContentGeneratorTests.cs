using Graphing.Core.Pie.Presentation;
using NUnit.Framework;

namespace Graphing.Core.Tests
{
    [TestFixture]
    public class PieTooltipContentGeneratorTests
    {
        [Test]
        public void GenerateTooltip_ReturnsEmptyStringForNullSlice()
        {
            var result = PieTooltipContentGenerator.GenerateTooltip(null);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GenerateTooltip_FormatsSliceContent()
        {
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("test-slice"),
                "Housing",
                2145.00,
                "$2,145.00",
                0.243,
                0,
                87.48,
                PieColor.Empty);

            var result = PieTooltipContentGenerator.GenerateTooltip(slice);

            Assert.That(result, Does.Contain("Housing"));
            Assert.That(result, Does.Contain("$2,145.00"));
            Assert.That(result, Does.Contain("24.3%"));
        }

        [Test]
        public void GenerateTooltip_ContainsNewlines()
        {
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("test-slice"),
                "Label",
                100.0,
                "100",
                0.5,
                0,
                180,
                PieColor.Empty);

            var result = PieTooltipContentGenerator.GenerateTooltip(slice);

            var lines = result.Split('\n');
            Assert.That(lines, Has.Length.EqualTo(3));
            Assert.That(lines[0], Is.EqualTo("Label"));
            Assert.That(lines[1], Is.EqualTo("100"));
            Assert.That(lines[2], Is.EqualTo("50.0%"));
        }

        [Test]
        public void GenerateTooltip_FormatsPercentageWithOneDecimal()
        {
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("test-slice"),
                "Test",
                10.0,
                "10",
                0.333333,
                0,
                120,
                PieColor.Empty);

            var result = PieTooltipContentGenerator.GenerateTooltip(slice);

            // 0.333333 * 100 = 33.3333 -> formatted to "33.3"
            Assert.That(result, Does.Contain("33.3%"));
        }

        [Test]
        public void GenerateTooltip_HandlesSmallPercentages()
        {
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("test-slice"),
                "Tiny",
                0.5,
                "0.5",
                0.001,
                0,
                3.6,
                PieColor.Empty);

            var result = PieTooltipContentGenerator.GenerateTooltip(slice);

            // 0.001 * 100 = 0.1 -> "0.1%"
            Assert.That(result, Does.Contain("0.1%"));
        }

        [Test]
        public void GenerateTooltip_HandlesFullCircle()
        {
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("test-slice"),
                "Total",
                100.0,
                "100",
                1.0,
                0,
                360,
                PieColor.Empty);

            var result = PieTooltipContentGenerator.GenerateTooltip(slice);

            Assert.That(result, Does.Contain("100.0%"));
        }

        [Test]
        public void GenerateTooltip_PreservesLabelAndFormattedValue()
        {
            var label = "Production Cost";
            var formattedValue = "$1,234.56";
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("test-slice"),
                label,
                1234.56,
                formattedValue,
                0.5,
                0,
                180,
                PieColor.Empty);

            var result = PieTooltipContentGenerator.GenerateTooltip(slice);

            Assert.That(result, Does.Contain(label));
            Assert.That(result, Does.Contain(formattedValue));
        }
    }
}
