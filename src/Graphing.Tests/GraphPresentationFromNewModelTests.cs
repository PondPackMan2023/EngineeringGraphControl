using System;
using System.Linq;
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
        private const double AxisStackGap = 0.025;

        [Test]
        public void Presentation_UsesExplicitAxisIdentityOrientationAndSide()
        {
            var model = CreateModel(chartType: ChartType.Line);

            var snapshot = new GraphSnapshotBuilder().Build(model);
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

            var linePresentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(lineModel));
            var barPresentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(barModel));

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

            var snapshot = new GraphSnapshotBuilder().Build(model);

            Assert.That(snapshot.Axes[0].FormatterName, Is.Null);
            Assert.That(snapshot.Axes[1].FormatterName, Is.EqualTo("formatter-y"));
            Assert.That(snapshot.Axes[1].DisplayUnitLabel, Is.EqualTo("m"));
        }

        [Test]
        public void AxisTitle_UpdatesWhenAxisUnitChanges()
        {
            var timeDimension = new Dimension("time");
            var hoursUnit = new Unit("hr", timeDimension, 3600.0);
            var secondsUnit = new Unit("s", timeDimension, 1.0);

            var timeAxisId = new AxisId("time-axis");
            var valueAxisId = new AxisId("value-axis");

            var timeAxis = new AxisModel(timeAxisId, ModelAxisOrientation.X, ModelAxisSide.Bottom, hoursUnit, "hr", null);
            var valueAxis = new AxisModel(valueAxisId, ModelAxisOrientation.Y, ModelAxisSide.Left, hoursUnit, "hr", null);

            var timeField = new TestFieldDefinition("Time", "time", hoursUnit, new[] { 1d, 2d, 3d });
            var valueField = new TestFieldDefinition("Value", "value", hoursUnit, new[] { 10d, 20d, 30d });

            var series = new GraphSeriesModel(1, "series-1", ChartType.Line, timeField, valueField, timeAxis, valueAxis);
            var model = new GraphModel(new[] { timeAxis, valueAxis }, new[] { series });

            var builder = new GraphSnapshotBuilder();

            var snapshot1 = builder.Build(model);
            var timeAxisSnapshot1 = snapshot1.Axes.First(a => a.AxisId == "time-axis");
            Assert.That(timeAxisSnapshot1.Title, Is.EqualTo("Time (hr)"));

            var updatedModel = model.ChangeAxisUnit(timeAxisId, secondsUnit);
            var snapshot2 = builder.Build(updatedModel);

            var timeAxisSnapshot2 = snapshot2.Axes.First(a => a.AxisId == "time-axis");
            Assert.That(timeAxisSnapshot2.Title, Is.EqualTo("Time (s)"));
        }

        [Test]
        public void AxisFormatter_UpdatesWhenAxisFormatChanges_AndUnitRemainsUnchanged()
        {
            var model = CreateModel(chartType: ChartType.Line);
            var xAxisId = new AxisId("x-axis");
            var yAxisId = new AxisId("y-axis");
            var formatterX = new NumericFormatter("formatter-x", UnitsRegistry.Default, "F1");
            var formatterY = new NumericFormatter("formatter-y-2", UnitsRegistry.Default, "F3");

            var originalXAxis = model.Axes.First(a => a.Id.Equals(xAxisId));
            var originalYAxis = model.Axes.First(a => a.Id.Equals(yAxisId));

            var xUpdatedModel = model.ChangeAxisFormat(xAxisId, formatterX);
            var yUpdatedModel = xUpdatedModel.ChangeAxisFormat(yAxisId, formatterY);

            var xAxis = yUpdatedModel.Axes.First(a => a.Id.Equals(xAxisId));
            var yAxis = yUpdatedModel.Axes.First(a => a.Id.Equals(yAxisId));

            Assert.That(xAxis.Unit, Is.SameAs(originalXAxis.Unit));
            Assert.That(yAxis.Unit, Is.SameAs(originalYAxis.Unit));

            var snapshot = new GraphSnapshotBuilder().Build(yUpdatedModel);
            Assert.That(snapshot.Axes.First(a => a.AxisId == "x-axis").FormatterName, Is.EqualTo("formatter-x"));
            Assert.That(snapshot.Axes.First(a => a.AxisId == "y-axis").FormatterName, Is.EqualTo("formatter-y-2"));
        }

        [Test]
        public void AxisUnitAndFormat_ChangeAtomically_WithSingleRebuildStep()
        {
            var registry = UnitsRegistry.Default;
            var timeDimension = new Dimension("time");
            var hoursUnit = new Unit("hr", timeDimension, 3600.0);
            var secondsUnit = new Unit("s", timeDimension, 1.0);

            var timeAxisId = new AxisId("time-axis");
            var valueAxisId = new AxisId("value-axis");
            var formatter = new NumericFormatter("formatter-seconds", registry, "F4");

            var timeAxis = new AxisModel(timeAxisId, ModelAxisOrientation.X, ModelAxisSide.Bottom, hoursUnit, "hr", null);
            var valueAxis = new AxisModel(valueAxisId, ModelAxisOrientation.Y, ModelAxisSide.Left, hoursUnit, "hr", null);

            var timeField = new TestFieldDefinition("Time", "time", hoursUnit, new[] { 1d, 2d, 3d });
            var valueField = new TestFieldDefinition("Value", "value", hoursUnit, new[] { 10d, 20d, 30d });

            var series = new GraphSeriesModel(1, "series-1", ChartType.Line, timeField, valueField, timeAxis, valueAxis);
            var model = new GraphModel(new[] { timeAxis, valueAxis }, new[] { series });

            var updatedModel = model.ChangeAxisUnitAndFormat(timeAxisId, secondsUnit, formatter);

            var rebuildCount = 0;
            var builder = new GraphSnapshotBuilder();
            IGraphSnapshot BuildSnapshot(IGraphModel graph)
            {
                rebuildCount++;
                return builder.Build(graph);
            }

            var snapshot = BuildSnapshot(updatedModel);

            var timeAxisSnapshot = snapshot.Axes.First(a => a.AxisId == "time-axis");
            Assert.That(timeAxisSnapshot.Title, Is.EqualTo("Time (s)"));
            Assert.That(timeAxisSnapshot.FormatterName, Is.EqualTo("formatter-seconds"));
            Assert.That(rebuildCount, Is.EqualTo(1));
        }

        [Test]
        public void AxisTickLabels_UseNumericFormatter_WhenFormatterIsPresent()
        {
            var registry = UnitsRegistry.Default;
            var unit = Units.Length.Meter;
            var formatter = new NumericFormatter("fmt-f4", registry, "F4");

            var yAxisId = new AxisId("y-axis");
            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(yAxisId, ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatter);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });

            var series = new GraphSeriesModel(1, "s", ChartType.Line, xField, yField, xAxis, yAxis);
            var model = new GraphModel(new[] { xAxis, yAxis }, new[] { series });

            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var yAxisPresentation = presentation.Axes.First(a => a.AxisId == "y-axis");
            Assert.That(yAxisPresentation.Ticks.Count, Is.GreaterThan(0));

            foreach (var tick in yAxisPresentation.Ticks)
            {
                var expected = formatter.Format(tick.Value);
                Assert.That(tick.Label, Is.EqualTo(expected));
            }
        }

        [Test]
        public void AxisTickLabels_ReflectFormatterPrecision_WhenPrecisionChanges()
        {
            var registry = UnitsRegistry.Default;
            var unit = Units.Length.Meter;
            var yAxisId = new AxisId("y-axis");

            var formatterF2 = new NumericFormatter("fmt-f2", registry, "F2");
            var formatterF6 = new NumericFormatter("fmt-f6", registry, "F6");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxisF2 = new AxisModel(yAxisId, ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatterF2);
            var yAxisF6 = new AxisModel(yAxisId, ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatterF6);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });

            var builder = new GraphSnapshotBuilder();

            var seriesF2 = new GraphSeriesModel(1, "s", ChartType.Line, xField, yField, xAxis, yAxisF2);
            var modelF2 = new GraphModel(new[] { xAxis, yAxisF2 }, new[] { seriesF2 });
            var presentationF2 = new GraphPresentationModel(builder.Build(modelF2));
            var labelsF2 = presentationF2.Axes.First(a => a.AxisId == "y-axis").Ticks.Select(t => t.Label).ToArray();

            var seriesF6 = new GraphSeriesModel(1, "s", ChartType.Line, xField, yField, xAxis, yAxisF6);
            var modelF6 = new GraphModel(new[] { xAxis, yAxisF6 }, new[] { seriesF6 });
            var presentationF6 = new GraphPresentationModel(builder.Build(modelF6));
            var labelsF6 = presentationF6.Axes.First(a => a.AxisId == "y-axis").Ticks.Select(t => t.Label).ToArray();

            Assert.That(labelsF2, Is.Not.EqualTo(labelsF6), "F2 and F6 labels should differ.");
            Assert.That(labelsF2.All(l => l.Contains(".")), Is.True, "F2 labels should have decimal point.");
            Assert.That(labelsF6.All(l => l.Contains(".")), Is.True, "F6 labels should have decimal point.");
        }

        [Test]
        public void AxisTickLabels_ReflectFormatterCulture_WhenCultureChanges()
        {
            var registry = UnitsRegistry.Default;
            var unit = Units.Length.Meter;
            var yAxisId = new AxisId("y-axis");

            var invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
            var germanCulture = new System.Globalization.CultureInfo("de-DE");

            var formatterInvariant = new NumericFormatter("fmt-inv", registry, "F2", invariantCulture);
            var formatterGerman = new NumericFormatter("fmt-de", registry, "F2", germanCulture);

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });
            var builder = new GraphSnapshotBuilder();

            var yAxisInv = new AxisModel(yAxisId, ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatterInvariant);
            var seriesInv = new GraphSeriesModel(1, "s", ChartType.Line, xField, yField, xAxis, yAxisInv);
            var modelInv = new GraphModel(new[] { xAxis, yAxisInv }, new[] { seriesInv });
            var presentationInv = new GraphPresentationModel(builder.Build(modelInv));

            var yAxisDe = new AxisModel(yAxisId, ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatterGerman);
            var seriesDe = new GraphSeriesModel(1, "s", ChartType.Line, xField, yField, xAxis, yAxisDe);
            var modelDe = new GraphModel(new[] { xAxis, yAxisDe }, new[] { seriesDe });
            var presentationDe = new GraphPresentationModel(builder.Build(modelDe));

            var ticksInv = presentationInv.Axes.First(a => a.AxisId == "y-axis").Ticks;
            var ticksDe = presentationDe.Axes.First(a => a.AxisId == "y-axis").Ticks;

            Assert.That(ticksInv.Count, Is.GreaterThan(0));
            Assert.That(ticksInv.Count, Is.EqualTo(ticksDe.Count));

            var nonZeroInv = ticksInv.FirstOrDefault(t => t.Value != 0d && t.Value != Math.Floor(t.Value));
            if (nonZeroInv != null)
            {
                var correspondingDe = ticksDe.First(t => t.Value == nonZeroInv.Value);
                Assert.That(nonZeroInv.Label, Does.Contain("."), "Invariant labels should use '.' decimal separator.");
                Assert.That(correspondingDe.Label, Does.Contain(","), "German labels should use ',' decimal separator.");
            }
        }

        [Test]
        public void AxisTickLabels_FallBackToToString_WhenFormatterIsNull()
        {
            var registry = UnitsRegistry.Default;
            var unit = Units.Length.Meter;

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });
            var series = new GraphSeriesModel(1, "s", ChartType.Line, xField, yField, xAxis, yAxis);
            var model = new GraphModel(new[] { xAxis, yAxis }, new[] { series });

            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var yAxisPresentation = presentation.Axes.First(a => a.AxisId == "y-axis");
            Assert.That(yAxisPresentation.Ticks.Count, Is.GreaterThan(0));
            Assert.That(yAxisPresentation.Ticks.All(t => t.Label != null), Is.True);
        }

        [Test]
        public void Layout_StackedLeftAxes_IncludeNormalizedGapBetweenAdjacentSpans()
        {
            var presentation = CreatePresentationWithLeftAxisCount(3);
            var leftAxes = presentation.Layout.Axes
                .Where(a => a.Side == PresentationAxisSide.Left)
                .OrderBy(a => a.SideIndex)
                .ToArray();

            Assert.That(leftAxes.Length, Is.EqualTo(3));

            var firstSpan = leftAxes[0].NormalizedSpanEnd - leftAxes[0].NormalizedSpanStart;
            Assert.That(firstSpan, Is.GreaterThan(0d));

            for (var index = 0; index < leftAxes.Length - 1; index++)
            {
                var upper = leftAxes[index];
                var lower = leftAxes[index + 1];
                var gap = upper.NormalizedSpanStart - lower.NormalizedSpanEnd;
                var span = lower.NormalizedSpanEnd - lower.NormalizedSpanStart;

                Assert.That(gap, Is.EqualTo(AxisStackGap).Within(1e-12));
                Assert.That(span, Is.EqualTo(firstSpan).Within(1e-12));
            }

            Assert.That(leftAxes[0].NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12));
            Assert.That(leftAxes[leftAxes.Length - 1].NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12));
        }

        [Test]
        public void Layout_SingleLeftAxis_RemainsFullHeightWithoutGap()
        {
            var presentation = CreatePresentationWithLeftAxisCount(1);
            var leftAxis = presentation.Layout.Axes.Single(a => a.Side == PresentationAxisSide.Left);

            Assert.That(leftAxis.NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12));
            Assert.That(leftAxis.NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12));
        }

        private static IGraphModel CreateModel(ChartType chartType)
        {
            var registry = UnitsRegistry.Default;
            var unit = Units.Length.Meter;
            var formatter = new NumericFormatter("formatter-y", registry, "F2");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatter);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d });

            var series = new GraphSeriesModel(1, "series-1", chartType, xField, yField, xAxis, yAxis);

            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static GraphPresentationModel CreatePresentationWithLeftAxisCount(int leftAxisCount)
        {
            var unit = Units.Length.Meter;

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });

            var axes = new System.Collections.Generic.List<IAxisModel> { xAxis };
            var series = new System.Collections.Generic.List<IGraphSeriesModel>();

            for (var index = 0; index < leftAxisCount; index++)
            {
                var axisId = "y-axis-" + index;
                var yAxis = new AxisModel(new AxisId(axisId), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
                var yValues = new[] { 10d + index, 20d + index, 30d + index };
                var yField = new TestFieldDefinition("Y" + index, "y" + index, unit, yValues);

                axes.Add(yAxis);
                series.Add(new GraphSeriesModel(index + 1, "series-" + index, ChartType.Line, xField, yField, xAxis, yAxis));
            }

            var model = new GraphModel(axes, series);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            return new GraphPresentationModel(snapshot);
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
    }
}
