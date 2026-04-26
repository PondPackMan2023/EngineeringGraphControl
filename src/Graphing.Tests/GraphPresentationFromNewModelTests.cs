using System;
using System.Collections.Generic;
using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.Controls.Snapshot;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;
using PresentationAxisOrientation = Graphing.Controls.Presentation.AxisOrientation;
using PresentationAxisSide = Graphing.Controls.Presentation.AxisSide;

namespace Graphing.Tests
{

    [TestFixture]
    public class GraphPresentationFromNewModelTests
    {
        [Test]
        public void Presentation_UsesExplicitAxisIdentityOrientationAndSide()
        {
            var model = CreateModel(chartType: ChartType.Line);

            var snapshot = BuildSnapshotFromModel(model);
            var presentation = new GraphPresentationModel(snapshot);

            Assert.That(presentation.Axes.Count, Is.EqualTo(2));
            Assert.That(presentation.Axes[0].AxisId, Is.EqualTo("x-axis"));
            Assert.That(presentation.Axes[0].Orientation, Is.EqualTo(PresentationAxisOrientation.Horizontal));
            Assert.That(presentation.Axes[0].Side, Is.EqualTo(PresentationAxisSide.Bottom));

            Assert.That(presentation.Axes[1].AxisId, Is.EqualTo("y-axis"));
            Assert.That(presentation.Axes[1].Orientation, Is.EqualTo(PresentationAxisOrientation.Vertical));
            Assert.That(presentation.Axes[1].Side, Is.EqualTo(PresentationAxisSide.Left));
        }

        [Test]
        public void ChartType_ActsAsRenderingIntent_AndDoesNotAffectAxisSelection()
        {
            var lineModel = CreateModel(chartType: ChartType.Line);
            var barModel = CreateModel(chartType: ChartType.Bar);

            var linePresentation = new GraphPresentationModel(BuildSnapshotFromModel(lineModel));
            var barPresentation = new GraphPresentationModel(BuildSnapshotFromModel(barModel));

            Assert.That(linePresentation.Series[0].ChartType, Is.EqualTo(ChartType.Line));
            Assert.That(barPresentation.Series[0].ChartType, Is.EqualTo(ChartType.Bar));

            Assert.That(linePresentation.Axes[0].AxisId, Is.EqualTo(barPresentation.Axes[0].AxisId));
            Assert.That(linePresentation.Axes[1].AxisId, Is.EqualTo(barPresentation.Axes[1].AxisId));
            Assert.That(linePresentation.Axes[0].Orientation, Is.EqualTo(barPresentation.Axes[0].Orientation));
            Assert.That(linePresentation.Axes[1].Orientation, Is.EqualTo(barPresentation.Axes[1].Orientation));
        }

        [Test]
        public void AxisFormatter_IsExplicitlySupplied_ByAxisModel()
        {
            var model = CreateModel(chartType: ChartType.Line);

            var snapshot = BuildSnapshotFromModel(model);

            Assert.That(snapshot.Axes[0].FormatterName, Is.Null);
            Assert.That(snapshot.Axes[1].FormatterName, Is.EqualTo("formatter-y"));
            Assert.That(snapshot.Axes[1].DisplayUnitLabel, Is.EqualTo("m"));
        }

        private static IGraphModel CreateModel(ChartType chartType)
        {
            var registry = UnitRegistry.UnitRegistry.Default;
            var unit = Units.Length.Meter;
            var formatter = new NumericFormatter("formatter-y", registry, "F2");

            var xAxis = new AxisModel("x-axis", ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel("y-axis", ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatter);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d });

            var series = new GraphSeriesModel(1, "series-1", chartType, xField, yField, xAxis, yAxis);

            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static IGraphSnapshot BuildSnapshotFromModel(IGraphModel model)
        {
            var seriesSnapshots = new List<ISeriesSnapshot>();
            foreach (var series in model.Series)
            {
                var xField = new TestFieldSnapshot(
                    series.XField.Label,
                    series.XField.Name,
                    series.XField.GetValues(),
                    series.XField.Unit,
                    series.XAxis.UnitLabel,
                    series.XAxis.NumericFormatter != null ? series.XAxis.NumericFormatter.Id.ToString() : null,
                    series.XAxis.NumericFormatter);

                var yField = new TestFieldSnapshot(
                    series.YField.Label,
                    series.YField.Name,
                    series.YField.GetValues(),
                    series.YField.Unit,
                    series.YAxis.UnitLabel,
                    series.YAxis.NumericFormatter != null ? series.YAxis.NumericFormatter.Id.ToString() : null,
                    series.YAxis.NumericFormatter);

                seriesSnapshots.Add(
                    new TestSeriesSnapshot(
                        series.Identifier,
                        series.Id,
                        series.Label,
                        series.ChartType,
                        series.XAxis.Id,
                        series.YAxis.Id,
                        xField,
                        yField));
            }

            var axisSnapshots = new List<IAxisSnapshot>();
            foreach (var axis in model.Axes)
            {
                axisSnapshots.Add(
                    new TestAxisSnapshot(
                        axis.Id,
                        axis.Orientation,
                        axis.Side,
                        axis.NumericFormatter != null ? axis.NumericFormatter.Id.ToString() : null,
                        axis.Unit,
                        axis.UnitLabel,
                        axis.ScaleType,
                        axis.IsAutoRange,
                        Array.Empty<IFieldSnapshot>(),
                        axis.MinimumValue,
                        axis.MaximumValue));
            }

            return new TestGraphSnapshot(seriesSnapshots, axisSnapshots);
        }

