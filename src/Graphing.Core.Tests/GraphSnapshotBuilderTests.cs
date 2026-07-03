using System;
using System.Linq;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Snapshot;
using NUnit.Framework;
using UnitRegistry.Formatting;
using UnitRegistry;

namespace Graphing.Core.Tests
{
    [TestFixture]
    public class GraphSnapshotBuilderTests
    {
        [Test]
        public void Build_Throws_WhenAxisIdsAreDuplicated()
        {
            var unit = Units.Length.Meter;
            var duplicateAxisId = new AxisId("axis-duplicate");

            var xAxis = new AxisModel(duplicateAxisId, AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(duplicateAxisId, AxisOrientation.Y, AxisSide.Left, unit, "m", null);

            var model = new GraphModel(new IAxisModel[] { xAxis, yAxis }, new IGraphSeriesModel[0]);
            var builder = new GraphSnapshotBuilder();

            Assert.That(
                () => builder.Build(model),
                Throws.TypeOf<System.InvalidOperationException>()
                    .With.Message.Contains("duplicate AxisId")
                    .And.Message.Contains("axis-duplicate"));
        }

        [Test]
        public void Build_AppliesSeriesOrder_FromPresentationOptions()
        {
            var model = CreateGraphModelWithMultipleSeries();
            var options = new Graphing.Controls.Presentation.GraphPresentationOptions(
                seriesOrder: new[]
                {
                    new SeriesId("series-3"),
                    new SeriesId("series-1"),
                    new SeriesId("series-2")
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);

            Assert.That(snapshot.Series[0].SeriesId, Is.EqualTo(new SeriesId("series-3")));
            Assert.That(snapshot.Series[1].SeriesId, Is.EqualTo(new SeriesId("series-1")));
            Assert.That(snapshot.Series[2].SeriesId, Is.EqualTo(new SeriesId("series-2")));
        }

        [Test]
        public void Build_IgnoresUnknownSeriesIds_AndAppendsNewSeriesInModelOrder()
        {
            var model = CreateGraphModelWithFourSeries();
            var options = new Graphing.Controls.Presentation.GraphPresentationOptions(
                seriesOrder: new[]
                {
                    new SeriesId("series-2"),
                    new SeriesId("missing-series"),
                    new SeriesId("series-1")
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);

            Assert.That(snapshot.Series[0].SeriesId, Is.EqualTo(new SeriesId("series-2")));
            Assert.That(snapshot.Series[1].SeriesId, Is.EqualTo(new SeriesId("series-1")));
            Assert.That(snapshot.Series[2].SeriesId, Is.EqualTo(new SeriesId("series-3")));
            Assert.That(snapshot.Series[3].SeriesId, Is.EqualTo(new SeriesId("series-4")));
        }

        [Test]
        public void PresentationAndLegend_FollowSnapshotSeriesOrder()
        {
            var model = CreateGraphModelWithMultipleSeries();
            var options = new Graphing.Controls.Presentation.GraphPresentationOptions(
                seriesOrder: new[]
                {
                    new SeriesId("series-2"),
                    new SeriesId("series-3"),
                    new SeriesId("series-1")
                });

            var snapshot = new GraphSnapshotBuilder().Build(model, options);
            var presentation = new Graphing.Controls.Presentation.GraphPresentationModel(snapshot, options);

            Assert.That(presentation.Series[0].SeriesId, Is.EqualTo(new SeriesId("series-2")));
            Assert.That(presentation.Series[1].SeriesId, Is.EqualTo(new SeriesId("series-3")));
            Assert.That(presentation.Series[2].SeriesId, Is.EqualTo(new SeriesId("series-1")));
            Assert.That(presentation.Layout.Legend, Is.Not.Null);
            Assert.That(presentation.Layout.Legend.Entries[0].SeriesId, Is.EqualTo(new SeriesId("series-2")));
            Assert.That(presentation.Layout.Legend.Entries[1].SeriesId, Is.EqualTo(new SeriesId("series-3")));
            Assert.That(presentation.Layout.Legend.Entries[2].SeriesId, Is.EqualTo(new SeriesId("series-1")));
        }

        [Test]
        public void Build_CarriesLineRenderMode_FromModelToSnapshot()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", null);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 2d, 3d });
            var series = new GraphSeriesModel(
                new SeriesId("series-1"),
                "series-1",
                SeriesType.Line,
                xField,
                yField,
                xAxis,
                yAxis,
                LineRenderMode.LineAndPoints);

            var model = new GraphModel(new[] { xAxis, yAxis }, new IGraphSeriesModel[] { series });

            var snapshot = new GraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.Series[0].LineRenderMode, Is.EqualTo(LineRenderMode.LineAndPoints));
        }

        [Test]
        public void GraphSeriesModel_DefaultsLineRenderMode_ToLineOnly()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", null);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 2d, 3d });

            var series = new GraphSeriesModel(new SeriesId("series-1"), "series-1", SeriesType.Line, xField, yField, xAxis, yAxis);

            Assert.That(series.LineRenderMode, Is.EqualTo(LineRenderMode.LineOnly));
        }

