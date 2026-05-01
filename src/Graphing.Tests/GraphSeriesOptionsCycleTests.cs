using System.Drawing;
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
    public class GraphSeriesOptionsCycleTests
    {
        [Test]
        public void FullCycle_SeriesVisibility_RoundTrips()
        {
            // Arrange
            var model = CreateGraphModelWithOneSeries();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(pm1.Series.Series.Count, Is.EqualTo(1));
            var seriesItem1 = pm1.Series.Series[0];
            Assert.That(seriesItem1.IsVisible, Is.True);

            // Modify - hide series
            seriesItem1.IsVisible = false;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.HiddenSeriesIds, Contains.Item(new SeriesId("series-1")));

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            var seriesItem2 = pm2.Series.Series[0];
            Assert.That(seriesItem2.IsVisible, Is.False);
        }

        [Test]
        public void FullCycle_MultipleSeries_IndependentVisibility()
        {
            // Arrange
            var model = CreateGraphModelWithMultipleSeries();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(pm1.Series.Series.Count, Is.EqualTo(3));
            var series0 = pm1.Series.Series[0];
            var series1 = pm1.Series.Series[1];
            var series2 = pm1.Series.Series[2];
            Assert.That(series0.IsVisible, Is.True);
            Assert.That(series1.IsVisible, Is.True);
            Assert.That(series2.IsVisible, Is.True);

            // Modify - hide only series1
            series1.IsVisible = false;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            var reloadSeries0 = pm2.Series.Series[0];
            var reloadSeries1 = pm2.Series.Series[1];
            var reloadSeries2 = pm2.Series.Series[2];
            Assert.That(reloadSeries0.IsVisible, Is.True);
            Assert.That(reloadSeries1.IsVisible, Is.False);
            Assert.That(reloadSeries2.IsVisible, Is.True);
        }

        [Test]
        public void FullCycle_UnhideSeries_RoundTrips()
        {
            // Arrange - start with a hidden series
            var model = CreateGraphModelWithOneSeries();
            var optionsWithHiddenSeries = new GraphPresentationOptions(
                hiddenSeriesIds: new[] { new SeriesId("series-1") });

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, optionsWithHiddenSeries);
            var seriesItem = pm1.Series.Series[0];
            Assert.That(seriesItem.IsVisible, Is.False);

            // Modify - show the series
            seriesItem.IsVisible = true;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.HiddenSeriesIds.Count, Is.EqualTo(0),
                "Hidden series list should be empty after showing all series");

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);
            var seriesItem2 = pm2.Series.Series[0];

            // Assert
            Assert.That(seriesItem2.IsVisible, Is.True);
        }

        [Test]
        public void FullCycle_SeriesLabelOverride_RoundTrips()
        {
            // Arrange
            var model = CreateGraphModelWithOneSeries();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            var seriesItem = pm1.Series.Series[0];
            Assert.That(seriesItem.HasLabelOverride, Is.False);

            // Modify - set label override
            seriesItem.HasLabelOverride = true;
            seriesItem.Label = "Custom Series Label";

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);
            var seriesItem2 = pm2.Series.Series[0];

            // Assert - note: the implementation may not currently persist these values
            Assert.That(seriesItem2.HasLabelOverride, Is.True,
                "Label override flag should round-trip (this may fail if not implemented)");
            Assert.That(seriesItem2.Label, Is.EqualTo("Custom Series Label"),
                "Series label override should round-trip (this may fail if not implemented)");
        }

        [Test]
        public void FullCycle_SeriesColorOverride_RoundTrips()
        {
            // Arrange
            var model = CreateGraphModelWithOneSeries();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            var seriesItem = pm1.Series.Series[0];
            Assert.That(seriesItem.HasColorOverride, Is.False);

            // Modify - set color override
            var customColor = Color.Red;
            seriesItem.HasColorOverride = true;
            seriesItem.Color = customColor;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);
            var seriesItem2 = pm2.Series.Series[0];

            // Assert - note: the implementation may not currently persist these values
            Assert.That(seriesItem2.HasColorOverride, Is.True,
                "Color override flag should round-trip (this may fail if not implemented)");
            Assert.That(seriesItem2.Color, Is.EqualTo(customColor),
                "Series color override should round-trip (this may fail if not implemented)");
        }

        [Test]
        public void FullCycle_AllSeriesHidden_ThenOneShown()
        {
            // Arrange
            var model = CreateGraphModelWithMultipleSeries();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);

            // Modify - hide all series
            foreach (var series in pm1.Series.Series)
            {
                series.IsVisible = false;
            }

            // Apply
            var optionsAllHidden = pm1.BuildGraphPresentationOptions();
            Assert.That(optionsAllHidden.HiddenSeriesIds.Count, Is.EqualTo(3));

            // Reload and modify - show one series
            var pm2 = new GraphOptionsPresentationModel(model, optionsAllHidden);
            pm2.Series.Series[0].IsVisible = true;

            // Apply again
            var optionsOneShown = pm2.BuildGraphPresentationOptions();
            Assert.That(optionsOneShown.HiddenSeriesIds.Count, Is.EqualTo(2));

            // Reload and verify
            var pm3 = new GraphOptionsPresentationModel(model, optionsOneShown);
            Assert.That(pm3.Series.Series[0].IsVisible, Is.True);
            Assert.That(pm3.Series.Series[1].IsVisible, Is.False);
            Assert.That(pm3.Series.Series[2].IsVisible, Is.False);
        }

        [Test]
        public void FullCycle_SeriesVisibilityPreserved_WhenNotModified()
        {
            // Arrange - start with a hidden series
            var model = CreateGraphModelWithOneSeries();
            var optionsWithHiddenSeries = new GraphPresentationOptions(
                hiddenSeriesIds: new[] { new SeriesId("series-1") });

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, optionsWithHiddenSeries);
            var seriesItem = pm1.Series.Series[0];
            Assert.That(seriesItem.IsVisible, Is.False);

            // Don't modify anything

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);
            var seriesItem2 = pm2.Series.Series[0];

            // Assert
            Assert.That(seriesItem2.IsVisible, Is.False,
                "Series visibility should be preserved when not modified");
        }

        [Test]
        public void FullCycle_SeriesCount_RemainsConsistent()
        {
            // Verify that the number of series in the editor model matches the graph model
            var model = CreateGraphModelWithMultipleSeries();
            var defaultOptions = new GraphPresentationOptions();

            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(pm1.Series.Series.Count, Is.EqualTo(3));

            var appliedOptions = pm1.BuildGraphPresentationOptions();
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            Assert.That(pm2.Series.Series.Count, Is.EqualTo(3),
                "Series count should match graph model series");
        }

        [Test]
        public void FullCycle_SeriesOrder_PreservedAcrossReload()
        {
            // Verify series order is preserved
            var model = CreateGraphModelWithMultipleSeries();
            var defaultOptions = new GraphPresentationOptions();

            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            var originalIds = new[] { pm1.Series.Series[0].SeriesId, pm1.Series.Series[1].SeriesId, pm1.Series.Series[2].SeriesId };

            var appliedOptions = pm1.BuildGraphPresentationOptions();
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            Assert.That(pm2.Series.Series[0].SeriesId, Is.EqualTo(originalIds[0]));
            Assert.That(pm2.Series.Series[1].SeriesId, Is.EqualTo(originalIds[1]));
            Assert.That(pm2.Series.Series[2].SeriesId, Is.EqualTo(originalIds[2]));
        }

        [Test]
        public void FullCycle_MixedHiddenAndVisibleSeries_RoundTrips()
        {
            // Arrange
            var model = CreateGraphModelWithMultipleSeries();
            var optionsWithMixed = new GraphPresentationOptions(
                hiddenSeriesIds: new[] { new SeriesId("series-2") });

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, optionsWithMixed);

            // Modify - change visibility pattern
            pm1.Series.Series[0].IsVisible = false;
            pm1.Series.Series[1].IsVisible = true;
            pm1.Series.Series[2].IsVisible = false;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.HiddenSeriesIds.Count, Is.EqualTo(2));

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(pm2.Series.Series[0].IsVisible, Is.False);
            Assert.That(pm2.Series.Series[1].IsVisible, Is.True);
            Assert.That(pm2.Series.Series[2].IsVisible, Is.False);
        }

        private static IGraphModel CreateGraphModelWithOneSeries()
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

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });

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

        private static IGraphModel CreateGraphModelWithMultipleSeries()
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
            var yField1 = new TestFieldDefinition("Y1", "y1", unit, new double[] { 0d, 1d });
            var yField2 = new TestFieldDefinition("Y2", "y2", unit, new double[] { 2d, 3d });
            var yField3 = new TestFieldDefinition("Y3", "y3", unit, new double[] { 4d, 5d });

            var series1 = new GraphSeriesModel(
                new SeriesId("series-1"),
                "series-1",
                SeriesType.Line,
                xField,
                yField1,
                xAxis,
                yAxis);

            var series2 = new GraphSeriesModel(
                new SeriesId("series-2"),
                "series-2",
                SeriesType.Line,
                xField,
                yField2,
                xAxis,
                yAxis);

            var series3 = new GraphSeriesModel(
                new SeriesId("series-3"),
                "series-3",
                SeriesType.Line,
                xField,
                yField3,
                xAxis,
                yAxis);

            return new GraphModel(new[] { xAxis, yAxis }, new[] { series1, series2, series3 });
        }
    }
}
