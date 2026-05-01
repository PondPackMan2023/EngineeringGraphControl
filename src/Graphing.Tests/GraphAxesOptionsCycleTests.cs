using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using Graphing.Editors.EditorModels;
using Graphing.Editors.Presentation;
using NUnit.Framework;
using UnitRegistry;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

namespace Graphing.Tests
{
    [TestFixture]
    public class GraphAxesOptionsCycleTests
    {
        [Test]
        public void FullCycle_AxisVisibility_RoundTrips()
        {
            // Arrange
            var model = CreateGraphModelWithTwoAxes();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(pm1.Axes.Axes.Count, Is.EqualTo(2));
            var yAxisItem1 = pm1.Axes.Axes[1];
            Assert.That(yAxisItem1.IsVisible, Is.True);

            // Modify - hide y-axis
            yAxisItem1.IsVisible = false;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.HiddenAxisIds, Contains.Item(new AxisId("y-axis")));

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            var yAxisItem2 = pm2.Axes.Axes[1];
            Assert.That(yAxisItem2.IsVisible, Is.False);
        }

        [Test]
        public void FullCycle_MultipleAxes_IndependentVisibility()
        {
            // Arrange
            var model = CreateGraphModelWithMultipleAxes();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(pm1.Axes.Axes.Count, Is.EqualTo(3));
            var axis0 = pm1.Axes.Axes[0];
            var axis1 = pm1.Axes.Axes[1];
            var axis2 = pm1.Axes.Axes[2];
            Assert.That(axis0.IsVisible, Is.True);
            Assert.That(axis1.IsVisible, Is.True);
            Assert.That(axis2.IsVisible, Is.True);

            // Modify - hide only axis1
            axis1.IsVisible = false;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            var reloadAxis0 = pm2.Axes.Axes[0];
            var reloadAxis1 = pm2.Axes.Axes[1];
            var reloadAxis2 = pm2.Axes.Axes[2];
            Assert.That(reloadAxis0.IsVisible, Is.True);
            Assert.That(reloadAxis1.IsVisible, Is.False);
            Assert.That(reloadAxis2.IsVisible, Is.True);
        }

        [Test]
        public void FullCycle_AxisRangeFixed_RoundTrips()
        {
            // Arrange
            var model = CreateGraphModelWithTwoAxes();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            var yAxisItem = pm1.Axes.Axes[1];
            Assert.That(yAxisItem.HasFixedRange, Is.False);

            // Modify - set fixed range
            yAxisItem.HasFixedRange = true;
            yAxisItem.Minimum = 10.0;
            yAxisItem.Maximum = 100.0;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);
            var yAxisItem2 = pm2.Axes.Axes[1];