        [Test]
        public void Build_CopiesAxisLabelValueConverter_FromAxisModelToSnapshot()
        {
            var unit = Units.Length.Meter;
            var converter = new TestAxisLabelValueConverter();
            var xAxis = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", null, converter);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d });
            var series = new GraphSeriesModel(new SeriesId("series-1"), "series-1", SeriesType.Line, xField, yField, xAxis, yAxis);
            var model = new GraphModel(new[] { xAxis, yAxis }, new IGraphSeriesModel[] { series });

            var snapshot = new GraphSnapshotBuilder().Build(model);
            var ySnapshot = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(ySnapshot.LabelValueConverter, Is.SameAs(converter));
        }

        [Test]
        public void Build_LeavesAxisLabelValueConverterNull_WhenAxisModelHasNoConverter()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", null);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d });
            var series = new GraphSeriesModel(new SeriesId("series-1"), "series-1", SeriesType.Line, xField, yField, xAxis, yAxis);
            var model = new GraphModel(new[] { xAxis, yAxis }, new IGraphSeriesModel[] { series });

            var snapshot = new GraphSnapshotBuilder().Build(model);
            var ySnapshot = snapshot.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(ySnapshot.LabelValueConverter, Is.Null);
        }

        [Test]
        public void Build_PreservesNumericSnapshotBehavior_WhenLabelValueConverterIsPresent()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;
            var formatter = new NumericFormatter("fmt-y", registry, "Y", "F2");
            var converter = new TestAxisLabelValueConverter();
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d, 3d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 40d, 60d });

            var xAxisWithoutConverter = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxisWithoutConverter = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", formatter);
            var seriesWithoutConverter = new GraphSeriesModel(new SeriesId("series-1"), "series-1", SeriesType.Line, xField, yField, xAxisWithoutConverter, yAxisWithoutConverter);
            var modelWithoutConverter = new GraphModel(new[] { xAxisWithoutConverter, yAxisWithoutConverter }, new IGraphSeriesModel[] { seriesWithoutConverter });

            var xAxisWithConverter = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxisWithConverter = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", formatter, converter);
            var seriesWithConverter = new GraphSeriesModel(new SeriesId("series-1"), "series-1", SeriesType.Line, xField, yField, xAxisWithConverter, yAxisWithConverter);
            var modelWithConverter = new GraphModel(new[] { xAxisWithConverter, yAxisWithConverter }, new IGraphSeriesModel[] { seriesWithConverter });

            var snapshotWithoutConverter = new GraphSnapshotBuilder().Build(modelWithoutConverter);
            var snapshotWithConverter = new GraphSnapshotBuilder().Build(modelWithConverter);

            var ySnapshotWithoutConverter = snapshotWithoutConverter.Axes.Single(a => a.AxisId == "y-axis");
            var ySnapshotWithConverter = snapshotWithConverter.Axes.Single(a => a.AxisId == "y-axis");

            Assert.That(ySnapshotWithoutConverter.MinimumValue, Is.EqualTo(ySnapshotWithConverter.MinimumValue));
            Assert.That(ySnapshotWithoutConverter.MaximumValue, Is.EqualTo(ySnapshotWithConverter.MaximumValue));
            Assert.That(ySnapshotWithoutConverter.Increment, Is.EqualTo(ySnapshotWithConverter.Increment));
            Assert.That(ySnapshotWithoutConverter.IsAutoIncrement, Is.EqualTo(ySnapshotWithConverter.IsAutoIncrement));
            Assert.That(ySnapshotWithoutConverter.MajorTickStride, Is.EqualTo(ySnapshotWithConverter.MajorTickStride));
            Assert.That(ySnapshotWithoutConverter.FormatterName, Is.EqualTo(ySnapshotWithConverter.FormatterName));
            Assert.That(ySnapshotWithoutConverter.LabelValueConverter, Is.Null);
            Assert.That(ySnapshotWithConverter.LabelValueConverter, Is.SameAs(converter));
        }

        private static IGraphModel CreateGraphModelWithMultipleSeries()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField1 = new TestFieldDefinition("Y1", "y1", unit, new[] { 0d, 1d });
            var yField2 = new TestFieldDefinition("Y2", "y2", unit, new[] { 2d, 3d });
            var yField3 = new TestFieldDefinition("Y3", "y3", unit, new[] { 4d, 5d });

            return new GraphModel(
                new[] { xAxis, yAxis },
                new IGraphSeriesModel[]
                {
                    new GraphSeriesModel(new SeriesId("series-1"), "series-1", SeriesType.Line, xField, yField1, xAxis, yAxis),
                    new GraphSeriesModel(new SeriesId("series-2"), "series-2", SeriesType.Line, xField, yField2, xAxis, yAxis),
                    new GraphSeriesModel(new SeriesId("series-3"), "series-3", SeriesType.Line, xField, yField3, xAxis, yAxis)
                });
        }

        private static IGraphModel CreateGraphModelWithFourSeries()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField1 = new TestFieldDefinition("Y1", "y1", unit, new[] { 0d, 1d });
            var yField2 = new TestFieldDefinition("Y2", "y2", unit, new[] { 2d, 3d });
            var yField3 = new TestFieldDefinition("Y3", "y3", unit, new[] { 4d, 5d });
            var yField4 = new TestFieldDefinition("Y4", "y4", unit, new[] { 6d, 7d });

            return new GraphModel(
                new[] { xAxis, yAxis },
                new IGraphSeriesModel[]
                {
                    new GraphSeriesModel(new SeriesId("series-1"), "series-1", SeriesType.Line, xField, yField1, xAxis, yAxis),
                    new GraphSeriesModel(new SeriesId("series-2"), "series-2", SeriesType.Line, xField, yField2, xAxis, yAxis),
                    new GraphSeriesModel(new SeriesId("series-3"), "series-3", SeriesType.Line, xField, yField3, xAxis, yAxis),
                    new GraphSeriesModel(new SeriesId("series-4"), "series-4", SeriesType.Line, xField, yField4, xAxis, yAxis)
                });
        }

        private sealed class TestFieldDefinition : IGraphFieldDefinition
        {
            private readonly Array _values;

            public TestFieldDefinition(string label, string name, Unit unit, Array values)
            {
                Label = label;
                Name = name;
                Unit = unit;
                _values = values;
            }

            public string Label { get; }

            public string Name { get; }

            public Unit Unit { get; }

            public Array GetValues()
            {
                return _values;
            }
        }

        private sealed class TestAxisLabelValueConverter : IAxisLabelValueConverter
        {
            public Type TargetValueType => typeof(string);

            public object Convert(double coordinateValue, IFormatProvider formatProvider = null)
            {
                return coordinateValue.ToString("G", formatProvider);
            }
        }
    }
}
