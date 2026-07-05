using System;
using System.Collections.Generic;
using Graphing.Core.Pie.Models;
using Graphing.Core.Pie.Presentation;
using Graphing.Core.Pie.Snapshot;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Core.Tests
{
    [TestFixture]
    public class PieHitTestHelperTests
    {
        [Test]
        public void HitTest_ReturnsNullForNullPresentation()
        {
            var result = PieHitTestHelper.HitTest(0.5, 0.5, null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void HitTest_ReturnsNullOutsideRadius()
        {
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("test"),
                "Test",
                10.0,
                "10",
                1.0,
                0,
                360,
                PieColor.Empty);

            var presentation = new PieGraphPresentationModel(
                "Test",
                new PiePoint(0.5, 0.5),
                0.2,  // Small radius
                new[] { slice },
                null,
                null);

            // Position far outside
            var result = PieHitTestHelper.HitTest(0.95, 0.95, presentation);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void HitTest_ReturnsNullAtCenter()
        {
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("test"),
                "Test",
                10.0,
                "10",
                1.0,
                0,
                360,
                PieColor.Empty);

            var presentation = new PieGraphPresentationModel(
                "Test",
                new PiePoint(0.5, 0.5),
                0.2,
                new[] { slice },
                null,
                null);

            // Exact center
            var result = PieHitTestHelper.HitTest(0.5, 0.5, presentation);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void HitTest_HitsFullCircleSlice()
        {
            var sliceId = new PieSliceId("test-full");
            var slice = new PieSlicePresentationGeometry(
                sliceId,
                "Full",
                10.0,
                "10",
                1.0,
                0,
                360,
                PieColor.Empty);

            var center = new PiePoint(0.5, 0.5);
            var radius = 0.2;
            var slices = new[] { slice };
            
            var presentation = new PieGraphPresentationModel(
                "Test",
                center,
                radius,
                slices,
                null,
                null);

            // Position at 45 degrees, well within radius
            var x = 0.5 + 0.15 * Math.Cos(45 * Math.PI / 180);
            var y = 0.5 + 0.15 * Math.Sin(45 * Math.PI / 180);

            var result = PieHitTestHelper.HitTest(x, y, presentation);

            Assert.That(result, Is.Not.Null);
            // The hit test successfully found a slice - the infrastructure is working
        }

        [Test]
        public void HitTest_MissesSliceOutsideAngularRange()
        {
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("test"),
                "Test",
                5.0,
                "5",
                0.5,
                0,
                180,  // Only upper half
                PieColor.Empty);

            var presentation = new PieGraphPresentationModel(
                "Test",
                new PiePoint(0.5, 0.5),
                0.2,
                new[] { slice },
                null,
                null);

            // Position at 270 degrees (below center), outside slice
            var x = 0.5;  // center X
            var y = 0.5 - 0.15;  // below center

            var result = PieHitTestHelper.HitTest(x, y, presentation);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void HitTest_ReturnsNullForZeroSweepSlice()
        {
            var slice = new PieSlicePresentationGeometry(
                new PieSliceId("zero-sweep"),
                "Zero",
                0.0,
                "0",
                0.0,
                0,
                0,  // Zero sweep
                PieColor.Empty);

            var presentation = new PieGraphPresentationModel(
                "Test",
                new PiePoint(0.5, 0.5),
                0.2,
                new[] { slice },
                null,
                null);

            var result = PieHitTestHelper.HitTest(0.65, 0.5, presentation);

            Assert.That(result, Is.Null);
        }
    }
}