            // Assert - note: the implementation may not currently persist these values
            // This test will help identify missing functionality
            Assert.That(yAxisItem2.HasFixedRange, Is.True,
                "Fixed range flag should round-trip (this may fail if not implemented)");
            Assert.That(yAxisItem2.Minimum, Is.EqualTo(10.0).Within(1e-9),
                "Minimum range value should round-trip (this may fail if not implemented)");
            Assert.That(yAxisItem2.Maximum, Is.EqualTo(100.0).Within(1e-9),
                "Maximum range value should round-trip (this may fail if not implemented)");
        }

        [Test]
        public void FullCycle_AxisTitleOverride_RoundTrips()
        {
            // Arrange
            var model = CreateGraphModelWithTwoAxes();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            var yAxisItem = pm1.Axes.Axes[1];
            Assert.That(yAxisItem.HasTitleOverride, Is.False);

            // Modify - set title override
            yAxisItem.HasTitleOverride = true;
            yAxisItem.Title = "Custom Axis Title";

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);
            var yAxisItem2 = pm2.Axes.Axes[1];

            // Assert - note: the implementation may not currently persist these values
            Assert.That(yAxisItem2.HasTitleOverride, Is.True,
                "Title override flag should round-trip (this may fail if not implemented)");
            Assert.That(yAxisItem2.Title, Is.EqualTo("Custom Axis Title"),
                "Axis title override should round-trip (this may fail if not implemented)");
        }

        [Test]
        public void FullCycle_AxisIncrement_RoundTrips()
        {
            // Arrange
            var model = CreateGraphModelWithTwoAxes();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            var yAxisItem = pm1.Axes.Axes[1];
            Assert.That(yAxisItem.HasFixedIncrement, Is.False);

            // Modify - set fixed increment
            yAxisItem.HasFixedIncrement = true;
            yAxisItem.Increment = 5.0;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);
            var yAxisItem2 = pm2.Axes.Axes[1];

            // Assert - note: the implementation may not currently persist these values
            Assert.That(yAxisItem2.HasFixedIncrement, Is.True,
                "Fixed increment flag should round-trip (this may fail if not implemented)");
            Assert.That(yAxisItem2.Increment, Is.EqualTo(5.0).Within(1e-9),
                "Axis increment should round-trip (this may fail if not implemented)");
        }

        [Test]
        public void FullCycle_ShowAxisPreserveWhenNotModified()
        {
            // Arrange - start with a hidden axis
            var model = CreateGraphModelWithTwoAxes();
            var optionsWithHiddenAxis = new GraphPresentationOptions(
                hiddenAxisIds: new[] { new AxisId("y-axis") });

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, optionsWithHiddenAxis);
            var yAxisItem = pm1.Axes.Axes[1];
            Assert.That(yAxisItem.IsVisible, Is.False);

            // Don't modify anything

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);
            var yAxisItem2 = pm2.Axes.Axes[1];

            // Assert
            Assert.That(yAxisItem2.IsVisible, Is.False,
                "Axis visibility should be preserved when not modified");
        }

        [Test]
        public void FullCycle_UnhideAxis_RoundTrips()
        {
            // Arrange - start with a hidden axis
            var model = CreateGraphModelWithTwoAxes();
            var optionsWithHiddenAxis = new GraphPresentationOptions(
                hiddenAxisIds: new[] { new AxisId("y-axis") });

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, optionsWithHiddenAxis);
            var yAxisItem = pm1.Axes.Axes[1];
            Assert.That(yAxisItem.IsVisible, Is.False);

            // Modify - show the axis
            yAxisItem.IsVisible = true;

            // Apply
            var appliedOptions = pm1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.HiddenAxisIds.Count, Is.EqualTo(0),
                "Hidden axes list should be empty after showing all axes");

            // Reload
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);
            var yAxisItem2 = pm2.Axes.Axes[1];

            // Assert
            Assert.That(yAxisItem2.IsVisible, Is.True);
        }

        [Test]
        public void FullCycle_AllAxesHidden_ThenOneShown()
        {
            // Arrange
            var model = CreateGraphModelWithMultipleAxes();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);

            // Modify - hide all axes
            foreach (var axis in pm1.Axes.Axes)
            {
                axis.IsVisible = false;
            }

            // Apply
            var optionsAllHidden = pm1.BuildGraphPresentationOptions();
            Assert.That(optionsAllHidden.HiddenAxisIds.Count, Is.EqualTo(3));

            // Reload and modify - show one axis
            var pm2 = new GraphOptionsPresentationModel(model, optionsAllHidden);
            pm2.Axes.Axes[0].IsVisible = true;

            // Apply again
            var optionsOneShown = pm2.BuildGraphPresentationOptions();
            Assert.That(optionsOneShown.HiddenAxisIds.Count, Is.EqualTo(2));

            // Reload and verify
            var pm3 = new GraphOptionsPresentationModel(model, optionsOneShown);
            Assert.That(pm3.Axes.Axes[0].IsVisible, Is.True);
            Assert.That(pm3.Axes.Axes[1].IsVisible, Is.False);
            Assert.That(pm3.Axes.Axes[2].IsVisible, Is.False);
        }

        [Test]
        public void FullCycle_AxisCount_RemainsConsistent()
        {
            // Verify that the number of axes in the editor model matches the graph model
            var model = CreateGraphModelWithMultipleAxes();
            var defaultOptions = new GraphPresentationOptions();

            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(pm1.Axes.Axes.Count, Is.EqualTo(3));

            var appliedOptions = pm1.BuildGraphPresentationOptions();
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            Assert.That(pm2.Axes.Axes.Count, Is.EqualTo(3),
                "Axis count should match graph model axes");
        }

        [Test]
        public void FullCycle_AxisOrder_PreservedAcrossReload()
        {
            // Verify axis order is preserved
            var model = CreateGraphModelWithMultipleAxes();
            var defaultOptions = new GraphPresentationOptions();

            var pm1 = new GraphOptionsPresentationModel(model, defaultOptions);
            var originalIds = new[] { pm1.Axes.Axes[0].AxisId, pm1.Axes.Axes[1].AxisId, pm1.Axes.Axes[2].AxisId };

            var appliedOptions = pm1.BuildGraphPresentationOptions();
            var pm2 = new GraphOptionsPresentationModel(model, appliedOptions);

            Assert.That(pm2.Axes.Axes[0].AxisId, Is.EqualTo(originalIds[0]));
            Assert.That(pm2.Axes.Axes[1].AxisId, Is.EqualTo(originalIds[1]));
            Assert.That(pm2.Axes.Axes[2].AxisId, Is.EqualTo(originalIds[2]));
        }

        private static IGraphModel CreateGraphModelWithTwoAxes()
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

        private static IGraphModel CreateGraphModelWithMultipleAxes()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(
                new AxisId("x-axis"),
                ModelAxisOrientation.X,
                ModelAxisSide.Bottom,
                unit,
                "m",
                null);
            var yAxis1 = new AxisModel(
                new AxisId("y-axis-1"),
                ModelAxisOrientation.Y,
                ModelAxisSide.Left,
                unit,
                "m",
                null);
            var yAxis2 = new AxisModel(
                new AxisId("y-axis-2"),
                ModelAxisOrientation.Y,
                ModelAxisSide.Left,
                unit,
                "m",
                null);

            var xField = new TestFieldDefinition("X", "x", unit, new double[] { 0d, 1d });
            var yField1 = new TestFieldDefinition("Y1", "y1", unit, new double[] { 0d, 1d });
            var yField2 = new TestFieldDefinition("Y2", "y2", unit, new double[] { 10d, 20d });

            var series1 = new GraphSeriesModel(
                new SeriesId("series-1"),
                "series-1",
                SeriesType.Line,
                xField,
                yField1,
                xAxis,
                yAxis1);

            var series2 = new GraphSeriesModel(
                new SeriesId("series-2"),
                "series-2",
                SeriesType.Line,
                xField,
                yField2,
                xAxis,
                yAxis2);

            return new GraphModel(new[] { xAxis, yAxis1, yAxis2 }, new[] { series1, series2 });
        }
    }
}