        private sealed class TestFieldDefinition : GraphFieldDefinitionBase
        {
            private readonly Array _values;

            public TestFieldDefinition(string label, string name, Unit unit, Array values)
                : base(name, label, unit)
            {
                _values = values;
            }

            public override Array GetValues()
            {
                return _values;
            }
        }

        private sealed class TestGraphSnapshot : IGraphSnapshot
        {
            public TestGraphSnapshot(IReadOnlyList<ISeriesSnapshot> series, IReadOnlyList<IAxisSnapshot> axes)
            {
                Series = series;
                Axes = axes;
            }

            public IReadOnlyList<ISeriesSnapshot> Series { get; }
            public IReadOnlyList<IAxisSnapshot> Axes { get; }
        }

        private sealed class TestSeriesSnapshot : ISeriesSnapshot
        {
            public TestSeriesSnapshot(
                object identifier,
                int id,
                string label,
                ChartType chartType,
                string xAxisId,
                string yAxisId,
                IFieldSnapshot xField,
                IFieldSnapshot yField)
            {
                Identifier = identifier;
                Id = id;
                Label = label;
                ChartType = chartType;
                XAxisId = xAxisId;
                YAxisId = yAxisId;
                XField = xField;
                YField = yField;
                Fields = new[] { xField, yField };
            }

            public object Identifier { get; }
            public int Id { get; }
            public string Label { get; }
            public ChartType ChartType { get; }
            public IFieldSnapshot XField { get; }
            public IFieldSnapshot YField { get; }
            public string XAxisId { get; }
            public string YAxisId { get; }
            public IReadOnlyList<IFieldSnapshot> Fields { get; }
        }

        private sealed class TestFieldSnapshot : IFieldSnapshot
        {
            public TestFieldSnapshot(
                string label,
                string name,
                Array values,
                Unit unit,
                string displayUnitLabel,
                string formatterName,
                NumericFormatter formatter)
            {
                Label = label;
                Name = name;
                Values = values;
                Unit = unit;
                DisplayUnitLabel = displayUnitLabel;
                FormatterName = formatterName;
                Formatter = formatter;
            }

            public string Label { get; }
            public string Name { get; }
            public Array Values { get; }
            public Unit Unit { get; }
            public string DisplayUnitLabel { get; }
            public string FormatterName { get; }
            public NumericFormatter Formatter { get; }
        }

        private sealed class TestAxisSnapshot : IAxisSnapshot
        {
            public TestAxisSnapshot(
                string axisId,
                ModelAxisOrientation orientation,
                ModelAxisSide side,
                string formatterName,
                Unit unit,
                string displayUnitLabel,
                AxisScaleType scaleType,
                bool isAutoRange,
                IReadOnlyList<IFieldSnapshot> fields,
                double? minimumValue,
                double? maximumValue)
            {
                AxisId = axisId;
                Orientation = orientation;
                Side = side;
                FormatterName = formatterName;
                Unit = unit;
                DisplayUnitLabel = displayUnitLabel;
                ScaleType = scaleType;
                IsAutoRange = isAutoRange;
                Fields = fields;
                MinimumValue = minimumValue;
                MaximumValue = maximumValue;
            }

            public string AxisId { get; }
            public ModelAxisOrientation Orientation { get; }
            public ModelAxisSide Side { get; }
            public string FormatterName { get; }
            public Unit Unit { get; }
            public AxisScaleType ScaleType { get; }
            public bool IsAutoRange { get; }
            public string DisplayUnitLabel { get; }
            public IReadOnlyList<IFieldSnapshot> Fields { get; }
            public double? MinimumValue { get; }
            public double? MaximumValue { get; }
        }
    }
}
