using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using Graphing.Editors.Presentation;
using NUnit.Framework;
using UnitRegistry;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

namespace Graphing.Tests
{
    [TestFixture]
    public class GraphLegendOptionsCycleTests
    {
        [Test]
        public void FullCycle_LegendPlacement_BottomDefault_RoundTrips()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(pm1.Legend.Position, Is.EqualTo(LegendPlacement.Bottom));

            // Don't modify

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.LegendPlacement, Is.EqualTo(LegendPlacement.Bottom));

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(pm2.Legend.Position, Is.EqualTo(LegendPlacement.Bottom));
        }

        [Test]
        public void FullCycle_LegendPlacement_ChangeToTop_RoundTrips()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var defaultOptions = new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom);

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(pm1.Legend.Position, Is.EqualTo(LegendPlacement.Bottom));

            // Modify - change to top
            pm1.Legend.Position = LegendPlacement.Top;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.LegendPlacement, Is.EqualTo(LegendPlacement.Top));

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(pm2.Legend.Position, Is.EqualTo(LegendPlacement.Top));
        }

        [Test]
        public void FullCycle_LegendPlacement_ChangeToLeft_RoundTrips()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var defaultOptions = new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom);

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);

            // Modify - change to left
            pm1.Legend.Position = LegendPlacement.Left;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.LegendPlacement, Is.EqualTo(LegendPlacement.Left));

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(pm2.Legend.Position, Is.EqualTo(LegendPlacement.Left));
        }

        [Test]
        public void FullCycle_LegendPlacement_ChangeToRight_RoundTrips()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var defaultOptions = new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom);

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);

            // Modify - change to right
            pm1.Legend.Position = LegendPlacement.Right;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.LegendPlacement, Is.EqualTo(LegendPlacement.Right));

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(pm2.Legend.Position, Is.EqualTo(LegendPlacement.Right));
        }

        [Test]
        public void FullCycle_LegendPlacement_CycleAllPlacements()
        {
            // Test that all placements can be cycled through successfully
            var model = CreateMinimalGraphModel();
            var placements = new[] {
                LegendPlacement.Top,
                LegendPlacement.Bottom,
                LegendPlacement.Left,
                LegendPlacement.Right
            };

            var options = new GraphPresentationOptions();

            foreach (var placement in placements)
            {
                var pm1 = new GraphOptionsPresentationModel(model, options);
                pm1.Legend.Position = placement;
                options = pm1.BuildGraphPresentationOptions();

                var pm2 = new GraphOptionsPresentationModel(model, options);
                Assert.That(pm2.Legend.Position, Is.EqualTo(placement),
                    $"Legend placement {placement} should round-trip correctly");
            }
        }

        [Test]
        public void FullCycle_LegendPlacement_WithTitlesAndAxes()
        {
            // Test legend placement round-trip while other options are also set
            var model = CreateMinimalGraphModel();
            var optionsWithOthers = new GraphPresentationOptions(
                graphTitle: "Test Title",
                hiddenAxisIds: new[] { new AxisId("y-axis") },
                legendPlacement: LegendPlacement.Bottom);

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, optionsWithOthers);
            Assert.That(pm1.Legend.Position, Is.EqualTo(LegendPlacement.Bottom));
            Assert.That(pm1.Titles.TitleText, Is.EqualTo("Test Title"));
            Assert.That(pm1.Axes.Axes[1].IsVisible, Is.False);

            // Modify legend placement only
            pm1.Legend.Position = LegendPlacement.Right;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert all settings persisted
            Assert.That(pm2.Legend.Position, Is.EqualTo(LegendPlacement.Right));
            Assert.That(pm2.Titles.TitleText, Is.EqualTo("Test Title"));
            Assert.That(pm2.Axes.Axes[1].IsVisible, Is.False);
        }

        [Test]
        public void FullCycle_LegendPlacement_PreservedWhenNotModified()
        {
            // Verify legend placement is preserved when not explicitly modified
            var model = CreateMinimalGraphModel();
            var optionsTop = new GraphPresentationOptions(legendPlacement: LegendPlacement.Top);

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, optionsTop);

            // Don't modify legend placement

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(pm2.Legend.Position, Is.EqualTo(LegendPlacement.Top));
        }

        [Test]
        public void FullCycle_MultipleRoundsOfPlacementChanges()
        {
            // Test multiple cycles of placement changes
            var model = CreateMinimalGraphModel();
            var options = new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom);

            // Round 1: Bottom → Top
            var pm1 = new GraphOptionsPresentationModel(model, options);
            pm1.Legend.Position = LegendPlacement.Top;
            options = pm1.BuildGraphPresentationOptions();

            var pm1Check = new GraphOptionsPresentationModel(model, options);
            Assert.That(pm1Check.Legend.Position, Is.EqualTo(LegendPlacement.Top));

            // Round 2: Top → Left
            var pm2 = new GraphOptionsPresentationModel(model, options);
            pm2.Legend.Position = LegendPlacement.Left;
            options = pm2.BuildGraphPresentationOptions();

            var pm2Check = new GraphOptionsPresentationModel(model, options);
            Assert.That(pm2Check.Legend.Position, Is.EqualTo(LegendPlacement.Left));

            // Round 3: Left → Right
            var pm3 = new GraphOptionsPresentationModel(model, options);
            pm3.Legend.Position = LegendPlacement.Right;
            options = pm3.BuildGraphPresentationOptions();

            var pm3Check = new GraphOptionsPresentationModel(model, options);
            Assert.That(pm3Check.Legend.Position, Is.EqualTo(LegendPlacement.Right));

            // Round 4: Right → Bottom
            var pm4 = new GraphOptionsPresentationModel(model, options);
            pm4.Legend.Position = LegendPlacement.Bottom;
            options = pm4.BuildGraphPresentationOptions();

            var pm4Check = new GraphOptionsPresentationModel(model, options);
            Assert.That(pm4Check.Legend.Position, Is.EqualTo(LegendPlacement.Bottom));
        }

        [Test]
        public void FullCycle_LegendPosition_IndependentOfSeriesVisibility()
        {
            // Verify legend placement is independent of series visibility
            var model = CreateMinimalGraphModel();
            var optionsWithSeriesHidden = new GraphPresentationOptions(
                legendPlacement: LegendPlacement.Right,
                hiddenSeriesIds: new[] { new SeriesId("series-1") });

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, optionsWithSeriesHidden);

            // Modify legend placement (series should remain hidden)
            pm1.Legend.Position = LegendPlacement.Top;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(pm2.Legend.Position, Is.EqualTo(LegendPlacement.Top),
                "Legend placement should change");
            Assert.That(pm2.Series.Series[0].IsVisible, Is.False,
                "Series visibility should be preserved independently");
        }

        private static IGraphModel CreateMinimalGraphModel()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(
                new AxisId("x-axis"),
                ModelAxisOrientation.X,
                ModelAxisSide.Bottom,
                unit,
                "m",
                null);
            var yAxis = new AxisModel(
                new AxisId("y-axis"),
                ModelAxisOrientation.Y,
                ModelAxisSide.Left,
                unit,
                "m",
                null);

            var xField = new TestFieldDefinition("X", "x", unit, new double[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new double[] { 0d, 1d });

            var series = new GraphSeriesModel(
                new SeriesId("series-1"),
                "series-1",
                SeriesType.Line,
                xField,
                yField,
                xAxis,
                yAxis);

            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }
    }
}
