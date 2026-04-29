using System;
using System.Linq;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
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
        private const double EdgePaddingBandConst = 0.012;
        private const double LegendBandHeightConst = 0.12;
        private const double LegendBandWidthConst = 0.18;

        [Test]
        public void Presentation_UsesExplicitAxisIdentityOrientationAndSide()
        {
            var model = CreateModel(seriesType: SeriesType.Line);

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
        public void SeriesType_ActsAsRenderingIntent_AndDoesNotAffectAxisSelection()
        {
            var lineModel = CreateModel(seriesType: SeriesType.Line);
            var barModel = CreateModel(seriesType: SeriesType.Bar);

            var linePresentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(lineModel));
            var barPresentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(barModel));

            Assert.That(linePresentation.Series[0].SeriesType, Is.EqualTo(SeriesType.Line));
            Assert.That(barPresentation.Series[0].SeriesType, Is.EqualTo(SeriesType.Bar));

            Assert.That(linePresentation.Axes[0].AxisId, Is.EqualTo(barPresentation.Axes[0].AxisId));
            Assert.That(linePresentation.Axes[1].AxisId, Is.EqualTo(barPresentation.Axes[1].AxisId));
            Assert.That(linePresentation.Axes[0].Orientation, Is.EqualTo(barPresentation.Axes[0].Orientation));
            Assert.That(linePresentation.Axes[1].Orientation, Is.EqualTo(barPresentation.Axes[1].Orientation));
        }

        [Test]
        public void AxisFormatter_IsExplicitlySupplied_ByAxisModel()
        {
            var model = CreateModel(seriesType: SeriesType.Line);

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

            var series = new GraphSeriesModel(new SeriesId("1"), "series-1", SeriesType.Line, timeField, valueField, timeAxis, valueAxis);
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
            var model = CreateModel(seriesType: SeriesType.Line);
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

            var series = new GraphSeriesModel(new SeriesId("1"), "series-1", SeriesType.Line, timeField, valueField, timeAxis, valueAxis);
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

            var series = new GraphSeriesModel(new SeriesId("1"), "series-1", SeriesType.Line, xField, yField, xAxis, yAxis);
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

            var seriesF2 = new GraphSeriesModel(new SeriesId("1"), "series-1", SeriesType.Line, xField, yField, xAxis, yAxisF2);
            var modelF2 = new GraphModel(new[] { xAxis, yAxisF2 }, new[] { seriesF2 });
            var presentationF2 = new GraphPresentationModel(builder.Build(modelF2));
            var labelsF2 = presentationF2.Axes.First(a => a.AxisId == "y-axis").Ticks.Select(t => t.Label).ToArray();

            var seriesF6 = new GraphSeriesModel(new SeriesId("1"), "series-1", SeriesType.Line, xField, yField, xAxis, yAxisF6);
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
            var seriesInv = new GraphSeriesModel(new SeriesId("1"), "series-1", SeriesType.Line, xField, yField, xAxis, yAxisInv);
            var modelInv = new GraphModel(new[] { xAxis, yAxisInv }, new[] { seriesInv });
            var presentationInv = new GraphPresentationModel(builder.Build(modelInv));

            var yAxisDe = new AxisModel(yAxisId, ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatterGerman);
            var seriesDe = new GraphSeriesModel(new SeriesId("1"), "series-1", SeriesType.Line, xField, yField, xAxis, yAxisDe);
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
            var series = new GraphSeriesModel(new SeriesId("1"), "series-1", SeriesType.Line, xField, yField, xAxis, yAxis);
            var model = new GraphModel(new[] { xAxis, yAxis }, new[] { series });

            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var yAxisPresentation = presentation.Axes.First(a => a.AxisId == "y-axis");
            Assert.That(yAxisPresentation.Ticks.Count, Is.GreaterThan(0));
            Assert.That(yAxisPresentation.Ticks.All(t => t.Label != null), Is.True);
        }

        [Test]
        public void Presentation_HidesAxis_ByAxisIdOptions()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(hiddenAxisIds: new[] { new AxisId("y-axis") });

            var presentation = new GraphPresentationModel(snapshot, options);

            Assert.That(presentation.Axes.Count, Is.EqualTo(1));
            Assert.That(presentation.Axes.Single().AxisId, Is.EqualTo("x-axis"));
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

        [Test]
        public void Layout_Title_CreatesGeometryWhenPresent()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(graphTitle: "Test Title");

            var presentation = new GraphPresentationModel(snapshot, options);

            Assert.That(presentation.Layout.Title, Is.Not.Null);
            Assert.That(presentation.Layout.Title.Text, Is.EqualTo("Test Title"));
        }

        [Test]
        public void Layout_Title_IsNullWhenAbsent()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions();

            var presentation = new GraphPresentationModel(snapshot, options);

            Assert.That(presentation.Layout.Title, Is.Null);
        }

        [Test]
        public void Layout_Subtitle_CreatesGeometryWhenPresent()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(graphSubtitle: "Test Subtitle");

            var presentation = new GraphPresentationModel(snapshot, options);

            Assert.That(presentation.Layout.Subtitle, Is.Not.Null);
            Assert.That(presentation.Layout.Subtitle.Text, Is.EqualTo("Test Subtitle"));
        }

        [Test]
        public void Layout_Subtitle_IsNullWhenAbsent()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions();

            var presentation = new GraphPresentationModel(snapshot, options);

            Assert.That(presentation.Layout.Subtitle, Is.Null);
        }

        [Test]
        public void Layout_TitleAndSubtitle_BothCreatedWhenPresent()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(graphTitle: "Title", graphSubtitle: "Subtitle");

            var presentation = new GraphPresentationModel(snapshot, options);

            Assert.That(presentation.Layout.Title, Is.Not.Null);
            Assert.That(presentation.Layout.Title.Text, Is.EqualTo("Title"));
            Assert.That(presentation.Layout.Subtitle, Is.Not.Null);
            Assert.That(presentation.Layout.Subtitle.Text, Is.EqualTo("Subtitle"));
        }

        [Test]
        public void Layout_Legend_CreatedWhenVisibleSeriesExist()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentation = new GraphPresentationModel(snapshot);

            Assert.That(presentation.Layout.Legend, Is.Not.Null);
            Assert.That(presentation.Layout.Legend.Entries.Count, Is.EqualTo(1));
            Assert.That(presentation.Layout.Legend.Entries[0].DisplayText, Is.EqualTo("series-1"));
            Assert.That(presentation.Layout.Legend.ShowBorder, Is.True);
        }

        [Test]
        public void Layout_LegendResizeChart_DefaultsToTrue()
        {
            var options = new GraphPresentationOptions();

            Assert.That(options.ResizeChart, Is.True);
        }

        [Test]
        public void Layout_Legend_OmittedWhenNoSeriesVisible()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(hiddenSeriesIds: new[] { new SeriesId("1") });

            var presentation = new GraphPresentationModel(snapshot, options);

            Assert.That(presentation.Layout.Legend, Is.Null);
            Assert.That(presentation.Series.Count, Is.EqualTo(0));
        }

        [Test]
        public void Layout_PlotArea_ReservesSpaceWhenLegendIsPresent()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var withLegend = new GraphPresentationModel(snapshot);
            var withoutLegend = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(hiddenSeriesIds: new[] { new SeriesId("1") }));

            // Legend band is additive with axis band: plot must inset strictly further when legend is present.
            Assert.That(withLegend.Layout.PlotArea.BottomLeft.Y, Is.GreaterThan(withoutLegend.Layout.PlotArea.BottomLeft.Y));
        }

        [Test]
        public void Layout_LegendBounds_AreBelowPlotArea()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.BottomLeft.Y).Within(1e-12));
            Assert.That(legend.BottomLeft.Y, Is.LessThan(legend.TopRight.Y));
        }

        [Test]
        public void Layout_LegendBottom_DoesNotConsumeBottomAxisProtectedBand()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom));
            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.BottomLeft.Y).Within(1e-12),
                "Legend should be outside (below) the bottom axis-title protected band.");
            Assert.That(plotArea.BottomLeft.Y - legend.TopRight.Y, Is.GreaterThan(0d),
                "Gap between plot and legend should remain positive.");
        }

        [Test]
        public void Layout_LegendTop_DoesNotConsumeTopAxisProtectedBand()
        {
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Top, yAxisSide: ModelAxisSide.Left);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Top));
            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.BottomLeft.Y, Is.GreaterThanOrEqualTo(plotArea.TopRight.Y).Within(1e-12),
                "Legend should be outside (above) the top axis-title protected band.");
            Assert.That(legend.BottomLeft.Y - plotArea.TopRight.Y, Is.GreaterThan(0d),
                "Gap between plot and legend should remain positive.");
        }

        [Test]
        public void Layout_LegendLeft_DoesNotConsumeLeftAxisProtectedBand()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Left));
            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.TopRight.X, Is.LessThanOrEqualTo(plotArea.BottomLeft.X).Within(1e-12),
                "Legend should be outside (left of) the left axis-title protected band.");
            Assert.That(plotArea.BottomLeft.X - legend.TopRight.X, Is.GreaterThan(0d),
                "Gap between plot and legend should remain positive.");
        }

        [Test]
        public void Layout_LegendRight_DoesNotConsumeRightAxisProtectedBand()
        {
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Right);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Right));
            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.BottomLeft.X, Is.GreaterThanOrEqualTo(plotArea.TopRight.X).Within(1e-12),
                "Legend should be outside (right of) the right axis-title protected band.");
            Assert.That(legend.BottomLeft.X - plotArea.TopRight.X, Is.GreaterThan(0d),
                "Gap between plot and legend should remain positive.");
        }

        [Test]
        public void Layout_LegendTop_RemainsAboveGraphTitleBand_WhenTitleExists()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(graphTitle: "Title", legendPlacement: LegendPlacement.Top);

            var presentation = new GraphPresentationModel(snapshot, options);
            var legend = presentation.Layout.Legend;
            var title = presentation.Layout.Title;

            Assert.That(legend, Is.Not.Null);
            Assert.That(title, Is.Not.Null);
            Assert.That(legend.BottomLeft.Y, Is.GreaterThanOrEqualTo(title.TopRight.Y).Within(1e-12),
                "Top legend should not overlap title band.");
        }

        [Test]
        public void Layout_Legend_DefaultPlacement_RemainsBottom()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentation = new GraphPresentationModel(snapshot);
            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.BottomLeft.Y).Within(1e-12));
        }

        [TestCase(LegendPlacement.Bottom)]
        [TestCase(LegendPlacement.Top)]
        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void Layout_LegendPlacement_PositionsLegendOnSelectedSide(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(legendPlacement: placement);

            var presentation = new GraphPresentationModel(snapshot, options);
            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);

            switch (placement)
            {
                case LegendPlacement.Bottom:
                    Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.BottomLeft.Y).Within(1e-12));
                    break;

                case LegendPlacement.Top:
                    Assert.That(legend.BottomLeft.Y, Is.GreaterThanOrEqualTo(plotArea.TopRight.Y).Within(1e-12));
                    break;

                case LegendPlacement.Left:
                    Assert.That(legend.TopRight.X, Is.LessThanOrEqualTo(plotArea.BottomLeft.X).Within(1e-12));
                    break;

                case LegendPlacement.Right:
                    Assert.That(legend.BottomLeft.X, Is.GreaterThanOrEqualTo(plotArea.TopRight.X).Within(1e-12));
                    break;
            }
        }

        [Test]
        public void Layout_LegendPlacement_AdjustsPlotAreaBoundsByPlacement()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var baseline = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(hiddenSeriesIds: new[] { new SeriesId("1") }));

            var bottom = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom));
            var top = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Top));
            var left = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Left));
            var right = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Right));

            Assert.That(bottom.Layout.PlotArea.BottomLeft.Y, Is.GreaterThan(baseline.Layout.PlotArea.BottomLeft.Y));
            Assert.That(top.Layout.PlotArea.TopRight.Y, Is.LessThan(baseline.Layout.PlotArea.TopRight.Y));
            Assert.That(left.Layout.PlotArea.BottomLeft.X, Is.GreaterThan(baseline.Layout.PlotArea.BottomLeft.X));
            Assert.That(right.Layout.PlotArea.TopRight.X, Is.LessThan(baseline.Layout.PlotArea.TopRight.X));
        }

        [TestCase(LegendPlacement.Bottom)]
        [TestCase(LegendPlacement.Top)]
        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void Layout_LegendEntryGeometry_RemainsValidForAllPlacements(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(legendPlacement: placement);

            var presentation = new GraphPresentationModel(snapshot, options);
            var legend = presentation.Layout.Legend;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.GreaterThan(0));

            for (var i = 0; i < legend.Entries.Count; i++)
            {
                var entry = legend.Entries[i];
                Assert.That(entry.TopRight.X, Is.GreaterThan(entry.BottomLeft.X));
                Assert.That(entry.TopRight.Y, Is.GreaterThan(entry.BottomLeft.Y));

                Assert.That(entry.BottomLeft.X, Is.GreaterThanOrEqualTo(legend.BottomLeft.X).Within(1e-12));
                Assert.That(entry.TopRight.X, Is.LessThanOrEqualTo(legend.TopRight.X).Within(1e-12));
                Assert.That(entry.BottomLeft.Y, Is.GreaterThanOrEqualTo(legend.BottomLeft.Y).Within(1e-12));
                Assert.That(entry.TopRight.Y, Is.LessThanOrEqualTo(legend.TopRight.Y).Within(1e-12));

                Assert.That(entry.GlyphBottomLeft.X, Is.GreaterThanOrEqualTo(entry.BottomLeft.X).Within(1e-12));
                Assert.That(entry.GlyphTopRight.X, Is.LessThanOrEqualTo(entry.TopRight.X).Within(1e-12));
                Assert.That(entry.GlyphBottomLeft.Y, Is.GreaterThanOrEqualTo(entry.BottomLeft.Y).Within(1e-12));
                Assert.That(entry.GlyphTopRight.Y, Is.LessThanOrEqualTo(entry.TopRight.Y).Within(1e-12));
            }
        }

        [Test]
        public void Layout_LegendPlacement_DoesNotRegressAxisOrSeriesGeometry()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var bottom = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom));
            var top = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Top));
            var left = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Left));
            var right = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: LegendPlacement.Right));

            var presentations = new[] { bottom, top, left, right };
            var baseline = bottom;

            for (var p = 0; p < presentations.Length; p++)
            {
                var presentation = presentations[p];
                Assert.That(presentation.Axes.Count, Is.EqualTo(baseline.Axes.Count));
                Assert.That(presentation.Series.Count, Is.EqualTo(baseline.Series.Count));

                for (var i = 0; i < baseline.Axes.Count; i++)
                {
                    Assert.That(presentation.Axes[i].AxisId, Is.EqualTo(baseline.Axes[i].AxisId));
                    Assert.That(presentation.Axes[i].Orientation, Is.EqualTo(baseline.Axes[i].Orientation));
                    Assert.That(presentation.Axes[i].Side, Is.EqualTo(baseline.Axes[i].Side));
                    Assert.That(presentation.Axes[i].Ticks.Count, Is.EqualTo(baseline.Axes[i].Ticks.Count));
                }

                Assert.That(presentation.Series[0].Points.Count, Is.EqualTo(baseline.Series[0].Points.Count));
                for (var pointIndex = 0; pointIndex < baseline.Series[0].Points.Count; pointIndex++)
                {
                    Assert.That(presentation.Series[0].Points[pointIndex].X, Is.EqualTo(baseline.Series[0].Points[pointIndex].X));
                    Assert.That(presentation.Series[0].Points[pointIndex].Y, Is.EqualTo(baseline.Series[0].Points[pointIndex].Y));
                }
            }
        }

        [TestCase(LegendPlacement.Bottom)]
        [TestCase(LegendPlacement.Top)]
        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void Layout_LegendOverlay_DoesNotResizePlotArea(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var baselineWithoutLegend = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(hiddenSeriesIds: new[] { new SeriesId("1") }, legendPlacement: placement));
            var overlay = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: placement, resizeChart: false));

            Assert.That(overlay.Layout.PlotArea.BottomLeft.X, Is.EqualTo(baselineWithoutLegend.Layout.PlotArea.BottomLeft.X).Within(1e-12));
            Assert.That(overlay.Layout.PlotArea.BottomLeft.Y, Is.EqualTo(baselineWithoutLegend.Layout.PlotArea.BottomLeft.Y).Within(1e-12));
            Assert.That(overlay.Layout.PlotArea.TopRight.X, Is.EqualTo(baselineWithoutLegend.Layout.PlotArea.TopRight.X).Within(1e-12));
            Assert.That(overlay.Layout.PlotArea.TopRight.Y, Is.EqualTo(baselineWithoutLegend.Layout.PlotArea.TopRight.Y).Within(1e-12));
        }

        [TestCase(LegendPlacement.Bottom)]
        [TestCase(LegendPlacement.Top)]
        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void Layout_LegendOverlay_IsPositionedInsidePlotAreaByPlacement(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var overlay = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: placement, resizeChart: false));

            var legend = overlay.Layout.Legend;
            var plotArea = overlay.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.BottomLeft.X, Is.GreaterThanOrEqualTo(plotArea.BottomLeft.X).Within(1e-12));
            Assert.That(legend.TopRight.X, Is.LessThanOrEqualTo(plotArea.TopRight.X).Within(1e-12));
            Assert.That(legend.BottomLeft.Y, Is.GreaterThanOrEqualTo(plotArea.BottomLeft.Y).Within(1e-12));
            Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.TopRight.Y).Within(1e-12));

            switch (placement)
            {
                case LegendPlacement.Bottom:
                    Assert.That(legend.BottomLeft.Y, Is.GreaterThanOrEqualTo(plotArea.BottomLeft.Y).Within(1e-12));
                    Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.BottomLeft.Y + LegendBandHeightConst).Within(1e-12));
                    break;

                case LegendPlacement.Top:
                    Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.TopRight.Y).Within(1e-12));
                    Assert.That(legend.BottomLeft.Y, Is.GreaterThanOrEqualTo(plotArea.TopRight.Y - LegendBandHeightConst).Within(1e-12));
                    break;

                case LegendPlacement.Left:
                    Assert.That(legend.BottomLeft.X, Is.GreaterThanOrEqualTo(plotArea.BottomLeft.X).Within(1e-12));
                    Assert.That(legend.TopRight.X, Is.LessThanOrEqualTo(plotArea.BottomLeft.X + LegendBandWidthConst).Within(1e-12));
                    break;

                case LegendPlacement.Right:
                    Assert.That(legend.TopRight.X, Is.LessThanOrEqualTo(plotArea.TopRight.X).Within(1e-12));
                    Assert.That(legend.BottomLeft.X, Is.GreaterThanOrEqualTo(plotArea.TopRight.X - LegendBandWidthConst).Within(1e-12));
                    break;
            }
        }

        [Test]
        public void Layout_LegendOverlay_RespectsAxisTitleProtectedSpace_Bottom()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var overlay = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom, resizeChart: false));

            var legend = overlay.Layout.Legend;
            var plotArea = overlay.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.BottomLeft.Y, Is.GreaterThanOrEqualTo(plotArea.BottomLeft.Y).Within(1e-12),
                "Overlay legend should remain inside plot and outside protected bottom axis-title band.");
        }

        [Test]
        public void Layout_LegendOverlay_RespectsAxisTitleProtectedSpace_Top()
        {
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Top, yAxisSide: ModelAxisSide.Left);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var overlay = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Top, resizeChart: false));

            var legend = overlay.Layout.Legend;
            var plotArea = overlay.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.TopRight.Y).Within(1e-12),
                "Overlay legend should remain inside plot and outside protected top axis-title band.");
        }

        [Test]
        public void Layout_LegendOverlay_RespectsAxisTitleProtectedSpace_Left()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var overlay = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Left, resizeChart: false));

            var legend = overlay.Layout.Legend;
            var plotArea = overlay.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.BottomLeft.X, Is.GreaterThanOrEqualTo(plotArea.BottomLeft.X).Within(1e-12),
                "Overlay legend should remain inside plot and outside protected left axis-title band.");
        }

        [Test]
        public void Layout_LegendOverlay_RespectsAxisTitleProtectedSpace_Right()
        {
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Right);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var overlay = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Right, resizeChart: false));

            var legend = overlay.Layout.Legend;
            var plotArea = overlay.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.TopRight.X, Is.LessThanOrEqualTo(plotArea.TopRight.X).Within(1e-12),
                "Overlay legend should remain inside plot and outside protected right axis-title band.");
        }

        [Test]
        public void Layout_LegendOverlay_DoesNotAffectAxisGridOrSeriesGeometry()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var resizeLayout = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Right, resizeChart: true));
            var overlayLayout = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Right, resizeChart: false));

            Assert.That(overlayLayout.Axes.Count, Is.EqualTo(resizeLayout.Axes.Count));
            Assert.That(overlayLayout.Series.Count, Is.EqualTo(resizeLayout.Series.Count));
            Assert.That(overlayLayout.Layout.GridLines.VerticalLines.Count, Is.EqualTo(resizeLayout.Layout.GridLines.VerticalLines.Count));
            Assert.That(overlayLayout.Layout.GridLines.HorizontalLines.Count, Is.EqualTo(resizeLayout.Layout.GridLines.HorizontalLines.Count));

            for (var i = 0; i < resizeLayout.Axes.Count; i++)
            {
                Assert.That(overlayLayout.Axes[i].AxisId, Is.EqualTo(resizeLayout.Axes[i].AxisId));
                Assert.That(overlayLayout.Axes[i].Orientation, Is.EqualTo(resizeLayout.Axes[i].Orientation));
                Assert.That(overlayLayout.Axes[i].Side, Is.EqualTo(resizeLayout.Axes[i].Side));
                Assert.That(overlayLayout.Axes[i].Ticks.Count, Is.EqualTo(resizeLayout.Axes[i].Ticks.Count));
            }

            for (var seriesIndex = 0; seriesIndex < resizeLayout.Series.Count; seriesIndex++)
            {
                Assert.That(overlayLayout.Series[seriesIndex].Points.Count, Is.EqualTo(resizeLayout.Series[seriesIndex].Points.Count));
                for (var pointIndex = 0; pointIndex < resizeLayout.Series[seriesIndex].Points.Count; pointIndex++)
                {
                    Assert.That(overlayLayout.Series[seriesIndex].Points[pointIndex].X, Is.EqualTo(resizeLayout.Series[seriesIndex].Points[pointIndex].X));
                    Assert.That(overlayLayout.Series[seriesIndex].Points[pointIndex].Y, Is.EqualTo(resizeLayout.Series[seriesIndex].Points[pointIndex].Y));
                }
            }
        }

        [Test]
        public void Layout_LegendReservation_DoesNotAffectAxisOrSeriesGeometryCorrectness()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var withLegend = new GraphPresentationModel(snapshot);
            var withoutLegend = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(hiddenSeriesIds: new[] { new SeriesId("1") }));

            Assert.That(withLegend.Axes.Count, Is.EqualTo(withoutLegend.Axes.Count));

            for (var i = 0; i < withLegend.Axes.Count; i++)
            {
                Assert.That(withLegend.Axes[i].AxisId, Is.EqualTo(withoutLegend.Axes[i].AxisId));
                Assert.That(withLegend.Axes[i].Orientation, Is.EqualTo(withoutLegend.Axes[i].Orientation));
                Assert.That(withLegend.Axes[i].Side, Is.EqualTo(withoutLegend.Axes[i].Side));
                Assert.That(withLegend.Axes[i].Ticks.Count, Is.EqualTo(withoutLegend.Axes[i].Ticks.Count));
            }

            Assert.That(withLegend.Series.Count, Is.EqualTo(1));
            Assert.That(withLegend.Series[0].Points.Count, Is.EqualTo(3));
            Assert.That(withLegend.Series[0].Points[0].X, Is.EqualTo(0d));
            Assert.That(withLegend.Series[0].Points[0].Y, Is.EqualTo(10d));
            Assert.That(withLegend.Series[0].Points[2].X, Is.EqualTo(2d));
            Assert.That(withLegend.Series[0].Points[2].Y, Is.EqualTo(30d));
        }

        [Test]
        public void Layout_PlotArea_ShiftsDownWhenTitleExists()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentationWithoutTitle = new GraphPresentationModel(snapshot);
            var presentationWithTitle = new GraphPresentationModel(snapshot, new GraphPresentationOptions(graphTitle: "Test Title"));

            Assert.That(presentationWithTitle.Layout.PlotArea.TopRight.Y, Is.LessThan(presentationWithoutTitle.Layout.PlotArea.TopRight.Y));
        }

        [Test]
        public void Layout_PlotArea_ShiftsDownMoreWhenBothTitleAndSubtitleExist()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentationWithoutTitle = new GraphPresentationModel(snapshot);
            var presentationWithTitle = new GraphPresentationModel(snapshot, new GraphPresentationOptions(graphTitle: "Test Title"));
            var presentationWithBoth = new GraphPresentationModel(snapshot, new GraphPresentationOptions(graphTitle: "Test Title", graphSubtitle: "Test Subtitle"));

            Assert.That(presentationWithBoth.Layout.PlotArea.TopRight.Y, Is.LessThan(presentationWithTitle.Layout.PlotArea.TopRight.Y));
            Assert.That(presentationWithTitle.Layout.PlotArea.TopRight.Y, Is.LessThan(presentationWithoutTitle.Layout.PlotArea.TopRight.Y));
        }

        [Test]
        public void Layout_TitleBounds_SpanFullWidthAndLocateAbovePlotArea()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(graphTitle: "Test Title");

            var presentation = new GraphPresentationModel(snapshot, options);
            var title = presentation.Layout.Title;

            Assert.That(title.BottomLeft.X, Is.EqualTo(EdgePaddingBandConst).Within(1e-12));
            Assert.That(title.TopRight.X, Is.EqualTo(1d - EdgePaddingBandConst).Within(1e-12));
            Assert.That(title.BottomLeft.Y, Is.GreaterThan(presentation.Layout.PlotArea.TopRight.Y));
            Assert.That(title.TopRight.Y, Is.GreaterThan(title.BottomLeft.Y));
        }

        [Test]
        public void Layout_SubtitleBounds_LocateBetweenTitleAndPlotArea()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(graphTitle: "Test Title", graphSubtitle: "Test Subtitle");

            var presentation = new GraphPresentationModel(snapshot, options);
            var title = presentation.Layout.Title;
            var subtitle = presentation.Layout.Subtitle;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(subtitle.TopRight.Y, Is.LessThan(title.BottomLeft.Y), "Subtitle should be below title");
            Assert.That(subtitle.BottomLeft.Y, Is.EqualTo(plotArea.TopRight.Y).Within(1e-12).Or.GreaterThan(plotArea.TopRight.Y), "Subtitle should align with or be above plot area");
        }

        [Test]
        public void Layout_PlotArea_IsAlwaysPresent()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            Assert.That(presentation.Layout.PlotArea, Is.Not.Null);
        }

        [Test]
        public void Layout_PlotArea_HasExpectedBoundsWithStandardAxes()
        {
            // A graph with one bottom (X) axis and one left (Y) axis should reserve:
            // AxisSlotSize on the left; AxisSlotSize + content-driven legend height on the bottom.
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var plotArea = presentation.Layout.PlotArea;
            var legend = presentation.Layout.Legend;

            Assert.That(plotArea.BottomLeft.X, Is.GreaterThan(0d), "Left margin should reserve a positive protected band.");
            Assert.That(plotArea.BottomLeft.Y, Is.GreaterThan(0d), "Bottom margin should include axis and legend reservations.");
            Assert.That(legend, Is.Not.Null, "Legend should be present with a visible series.");
            Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.BottomLeft.Y).Within(1e-12), "Legend should sit below the plot area.");
            Assert.That(plotArea.TopRight.X, Is.EqualTo(1d - EdgePaddingBandConst).Within(1e-12), "Right edge should reserve fixed edge padding.");
            Assert.That(plotArea.TopRight.Y, Is.EqualTo(1d - EdgePaddingBandConst).Within(1e-12), "Top edge should reserve fixed edge padding.");
        }

        [Test]
        public void Layout_PlotArea_ExpandsWhenLeftAxisIsHidden()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var optionsVisible = new GraphPresentationOptions();
            var optionsHidden = new GraphPresentationOptions(hiddenAxisIds: new[] { new AxisId("y-axis") });

            var presentationVisible = new GraphPresentationModel(snapshot, optionsVisible);
            var presentationHidden = new GraphPresentationModel(snapshot, optionsHidden);

            // Hiding the left axis should remove axis withdrawal while preserving fixed edge padding.
            Assert.That(presentationHidden.Layout.PlotArea.BottomLeft.X,
                Is.LessThan(presentationVisible.Layout.PlotArea.BottomLeft.X),
                "Left plot area bound should move left when the left axis is hidden.");
            Assert.That(presentationHidden.Layout.PlotArea.BottomLeft.X, Is.EqualTo(EdgePaddingBandConst).Within(1e-12));
        }

        [Test]
        public void Layout_PlotArea_BoundsUnchangedWithStackedLeftAxes()
        {
            // Stacked left axes affect internal axis span layout but should NOT change
                // the horizontal plot area margin (a single AxisSlotSize is always used on the left).
                // Note: vertical bounds legitimately differ when axis count changes, because
                // CreatePresentationWithLeftAxisCount adds one series per axis, and legend height
                // is content-driven (more series → taller band). Only X bounds are invariant here.
            var presentationSingle = CreatePresentationWithLeftAxisCount(1);
            var presentationStacked = CreatePresentationWithLeftAxisCount(3);

            var singlePlotArea = presentationSingle.Layout.PlotArea;
            var stackedPlotArea = presentationStacked.Layout.PlotArea;

            Assert.That(stackedPlotArea.BottomLeft.X, Is.EqualTo(singlePlotArea.BottomLeft.X).Within(1e-12),
                "Left margin should be the same regardless of how many left axes are stacked.");
            Assert.That(stackedPlotArea.TopRight.X, Is.EqualTo(singlePlotArea.TopRight.X).Within(1e-12));
                Assert.That(stackedPlotArea.TopRight.Y, Is.EqualTo(singlePlotArea.TopRight.Y).Within(1e-12),
                    "Top margin should be unaffected by left-axis stacking.");
        }

        [Test]
        public void Layout_PlotArea_BoundsConsistentWithAxisLayoutSpans()
        {
            // Axis NormalizedSpanStart/End are within [0,1] relative to the plot area height.
            // The plot area itself is defined by PlotAreaLayout. This test verifies that
            // the axis spans are valid within that context.
            var presentation = CreatePresentationWithLeftAxisCount(3);
            var leftAxes = presentation.Layout.Axes
                .Where(a => a.Side == PresentationAxisSide.Left)
                .ToArray();

            Assert.That(leftAxes.Length, Is.EqualTo(3));

            foreach (var entry in leftAxes)
            {
                Assert.That(entry.NormalizedSpanStart, Is.GreaterThanOrEqualTo(0d).Within(1e-12),
                    "Axis span start must be within plot area [0,1] range.");
                Assert.That(entry.NormalizedSpanEnd, Is.LessThanOrEqualTo(1d).Within(1e-12),
                    "Axis span end must be within plot area [0,1] range.");
                Assert.That(entry.NormalizedSpanStart, Is.LessThan(entry.NormalizedSpanEnd),
                    "Axis span must have positive height.");
            }
        }

        [Test]
        public void Layout_GridLines_AreAlwaysPresent()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            Assert.That(presentation.Layout.GridLines, Is.Not.Null);
        }

        [Test]
        public void Layout_GridLines_HasVerticalLinesFromXAxisTicks()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var verticalLines = presentation.Layout.GridLines.VerticalLines;
            Assert.That(verticalLines.Count, Is.GreaterThan(0),
                "Should have vertical grid lines from X-axis ticks");

            var yEntries = presentation.Layout.Axes
                .Where(a => a.Axis.Orientation == PresentationAxisOrientation.Vertical)
                .ToArray();
            var expectedMinY = yEntries.Min(e => e.Axis.MinimumValue.Value);
            var expectedMaxY = yEntries.Max(e => e.Axis.MaximumValue.Value);

            for (var i = 0; i < verticalLines.Count; i++)
            {
                var line = verticalLines[i];
                Assert.That(line.Orientation, Is.EqualTo(PresentationAxisOrientation.Vertical),
                    "Lines should be vertical");
                Assert.That(line.Start.Y, Is.EqualTo(expectedMinY).Within(1e-12),
                    "Vertical grid line should start at Y-domain minimum.");
                Assert.That(line.End.Y, Is.EqualTo(expectedMaxY).Within(1e-12),
                    "Vertical grid line should end at Y-domain maximum.");
            }
        }

        [Test]
        public void Layout_GridLines_HasHorizontalLinesFromYAxisTicks()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;
            Assert.That(horizontalLines.Count, Is.GreaterThan(0),
                "Should have horizontal grid lines from Y-axis ticks");

            var xEntries = presentation.Layout.Axes
                .Where(a => a.Axis.Orientation == PresentationAxisOrientation.Horizontal)
                .ToArray();
            var expectedMinX = xEntries.Min(e => e.Axis.MinimumValue.Value);
            var expectedMaxX = xEntries.Max(e => e.Axis.MaximumValue.Value);

            for (var i = 0; i < horizontalLines.Count; i++)
            {
                var line = horizontalLines[i];
                Assert.That(line.Orientation, Is.EqualTo(PresentationAxisOrientation.Horizontal),
                    "Lines should be horizontal");
                Assert.That(line.Start.X, Is.EqualTo(expectedMinX).Within(1e-12),
                    "Horizontal grid line should start at X-domain minimum.");
                Assert.That(line.End.X, Is.EqualTo(expectedMaxX).Within(1e-12),
                    "Horizontal grid line should end at X-domain maximum.");
            }
        }

        [Test]
        public void Layout_GridLines_AlignWithTickPositions()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var xTickValues = presentation.Layout.Axes
                .Where(a => a.Axis.Orientation == PresentationAxisOrientation.Horizontal)
                .SelectMany(a => a.Axis.Ticks.Select(t => t.Value))
                .ToArray();
            var yTickValues = presentation.Layout.Axes
                .Where(a => a.Axis.Orientation == PresentationAxisOrientation.Vertical)
                .SelectMany(a => a.Axis.Ticks.Select(t => t.Value))
                .ToArray();

            var verticalLines = presentation.Layout.GridLines.VerticalLines;
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;

            for (var i = 0; i < verticalLines.Count; i++)
            {
                var line = verticalLines[i];
                Assert.That(xTickValues.Any(v => Math.Abs(v - line.Start.X) <= 1e-12), Is.True,
                    "Vertical line X must match an X-axis tick domain value.");
            }

            for (var i = 0; i < horizontalLines.Count; i++)
            {
                var line = horizontalLines[i];
                Assert.That(yTickValues.Any(v => Math.Abs(v - line.Start.Y) <= 1e-12), Is.True,
                    "Horizontal line Y must match a Y-axis tick domain value.");
            }
        }

        [Test]
        public void Layout_GridLines_StayWithinAxisDomainExtents()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var xEntries = presentation.Layout.Axes
                .Where(a => a.Axis.Orientation == PresentationAxisOrientation.Horizontal)
                .ToArray();
            var yEntries = presentation.Layout.Axes
                .Where(a => a.Axis.Orientation == PresentationAxisOrientation.Vertical)
                .ToArray();

            var minX = xEntries.Min(e => e.Axis.MinimumValue.Value);
            var maxX = xEntries.Max(e => e.Axis.MaximumValue.Value);
            var minY = yEntries.Min(e => e.Axis.MinimumValue.Value);
            var maxY = yEntries.Max(e => e.Axis.MaximumValue.Value);

            var verticalLines = presentation.Layout.GridLines.VerticalLines;
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;

            for (var i = 0; i < verticalLines.Count; i++)
            {
                var line = verticalLines[i];
                Assert.That(line.Start.X, Is.GreaterThanOrEqualTo(minX).Within(1e-12));
                Assert.That(line.Start.X, Is.LessThanOrEqualTo(maxX).Within(1e-12));
                Assert.That(line.Start.Y, Is.EqualTo(minY).Within(1e-12));
                Assert.That(line.End.Y, Is.EqualTo(maxY).Within(1e-12));
            }

            for (var i = 0; i < horizontalLines.Count; i++)
            {
                var line = horizontalLines[i];
                Assert.That(line.Start.Y, Is.GreaterThanOrEqualTo(minY).Within(1e-12));
                Assert.That(line.Start.Y, Is.LessThanOrEqualTo(maxY).Within(1e-12));
                Assert.That(line.Start.X, Is.EqualTo(minX).Within(1e-12));
                Assert.That(line.End.X, Is.EqualTo(maxX).Within(1e-12));
            }
        }

        [Test]
        public void GridLines_AreDomainRelative_NotPlotNormalized()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-domain"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-domain"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 10d, 20d, 30d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 100d, 200d, 300d });
            var series = new GraphSeriesModel(new SeriesId("1"), "s", SeriesType.Line, xField, yField, xAxis, yAxis);
            var model = new GraphModel(new[] { xAxis, yAxis }, new[] { series });

            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model));
            var verticalLines = presentation.Layout.GridLines.VerticalLines;

            Assert.That(verticalLines.Count, Is.GreaterThan(0));

            var xTicks = presentation.Layout.Axes
                .Where(a => a.Axis.Orientation == PresentationAxisOrientation.Horizontal)
                .SelectMany(a => a.Axis.Ticks.Select(t => t.Value))
                .ToArray();

            for (var i = 0; i < verticalLines.Count; i++)
            {
                var x = verticalLines[i].Start.X;
                Assert.That(xTicks.Any(v => Math.Abs(v - x) <= 1e-12), Is.True,
                    "Vertical grid line must use domain tick values, not plot-normalized coordinates.");
            }

            // Domain is [10,30], so normalized [0,1] values are invalid except if equal to tick values by coincidence.
            Assert.That(verticalLines.Any(l => l.Start.X > 1d), Is.True,
                "At least one vertical grid line should prove domain-space emission for non-[0,1] domains.");
        }

        [Test]
        public void Layout_GridLines_OmittedWhenYAxisIsHidden()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentationWithAxis = new GraphPresentationModel(snapshot);
            var presentationWithoutAxis = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(hiddenAxisIds: new[] { new AxisId("y-axis") }));

            Assert.That(presentationWithAxis.Layout.GridLines.HorizontalLines.Count, Is.GreaterThan(0));
            Assert.That(presentationWithoutAxis.Layout.GridLines.HorizontalLines.Count, Is.EqualTo(0),
                "Horizontal grid lines should be omitted when Y-axis is hidden");
        }

        [Test]
        public void Layout_GridLines_StackedYAxes_ProduceAxisScopedHorizontalLines()
        {
            var presentation = CreatePresentationWithLeftAxisCount(2);
            var leftAxisEntries = presentation.Layout.Axes
                .Where(a => a.Side == PresentationAxisSide.Left)
                .OrderBy(a => a.SideIndex)
                .ToArray();

            Assert.That(leftAxisEntries.Length, Is.EqualTo(2));

            var expectedHorizontalCount = leftAxisEntries.Sum(entry => entry.Axis.Ticks.Count);
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;
            Assert.That(horizontalLines.Count, Is.EqualTo(expectedHorizontalCount),
                "Each stacked Y-axis should contribute its own horizontal grid lines.");

            for (var axisIndex = 0; axisIndex < leftAxisEntries.Length; axisIndex++)
            {
                var entry = leftAxisEntries[axisIndex];
                for (var tickIndex = 0; tickIndex < entry.Axis.Ticks.Count; tickIndex++)
                {
                    var tick = entry.Axis.Ticks[tickIndex];
                    var hasMatch = horizontalLines.Any(line =>
                        Math.Abs(line.Start.Y - tick.Value) <= 1e-12 &&
                        Math.Abs(line.End.Y - tick.Value) <= 1e-12);

                    Assert.That(hasMatch, Is.True,
                        "Horizontal line should align to the source axis tick domain value.");
                }
            }
        }

        [Test]
        public void Layout_GridLines_StackedYAxes_HorizontalLinesStayWithinAxisDomains()
        {
            var presentation = CreatePresentationWithLeftAxisCount(3);
            var leftAxisEntries = presentation.Layout.Axes
                .Where(a => a.Side == PresentationAxisSide.Left)
                .OrderBy(a => a.SideIndex)
                .ToArray();
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;

            Assert.That(horizontalLines.Count, Is.GreaterThan(0));

            for (var i = 0; i < horizontalLines.Count; i++)
            {
                var y = horizontalLines[i].Start.Y;
                var coveredBySomeAxisDomain = leftAxisEntries.Any(entry =>
                    y >= entry.Axis.MinimumValue.Value - 1e-12 &&
                    y <= entry.Axis.MaximumValue.Value + 1e-12);

                Assert.That(coveredBySomeAxisDomain, Is.True,
                    "Horizontal grid line should stay within at least one owning Y-axis domain.");
            }
        }

        [Test]
        public void Layout_GridLines_SingleYAxisBehavior_RemainsUnchanged()
        {
            var presentation = CreatePresentationWithLeftAxisCount(1);
            var leftEntry = presentation.Layout.Axes.Single(a => a.Side == PresentationAxisSide.Left);
            var xEntry = presentation.Layout.Axes.Single(a => a.Axis.Orientation == PresentationAxisOrientation.Horizontal);
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;

            Assert.That(horizontalLines.Count, Is.EqualTo(leftEntry.Axis.Ticks.Count));

            for (var i = 0; i < horizontalLines.Count; i++)
            {
                Assert.That(horizontalLines[i].Start.X, Is.EqualTo(xEntry.Axis.MinimumValue.Value).Within(1e-12));
                Assert.That(horizontalLines[i].End.X, Is.EqualTo(xEntry.Axis.MaximumValue.Value).Within(1e-12));
                Assert.That(horizontalLines[i].Start.Y, Is.GreaterThanOrEqualTo(leftEntry.Axis.MinimumValue.Value).Within(1e-12));
                Assert.That(horizontalLines[i].Start.Y, Is.LessThanOrEqualTo(leftEntry.Axis.MaximumValue.Value).Within(1e-12));
            }
        }

        [Test]
        public void GridLines_UseCorrectAxisLayoutEntry()
        {
            var presentation = CreatePresentationWithLeftAxisCount(2);
            var xAxis = presentation.Layout.Axes.First(a => a.Axis.Orientation == PresentationAxisOrientation.Horizontal);
            var yAxes = presentation.Layout.Axes.Where(a => a.Axis.Orientation == PresentationAxisOrientation.Vertical).ToArray();

            var verticalLines = presentation.Layout.GridLines.VerticalLines;
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;

            Assert.That(verticalLines.Count, Is.GreaterThan(0), "Should have vertical grid lines.");
            for (var i = 0; i < verticalLines.Count; i++)
            {
                var line = verticalLines[i];
                Assert.That(line.AxisEntry, Is.Not.Null, "Vertical grid line must bind to an axis entry.");
                Assert.That(line.AxisEntry.Axis.Orientation, Is.EqualTo(PresentationAxisOrientation.Horizontal),
                    "Vertical grid line must bind to an X-axis entry.");
            }

            Assert.That(horizontalLines.Count, Is.GreaterThan(0), "Should have horizontal grid lines.");
            for (var i = 0; i < horizontalLines.Count; i++)
            {
                var line = horizontalLines[i];
                Assert.That(line.AxisEntry, Is.Not.Null, "Horizontal grid line must bind to an axis entry.");
                Assert.That(line.AxisEntry.Axis.Orientation, Is.EqualTo(PresentationAxisOrientation.Vertical),
                    "Horizontal grid line must bind to a Y-axis entry.");
                var boundAxisId = line.AxisEntry.Axis.AxisId;
                Assert.That(yAxes.Any(a => a.Axis.AxisId == boundAxisId), Is.True,
                    "Horizontal grid line must bind to one of the source Y-axis entries.");
            }
        }

        // ── Phase L2b: Left/Right resize-mode legend placement respects axis-title protected band ──

        [Test]
        public void Layout_LegendLeft_ResizeMode_PlotAreaIncludesBothAxisSlotAndLegendBand()
        {
            // With a left axis and left legend (resize mode), layout order is:
            //   legend band [0, bandWidth] | axis-title band | plot area
            // Band width is content-driven; assert structural constraints only.
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Left, resizeChart: true));

            var plotArea = presentation.Layout.PlotArea;
            var legend = presentation.Layout.Legend;

            Assert.That(plotArea.BottomLeft.X, Is.GreaterThan(0d),
                "Plot area left edge should reserve both axis-title band and content-driven legend band.");
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.TopRight.X, Is.LessThanOrEqualTo(plotArea.BottomLeft.X).Within(1e-12),
                "Legend container must fit within the reserved band (outside the axis-title zone).");
            Assert.That(plotArea.BottomLeft.X - legend.TopRight.X, Is.GreaterThan(0d),
                "A non-zero axis protected band must remain between legend and plot.");
        }

        [Test]
        public void Layout_LegendLeft_ResizeMode_LegendBandIsFullWidth_NotTruncatedByAxisSlot()
        {
            // The legend band must span the full LegendBandWidth, not be clipped to AxisSlotSize.
            // legend container: [LegendOuterPaddingX, LegendBandWidth - LegendOuterPaddingX]
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Left, resizeChart: true));

            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);

            // The outer legend band ends at plotLeft - AxisSlotSize = LegendBandWidth.
            // The container right edge = band right - LegendOuterPaddingX, which must be
            // strictly less than the start of the axis-title zone (plotLeft - AxisSlotSize).
            Assert.That(legend.TopRight.X, Is.LessThan(plotArea.BottomLeft.X).Within(1e-12),
                "Legend container right edge must not reach into the Y-axis title protected zone.");

            // The legend band must be the full LegendBandWidth wide (container spans most of it).
            // Container width = bandWidth - 2*outerPadding. Assert it is positive and substantial.
            Assert.That(legend.TopRight.X, Is.GreaterThan(legend.BottomLeft.X),
                "Legend container must have positive width.");
            Assert.That(legend.TopRight.X - legend.BottomLeft.X,
                Is.GreaterThan(LegendBandWidthConst * 0.5),
                "Legend container should span most of the legend band width.");
        }

        [Test]
        public void Layout_LegendLeft_ResizeMode_LegendBandDoesNotOverlapAxisTitleZone()
        {
            // The legend band [0, legendBandRight] must not overlap the axis-title zone
            // [plotLeft - axisSlot, plotLeft]. They must be strictly adjacent: legendBandRight == plotLeft - axisSlot.
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Left, resizeChart: true));

            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);

            // axis-title zone occupies [plotLeft - AxisSlotSize, plotLeft].
            // Legend band occupies [0, plotLeft - AxisSlotSize]. No overlap.
            Assert.That(legend.TopRight.X, Is.LessThanOrEqualTo(plotArea.BottomLeft.X).Within(1e-12),
                "The legend band right edge must not exceed the left boundary of the Y-axis title protected zone.");
        }

        [Test]
        public void Layout_LegendRight_ResizeMode_PlotAreaIncludesBothAxisSlotAndLegendBand()
        {
            // With a right axis and right legend (resize mode), layout order is:
            //   plot area | axis-title band | legend band [axisEnd, 1.0]
            // Band width is content-driven; assert structural constraints only.
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Right);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Right, resizeChart: true));

            var plotArea = presentation.Layout.PlotArea;
            var legend = presentation.Layout.Legend;

            Assert.That(plotArea.TopRight.X, Is.LessThan(1.0),
                "Plot area right edge should reserve both axis-title band and content-driven legend band.");
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.BottomLeft.X, Is.GreaterThanOrEqualTo(plotArea.TopRight.X).Within(1e-12),
                "Legend container must fit within the reserved band (outside the axis-title zone).");
            Assert.That(legend.BottomLeft.X - plotArea.TopRight.X, Is.GreaterThan(0d),
                "A non-zero axis protected band must remain between plot and legend.");
        }

        [Test]
        public void Layout_LegendRight_ResizeMode_LegendBandDoesNotOverlapAxisTitleZone()
        {
            // The legend band [legendBandLeft, 1.0] must not overlap the axis-title zone
            // [plotRight, plotRight + axisSlot]. They must be strictly adjacent.
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Right);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Right, resizeChart: true));

            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);

            // axis-title zone occupies [plotRight, plotRight + AxisSlotSize].
            // Legend band occupies [plotRight + AxisSlotSize, 1.0]. No overlap.
            Assert.That(legend.BottomLeft.X, Is.GreaterThanOrEqualTo(plotArea.TopRight.X).Within(1e-12),
                "The legend band left edge must not intrude into the Y-axis title protected zone.");
        }

        [Test]
        public void Layout_LegendRight_ResizeMode_LegendBandIsFullWidth_NotTruncatedByAxisSlot()
        {
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Right);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Right, resizeChart: true));

            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);

            Assert.That(legend.BottomLeft.X, Is.GreaterThan(plotArea.TopRight.X).Within(1e-12),
                "Legend container left edge must not reach back into the Y-axis title protected zone.");

            Assert.That(legend.TopRight.X, Is.GreaterThan(legend.BottomLeft.X),
                "Legend container must have positive width.");
            Assert.That(legend.TopRight.X - legend.BottomLeft.X,
                Is.GreaterThan(LegendBandWidthConst * 0.5),
                "Legend container should span most of the legend band width.");
        }

        [Test]
        public void Layout_LegendBottom_ResizeMode_IsUnchangedByL2bFix()
        {
            // Bottom placement: legend band below axis-title slot; band height is content-driven.
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom, resizeChart: true));

            var plotArea = presentation.Layout.PlotArea;
            var legend = presentation.Layout.Legend;

            Assert.That(plotArea.BottomLeft.Y, Is.GreaterThan(0d),
                "Bottom plot edge should reserve axis-title band + content-driven legend band.");
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.TopRight.Y, Is.LessThanOrEqualTo(plotArea.BottomLeft.Y).Within(1e-12),
                "Legend must sit below the plot area bottom.");
            Assert.That(plotArea.BottomLeft.Y - legend.TopRight.Y, Is.GreaterThan(0.0),
                "Legend band must have positive height.");
        }

        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void Layout_LegendOverlay_IsUnaffectedByL2bFix(LegendPlacement placement)
        {
            // Overlay mode (resizeChart=false) must not be changed by L2b.
            // Legend should remain inside the plot area.
            IGraphModel model;
            if (placement == LegendPlacement.Right)
            {
                model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Right);
            }
            else
            {
                model = CreateModel(seriesType: SeriesType.Line);
            }

            var snapshot = new GraphSnapshotBuilder().Build(model);
            var overlay = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: placement, resizeChart: false));
            var resize = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(legendPlacement: placement, resizeChart: true));

            // Overlay plot area must be larger (no shrinkage from legend)
            if (placement == LegendPlacement.Left)
            {
                Assert.That(overlay.Layout.PlotArea.BottomLeft.X,
                    Is.LessThan(resize.Layout.PlotArea.BottomLeft.X),
                    "Overlay must not shrink the plot area on the left.");
            }
            else
            {
                Assert.That(overlay.Layout.PlotArea.TopRight.X,
                    Is.GreaterThan(resize.Layout.PlotArea.TopRight.X),
                    "Overlay must not shrink the plot area on the right.");
            }

            // Overlay legend must remain inside plot area
            var legend = overlay.Layout.Legend;
            var plotArea = overlay.Layout.PlotArea;
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.BottomLeft.X, Is.GreaterThanOrEqualTo(plotArea.BottomLeft.X).Within(1e-12));
            Assert.That(legend.TopRight.X, Is.LessThanOrEqualTo(plotArea.TopRight.X).Within(1e-12));
        }

        // ── Phase L4: Content-driven legend sizing and series color ──────────
        
        [Test]
        public void Layout_LegendBandWidth_GrowsWithLongerSeriesLabel()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });
            var opts = new GraphPresentationOptions(legendPlacement: LegendPlacement.Left);
            
            var shortSeries = new GraphSeriesModel(new SeriesId("s1"), "A", SeriesType.Line, xField, yField, xAxis, yAxis);
            var shortPresentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(new GraphModel(new[] { xAxis, yAxis }, new[] { shortSeries })), opts);
            
            var longSeries = new GraphSeriesModel(new SeriesId("s2"), "A Very Long Series Label Name", SeriesType.Line, xField, yField, xAxis, yAxis);
            var longPresentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(new GraphModel(new[] { xAxis, yAxis }, new[] { longSeries })), opts);
            
            var shortBandWidth = shortPresentation.Layout.Legend.TopRight.X - shortPresentation.Layout.Legend.BottomLeft.X;
            var longBandWidth = longPresentation.Layout.Legend.TopRight.X - longPresentation.Layout.Legend.BottomLeft.X;
            
            Assert.That(longBandWidth, Is.GreaterThan(shortBandWidth),
                "A longer series label should produce a wider legend band.");
        }
        
        [Test]
        public void Layout_LegendBandHeight_GrowsWithMoreEntries()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });
            var opts = new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom);
            
            var oneSeries = new GraphSeriesModel(new SeriesId("1"), "S", SeriesType.Line, xField, yField, xAxis, yAxis);
            var onePresentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(new GraphModel(new[] { xAxis, yAxis }, new[] { oneSeries })), opts);
            
            var s1 = new GraphSeriesModel(new SeriesId("1"), "LongLabel-001", SeriesType.Line, xField, yField, xAxis, yAxis);
            var s2 = new GraphSeriesModel(new SeriesId("2"), "LongLabel-002", SeriesType.Line, xField, yField, xAxis, yAxis);
            var s3 = new GraphSeriesModel(new SeriesId("3"), "LongLabel-003", SeriesType.Line, xField, yField, xAxis, yAxis);
            var s4 = new GraphSeriesModel(new SeriesId("4"), "LongLabel-004", SeriesType.Line, xField, yField, xAxis, yAxis);
            var s5 = new GraphSeriesModel(new SeriesId("5"), "LongLabel-005", SeriesType.Line, xField, yField, xAxis, yAxis);
            var s6 = new GraphSeriesModel(new SeriesId("6"), "LongLabel-006", SeriesType.Line, xField, yField, xAxis, yAxis);
            var threePresentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(new GraphModel(new[] { xAxis, yAxis }, new[] { s1, s2, s3, s4, s5, s6 })), opts);
            
            var oneBandHeight = onePresentation.Layout.Legend.TopRight.Y - onePresentation.Layout.Legend.BottomLeft.Y;
            var threeBandHeight = threePresentation.Layout.Legend.TopRight.Y - threePresentation.Layout.Legend.BottomLeft.Y;
            
            Assert.That(threeBandHeight, Is.GreaterThan(oneBandHeight),
                "More series entries should produce a taller bottom legend band.");
        }
        
        [Test]
        public void Layout_LegendBandWidth_IsContentDriven_NotFixedConstant()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });
            var opts = new GraphPresentationOptions(legendPlacement: LegendPlacement.Left);
            
            string[] labels = { "S", "Medium Label", "A Very Very Long Label Indeed" };
            var bandWidths = new double[labels.Length];
            for (var i = 0; i < labels.Length; i++)
            {
                var series = new GraphSeriesModel(new SeriesId("1"), labels[i], SeriesType.Line, xField, yField, xAxis, yAxis);
                var m = new GraphModel(new[] { xAxis, yAxis }, new[] { series });
                var p = new GraphPresentationModel(new GraphSnapshotBuilder().Build(m), opts);
                bandWidths[i] = p.Layout.Legend.TopRight.X - p.Layout.Legend.BottomLeft.X;
            }
            
            Assert.That(bandWidths[1], Is.GreaterThan(bandWidths[0]), "Medium label → wider band than short.");
            Assert.That(bandWidths[2], Is.GreaterThan(bandWidths[1]), "Long label → wider band than medium.");
        }
        
        [Test]
        public void Layout_LegendLeft_ContentDrivenWidth_StillRespectsAxisTitleZone()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });
            
            var longSeries = new GraphSeriesModel(
                new SeriesId("1"),
                "Extremely Long Series Name For Stress Testing The Legend Width Calculation",
                SeriesType.Line, xField, yField, xAxis, yAxis);
            var presentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(new GraphModel(new[] { xAxis, yAxis }, new[] { longSeries })),
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Left, resizeChart: true));
            
            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.TopRight.X, Is.LessThan(plotArea.BottomLeft.X).Within(1e-12),
                "Wide legend must not encroach on the Y-axis title protected zone.");
        }

        [Test]
        public void Layout_MultipleLeftAxes_HeterogeneousFormatting_IncreasesProtectedBandWidth()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;
            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxisShort = new AxisModel(
                new AxisId("y-short"),
                ModelAxisOrientation.Y,
                ModelAxisSide.Left,
                unit,
                "m",
                new NumericFormatter("fmt-short", registry, "F0"));
            var yAxisLong = new AxisModel(
                new AxisId("y-long"),
                ModelAxisOrientation.Y,
                ModelAxisSide.Left,
                unit,
                "m",
                new NumericFormatter("fmt-long", registry, "F6"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yFieldShort = new TestFieldDefinition("Pressure", "p", unit, new[] { 1d, 2d, 3d });
            var yFieldLong = new TestFieldDefinition("Volumetric Flow Super Long", "q", unit, new[] { 1.123456d, 2.234567d, 3.345678d });

            var singleSeries = new GraphSeriesModel(new SeriesId("s1"), "S1", SeriesType.Line, xField, yFieldShort, xAxis, yAxisShort);
            var singleModel = new GraphModel(new[] { xAxis, yAxisShort }, new[] { singleSeries });
            var singlePresentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(singleModel),
                new GraphPresentationOptions(hiddenSeriesIds: new[] { new SeriesId("s1") }));

            var s1 = new GraphSeriesModel(new SeriesId("s1"), "S1", SeriesType.Line, xField, yFieldShort, xAxis, yAxisShort);
            var s2 = new GraphSeriesModel(new SeriesId("s2"), "S2", SeriesType.Line, xField, yFieldLong, xAxis, yAxisLong);
            var multiModel = new GraphModel(new[] { xAxis, yAxisShort, yAxisLong }, new[] { s1, s2 });
            var multiPresentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(multiModel),
                new GraphPresentationOptions(hiddenSeriesIds: new[] { new SeriesId("s1"), new SeriesId("s2") }));

            Assert.That(multiPresentation.Layout.PlotArea.BottomLeft.X,
                Is.GreaterThan(singlePresentation.Layout.PlotArea.BottomLeft.X),
                "Combined left-axis protected band should be driven by the widest axis formatting.");
        }

        [Test]
        public void Layout_LegendBottom_HorizontalFlow_WrapsIntoMultipleRowsWhenWidthIsConstrained()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var leftAxis = new AxisModel(new AxisId("y-left"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", new NumericFormatter("fmt-left", registry, "F4"));
            var rightAxis = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", new NumericFormatter("fmt-right", registry, "F4"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var series = new System.Collections.Generic.List<IGraphSeriesModel>();
            for (var i = 0; i < 8; i++)
            {
                var yField = new TestFieldDefinition("Y" + i, "y" + i, unit, new[] { 10d + i, 20d + i, 30d + i });
                var yAxis = i % 2 == 0 ? leftAxis : rightAxis;
                series.Add(new GraphSeriesModel(new SeriesId("s" + i), "Long-Wrapping-Label-" + i, SeriesType.Line, xField, yField, xAxis, yAxis));
            }

            var model = new GraphModel(new IAxisModel[] { xAxis, leftAxis, rightAxis }, series);
            var presentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(model),
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Bottom, resizeChart: true));

            var legend = presentation.Layout.Legend;
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.GreaterThan(2));

            var distinctEntryTopRows = legend.Entries
                .Select(e => Math.Round(e.TopRight.Y, 6))
                .Distinct()
                .Count();

            Assert.That(distinctEntryTopRows, Is.GreaterThan(1),
                "Bottom legend should wrap entries into multiple rows when horizontal space is constrained.");
        }
        
        [Test]
        public void LegendEntry_SeriesColor_IsAssignedNonDefault()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model));
            var legend = presentation.Layout.Legend;
            
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.GreaterThan(0));
            var color = legend.Entries[0].SeriesColor;
            Assert.That(color, Is.Not.EqualTo(System.Drawing.Color.Empty),
                "Legend entry should have an explicitly assigned color.");
            Assert.That(color.A, Is.GreaterThan(0), "Assigned series color must be fully opaque.");
        }
        
        [Test]
        public void LegendEntry_MultipleSeriesColors_AreDistinct()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 0d, 1d });
            
            var s1 = new GraphSeriesModel(new SeriesId("1"), "S1", SeriesType.Line, xField, yField, xAxis, yAxis);
            var s2 = new GraphSeriesModel(new SeriesId("2"), "S2", SeriesType.Line, xField, yField, xAxis, yAxis);
            var presentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(new GraphModel(new[] { xAxis, yAxis }, new[] { s1, s2 })));
            
            var legend = presentation.Layout.Legend;
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.EqualTo(2));
            Assert.That(legend.Entries[0].SeriesColor, Is.Not.EqualTo(legend.Entries[1].SeriesColor),
                "Different series should receive distinct palette colors.");
        }
        
        [Test]
        public void SeriesGeometry_SeriesColor_MatchesLegendEntryColor()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model));
            
            var legend = presentation.Layout.Legend;
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.EqualTo(1));
            Assert.That(presentation.Series.Count, Is.EqualTo(1));
            Assert.That(legend.Entries[0].SeriesColor, Is.EqualTo(presentation.Series[0].SeriesColor),
                "Legend entry color must match the corresponding series geometry color.");
        }

        [Test]
        public void LayoutInvariant_PlotArea_NeverDropsBelowMinimum()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var options = new GraphPresentationOptions(
                graphTitle: new string('T', 200),
                graphSubtitle: new string('S', 200),
                legendPlacement: LegendPlacement.Top,
                resizeChart: true);

            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model), options);
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(plotArea.TopRight.X - plotArea.BottomLeft.X, Is.GreaterThanOrEqualTo(0.10d));
            Assert.That(plotArea.TopRight.Y - plotArea.BottomLeft.Y, Is.GreaterThanOrEqualTo(0.10d));
        }

        [Test]
        public void LayoutInvariant_AxisTitleBands_DoNotOverlapLegend()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var options = new GraphPresentationOptions(legendPlacement: LegendPlacement.Right, resizeChart: true);
            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model), options);

            var legend = presentation.Layout.Legend;
            Assert.That(legend, Is.Not.Null);

            var bands = presentation.Layout.AxisTitleBands;
            for (var i = 0; i < bands.Count; i++)
            {
                var band = bands[i];
                var overlaps = band.BottomLeft.X < legend.TopRight.X
                    && band.TopRight.X > legend.BottomLeft.X
                    && band.BottomLeft.Y < legend.TopRight.Y
                    && band.TopRight.Y > legend.BottomLeft.Y;
                Assert.That(overlaps, Is.False, "Axis-title bands must never overlap legend geometry.");
            }
        }

        [Test]
        public void Layout_AxisBands_AreSubdividedIntoNonOverlappingTitleAndTickRegions()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model));

            var leftBand = presentation.Layout.AxisTitleBands.First(b => b.Side == PresentationAxisSide.Left);
            Assert.That(leftBand.AxisTitleRegion.TopRight.X, Is.LessThanOrEqualTo(leftBand.AxisTickLabelRegion.BottomLeft.X).Within(1e-12));

            var bottomBand = presentation.Layout.AxisTitleBands.First(b => b.Side == PresentationAxisSide.Bottom);
            Assert.That(bottomBand.AxisTitleRegion.TopRight.Y, Is.LessThanOrEqualTo(bottomBand.AxisTickLabelRegion.BottomLeft.Y).Within(1e-12));

            for (var i = 0; i < leftBand.Items.Count; i++)
            {
                Assert.That(
                    leftBand.Items[i].AxisTitleRegion.TopRight.X,
                    Is.LessThanOrEqualTo(leftBand.Items[i].AxisTickLabelRegion.BottomLeft.X).Within(1e-12));
            }
        }

        [Test]
        public void Layout_LegendTopHeight_IncreasesWhenAvailableWidthDropsAndWrappingIncreases()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var leftAxis = new AxisModel(new AxisId("y-left"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", new NumericFormatter("fmt-left", registry, "F2"));
            var rightAxis = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", new NumericFormatter("fmt-right", registry, "F2"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yShort = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d });
            var yLongLeft = new TestFieldDefinition("Extremely Long Left Axis Title For Width Pressure", "yl", unit, new[] { 10d, 20d, 30d });
            var yLongRight = new TestFieldDefinition("Extremely Long Right Axis Title For Width Pressure", "yr", unit, new[] { 10d, 20d, 30d });

            var wideSeries = new System.Collections.Generic.List<IGraphSeriesModel>();
            var narrowSeries = new System.Collections.Generic.List<IGraphSeriesModel>();

            for (var i = 0; i < 10; i++)
            {
                var label = "LegendLbl" + i;
                wideSeries.Add(new GraphSeriesModel(new SeriesId("w" + i), label, SeriesType.Line, xField, yShort, xAxis, leftAxis));

                var yField = i % 2 == 0 ? yLongLeft : yLongRight;
                var yAxis = i % 2 == 0 ? leftAxis : rightAxis;
                narrowSeries.Add(new GraphSeriesModel(new SeriesId("n" + i), label, SeriesType.Line, xField, yField, xAxis, yAxis));
            }

            var wideModel = new GraphModel(new IAxisModel[] { xAxis, leftAxis }, wideSeries);
            var narrowModel = new GraphModel(new IAxisModel[] { xAxis, leftAxis, rightAxis }, narrowSeries);

            var options = new GraphPresentationOptions(legendPlacement: LegendPlacement.Top, resizeChart: true);
            var widePresentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(wideModel), options);
            var narrowPresentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(narrowModel), options);

            var wideLegendHeight = widePresentation.Layout.Legend.TopRight.Y - widePresentation.Layout.Legend.BottomLeft.Y;
            var narrowLegendHeight = narrowPresentation.Layout.Legend.TopRight.Y - narrowPresentation.Layout.Legend.BottomLeft.Y;
            var wideRows = widePresentation.Layout.Legend.Entries.Select(e => Math.Round(e.TopRight.Y, 6)).Distinct().Count();
            var narrowRows = narrowPresentation.Layout.Legend.Entries.Select(e => Math.Round(e.TopRight.Y, 6)).Distinct().Count();

            Assert.That(narrowRows, Is.GreaterThanOrEqualTo(wideRows),
                "Narrower available width should not reduce wrapping row count.");

            Assert.That(narrowLegendHeight, Is.GreaterThanOrEqualTo(wideLegendHeight),
                "Legend height should not shrink when width pressure increases wrapping demand.");
        }

        [Test]
        public void Layout_EdgePaddingBands_ExistForAllSidesWithFixedThickness()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model));

            var edgeBands = presentation.Layout.EdgePaddingBands;
            Assert.That(edgeBands.Count, Is.EqualTo(4));

            var left = edgeBands.Single(b => b.Side == PresentationAxisSide.Left);
            var right = edgeBands.Single(b => b.Side == PresentationAxisSide.Right);
            var bottom = edgeBands.Single(b => b.Side == PresentationAxisSide.Bottom);
            var top = edgeBands.Single(b => b.Side == PresentationAxisSide.Top);

            Assert.That(left.TopRight.X - left.BottomLeft.X, Is.EqualTo(EdgePaddingBandConst).Within(1e-12));
            Assert.That(right.TopRight.X - right.BottomLeft.X, Is.EqualTo(EdgePaddingBandConst).Within(1e-12));
            Assert.That(bottom.TopRight.Y - bottom.BottomLeft.Y, Is.EqualTo(EdgePaddingBandConst).Within(1e-12));
            Assert.That(top.TopRight.Y - top.BottomLeft.Y, Is.EqualTo(EdgePaddingBandConst).Within(1e-12));
        }

        [Test]
        public void Layout_Legend_ContentRegion_IsInsideLegendContainer()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var legend = presentation.Layout.Legend;
            Assert.That(legend, Is.Not.Null);

            Assert.That(legend.ContentBottomLeft.X, Is.GreaterThan(legend.BottomLeft.X));
            Assert.That(legend.ContentBottomLeft.Y, Is.GreaterThan(legend.BottomLeft.Y));
            Assert.That(legend.ContentTopRight.X, Is.LessThan(legend.TopRight.X));
            Assert.That(legend.ContentTopRight.Y, Is.LessThan(legend.TopRight.Y));
        }

        [Test]
        public void Layout_Legend_Entries_StayWithinLegendContentRegion()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var legend = presentation.Layout.Legend;
            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.GreaterThan(0));

            for (var i = 0; i < legend.Entries.Count; i++)
            {
                var entry = legend.Entries[i];
                Assert.That(entry.BottomLeft.X, Is.GreaterThanOrEqualTo(legend.ContentBottomLeft.X).Within(1e-12));
                Assert.That(entry.TopRight.X, Is.LessThanOrEqualTo(legend.ContentTopRight.X).Within(1e-12));
                Assert.That(entry.BottomLeft.Y, Is.GreaterThanOrEqualTo(legend.ContentBottomLeft.Y).Within(1e-12));
                Assert.That(entry.TopRight.Y, Is.LessThanOrEqualTo(legend.ContentTopRight.Y).Within(1e-12));
            }
        }

        [Test]
        public void Layout_LegendLeft_GrowsInCrossAxis_WhenMeasuredContentWidthIncreases()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(legendPlacement: LegendPlacement.Left, resizeChart: true);

            var narrowMeasurement = new StubLayoutMeasurementInput(
                legendAdvice: new LegendMeasurementAdvice(
                    requiredThickness: 0.12,
                    itemWidth: 0.06,
                    itemHeight: 0.03,
                    availablePrimarySpan: 0.5,
                    itemsPerPrimarySpan: 10,
                    secondaryLineCount: 1));
            var wideMeasurement = new StubLayoutMeasurementInput(
                legendAdvice: new LegendMeasurementAdvice(
                    requiredThickness: 0.28,
                    itemWidth: 0.20,
                    itemHeight: 0.03,
                    availablePrimarySpan: 0.5,
                    itemsPerPrimarySpan: 10,
                    secondaryLineCount: 1));

            var narrowPresentation = new GraphPresentationModel(snapshot, options, narrowMeasurement);
            var widePresentation = new GraphPresentationModel(snapshot, options, wideMeasurement);

            var narrowLegend = narrowPresentation.Layout.Legend;
            var wideLegend = widePresentation.Layout.Legend;

            Assert.That(widePresentation.Layout.PlotArea.BottomLeft.X,
                Is.GreaterThan(narrowPresentation.Layout.PlotArea.BottomLeft.X),
                "A wider measured left legend should withdraw more cross-axis space from the plot area.");
            Assert.That(wideLegend.ContentTopRight.X - wideLegend.ContentBottomLeft.X,
                Is.GreaterThan(narrowLegend.ContentTopRight.X - narrowLegend.ContentBottomLeft.X),
                "Legend content width should expand when the measured label footprint grows.");
            Assert.That(wideLegend.Entries[0].TopRight.X - wideLegend.Entries[0].BottomLeft.X,
                Is.GreaterThanOrEqualTo(0.20 - 1e-6).Within(1e-12),
                "Legend entry width should honor the measured cross-axis item width.");
        }

        [Test]
        public void Layout_LegendTop_DoesNotGrowInCrossAxis_WhenRowCountIsUnchanged()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(legendPlacement: LegendPlacement.Top, resizeChart: true);

            var narrowMeasurement = new StubLayoutMeasurementInput(
                legendAdvice: new LegendMeasurementAdvice(
                    requiredThickness: 0.08,
                    itemWidth: 0.06,
                    itemHeight: 0.03,
                    availablePrimarySpan: 0.5,
                    itemsPerPrimarySpan: 10,
                    secondaryLineCount: 1));
            var wideMeasurement = new StubLayoutMeasurementInput(
                legendAdvice: new LegendMeasurementAdvice(
                    requiredThickness: 0.30,
                    itemWidth: 0.20,
                    itemHeight: 0.03,
                    availablePrimarySpan: 0.5,
                    itemsPerPrimarySpan: 10,
                    secondaryLineCount: 1));

            var narrowPresentation = new GraphPresentationModel(snapshot, options, narrowMeasurement);
            var widePresentation = new GraphPresentationModel(snapshot, options, wideMeasurement);

            var narrowLegend = narrowPresentation.Layout.Legend;
            var wideLegend = widePresentation.Layout.Legend;

            Assert.That(wideLegend.TopRight.Y - wideLegend.BottomLeft.Y,
                Is.EqualTo(narrowLegend.TopRight.Y - narrowLegend.BottomLeft.Y).Within(1e-12),
                "Top legend height should not grow from single-row item width alone.");
            Assert.That(widePresentation.Layout.PlotArea.TopRight.Y,
                Is.EqualTo(narrowPresentation.Layout.PlotArea.TopRight.Y).Within(1e-12),
                "Top plot boundary should remain unchanged when no additional legend row wrapping occurs.");
        }

        [Test]
        public void Layout_AxisEndpointInsets_AutoMode_AppliesToAllAxes()
        {
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Left);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(axisEndpointInsetMode: AxisEndpointInsetMode.Auto);
            var presentation = new GraphPresentationModel(snapshot, options);

            var entries = presentation.Layout.Axes;
            var bottom = entries.Single(e => e.Side == PresentationAxisSide.Bottom);
            var left = entries.Single(e => e.Side == PresentationAxisSide.Left);

            Assert.That(bottom.TickEndpointInset, Is.GreaterThan(0d));
            Assert.That(left.TickEndpointInset, Is.GreaterThan(0d));
        }

        [Test]
        public void Layout_AxisEndpointInsets_NoneMode_DisablesInsets()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(axisEndpointInsetMode: AxisEndpointInsetMode.None);
            var presentation = new GraphPresentationModel(snapshot, options);

            for (var i = 0; i < presentation.Layout.Axes.Count; i++)
            {
                Assert.That(presentation.Layout.Axes[i].TickEndpointInset, Is.EqualTo(0d).Within(1e-12));
            }
        }

        [Test]
        public void Layout_AxisEndpointInsets_FixedMode_UsesProvidedConstant()
        {
            const double fixedInset = 0.02;
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(
                axisEndpointInsetMode: AxisEndpointInsetMode.Fixed,
                axisEndpointInsetFixedValue: fixedInset);
            var presentation = new GraphPresentationModel(snapshot, options);

            for (var i = 0; i < presentation.Layout.Axes.Count; i++)
            {
                Assert.That(presentation.Layout.Axes[i].TickEndpointInset, Is.EqualTo(fixedInset).Within(1e-12));
            }
        }

        [Test]
        public void Layout_AxisEndpointInsets_AutoMode_LeavesTickValuesAtDomainEndpoints()
        {
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Left);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(axisEndpointInsetMode: AxisEndpointInsetMode.Auto);
            var presentation = new GraphPresentationModel(snapshot, options);

            var bottomAxis = presentation.Layout.Axes.Single(e => e.Side == PresentationAxisSide.Bottom).Axis;
            var leftAxis = presentation.Layout.Axes.Single(e => e.Side == PresentationAxisSide.Left).Axis;

            Assert.That(bottomAxis.Ticks.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(leftAxis.Ticks.Count, Is.GreaterThanOrEqualTo(2));

            Assert.That(bottomAxis.MinimumValue.HasValue, Is.True);
            Assert.That(bottomAxis.MaximumValue.HasValue, Is.True);
            Assert.That(leftAxis.MinimumValue.HasValue, Is.True);
            Assert.That(leftAxis.MaximumValue.HasValue, Is.True);

            Assert.That(bottomAxis.Ticks[0].Value, Is.EqualTo(bottomAxis.MinimumValue.Value).Within(1e-12));
            Assert.That(bottomAxis.Ticks[bottomAxis.Ticks.Count - 1].Value, Is.EqualTo(bottomAxis.MaximumValue.Value).Within(1e-12));
            Assert.That(leftAxis.Ticks[0].Value, Is.EqualTo(leftAxis.MinimumValue.Value).Within(1e-12));
            Assert.That(leftAxis.Ticks[leftAxis.Ticks.Count - 1].Value, Is.EqualTo(leftAxis.MaximumValue.Value).Within(1e-12));
        }

        [Test]
        public void Layout_AxisEndpointInsets_AutoMode_KeepsVerticalTickValuesSymmetric()
        {
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Left);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(axisEndpointInsetMode: AxisEndpointInsetMode.Auto);
            var presentation = new GraphPresentationModel(snapshot, options);

            var leftAxis = presentation.Layout.Axes.Single(e => e.Side == PresentationAxisSide.Left).Axis;
            Assert.That(leftAxis.Ticks.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(leftAxis.MinimumValue.HasValue, Is.True);
            Assert.That(leftAxis.MaximumValue.HasValue, Is.True);

            var minTick = leftAxis.Ticks[0].Value;
            var maxTick = leftAxis.Ticks[leftAxis.Ticks.Count - 1].Value;
            Assert.That(minTick + maxTick, Is.EqualTo(leftAxis.MinimumValue.Value + leftAxis.MaximumValue.Value).Within(1e-12));
        }

        [Test]
        public void Layout_AxisEndpointInsets_NoneMode_LeavesTickValuesAtDomainEndpoints()
        {
            var model = CreateModelWithAxisSides(seriesType: SeriesType.Line, xAxisSide: ModelAxisSide.Bottom, yAxisSide: ModelAxisSide.Left);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var options = new GraphPresentationOptions(axisEndpointInsetMode: AxisEndpointInsetMode.None);
            var presentation = new GraphPresentationModel(snapshot, options);

            var bottomAxis = presentation.Layout.Axes.Single(e => e.Side == PresentationAxisSide.Bottom).Axis;
            var leftAxis = presentation.Layout.Axes.Single(e => e.Side == PresentationAxisSide.Left).Axis;

            Assert.That(bottomAxis.MinimumValue.HasValue, Is.True);
            Assert.That(bottomAxis.MaximumValue.HasValue, Is.True);
            Assert.That(leftAxis.MinimumValue.HasValue, Is.True);
            Assert.That(leftAxis.MaximumValue.HasValue, Is.True);

            Assert.That(bottomAxis.Ticks[0].Value, Is.EqualTo(bottomAxis.MinimumValue.Value).Within(1e-12));
            Assert.That(bottomAxis.Ticks[bottomAxis.Ticks.Count - 1].Value, Is.EqualTo(bottomAxis.MaximumValue.Value).Within(1e-12));
            Assert.That(leftAxis.Ticks[0].Value, Is.EqualTo(leftAxis.MinimumValue.Value).Within(1e-12));
            Assert.That(leftAxis.Ticks[leftAxis.Ticks.Count - 1].Value, Is.EqualTo(leftAxis.MaximumValue.Value).Within(1e-12));
        }

        [Test]
        public void Layout_LegendLeft_WidthIncreasesWhenVerticalPrimarySpanShrinksAndColumnsIncrease()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var leftAxis = new AxisModel(new AxisId("y-left"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", new NumericFormatter("fmt-left", registry, "F2"));
            var rightAxis = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", new NumericFormatter("fmt-right", registry, "F2"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d });

            var series = new System.Collections.Generic.List<IGraphSeriesModel>();
            for (var i = 0; i < 24; i++)
            {
                var yAxis = i % 2 == 0 ? leftAxis : rightAxis;
                series.Add(new GraphSeriesModel(new SeriesId("s" + i), "VerticalWrapLegendLabel-" + i, SeriesType.Line, xField, yField, xAxis, yAxis));
            }

            var wideHeightModel = new GraphModel(new IAxisModel[] { xAxis, leftAxis, rightAxis }, series);
            var narrowHeightModel = new GraphModel(new IAxisModel[] { xAxis, leftAxis, rightAxis }, series);

            var wideHeightPresentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(wideHeightModel),
                new GraphPresentationOptions(legendPlacement: LegendPlacement.Left, resizeChart: true));

            var narrowHeightPresentation = new GraphPresentationModel(
                new GraphSnapshotBuilder().Build(narrowHeightModel),
                new GraphPresentationOptions(
                    legendPlacement: LegendPlacement.Left,
                    resizeChart: true,
                    graphTitle: "Very Long Title",
                    graphSubtitle: "Very Long Subtitle"));

            var wideLegend = wideHeightPresentation.Layout.Legend;
            var narrowLegend = narrowHeightPresentation.Layout.Legend;
            Assert.That(wideLegend, Is.Not.Null);
            Assert.That(narrowLegend, Is.Not.Null);

            var wideWidth = wideLegend.TopRight.X - wideLegend.BottomLeft.X;
            var narrowWidth = narrowLegend.TopRight.X - narrowLegend.BottomLeft.X;

            var wideColumns = wideLegend.Entries.Select(e => Math.Round(e.BottomLeft.X, 6)).Distinct().Count();
            var narrowColumns = narrowLegend.Entries.Select(e => Math.Round(e.BottomLeft.X, 6)).Distinct().Count();

            Assert.That(narrowColumns, Is.GreaterThanOrEqualTo(wideColumns),
                "Reduced vertical primary span should not reduce legend column count.");
            Assert.That(narrowWidth, Is.GreaterThanOrEqualTo(wideWidth),
                "Legend width should not shrink when vertical wrapping requires additional columns.");
        }

        // ── Option A: Explicit Series-to-Axis Binding ─────────────────────────

        [Test]
        public void SeriesAxisBinding_SingleSeries_HasBoundXAndYAxisEntries()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            Assert.That(presentation.Series.Count, Is.EqualTo(1));
            Assert.That(presentation.Series[0].XAxisEntry, Is.Not.Null,
                "Series should have a bound X-axis entry.");
            Assert.That(presentation.Series[0].YAxisEntry, Is.Not.Null,
                "Series should have a bound Y-axis entry.");
            Assert.That(presentation.Series[0].XAxisEntry.Axis.AxisId, Is.EqualTo("x-axis"));
            Assert.That(presentation.Series[0].YAxisEntry.Axis.AxisId, Is.EqualTo("y-axis"));
        }

        [Test]
        public void SeriesAxisBinding_RightYAxis_SpansFullPlotHeight()
        {
            // A series bound to a right Y-axis must use a span of [0, 1] (full plot height).
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d });

            var series = new GraphSeriesModel(new SeriesId("1"), "right-series", SeriesType.Line, xField, yField, xAxis, yRight);
            var model = new GraphModel(new[] { xAxis, yRight }, new[] { series });

            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            Assert.That(presentation.Series.Count, Is.EqualTo(1));
            var yEntry = presentation.Series[0].YAxisEntry;
            Assert.That(yEntry, Is.Not.Null);
            Assert.That(yEntry.Side, Is.EqualTo(PresentationAxisSide.Right));
            Assert.That(yEntry.NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12),
                "Right Y-axis span should start at 0 (full plot height).");
            Assert.That(yEntry.NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12),
                "Right Y-axis span should end at 1 (full plot height).");
        }

        [Test]
        public void SeriesAxisBinding_MixedLeftAndRightAxes_EachSeriesUsesCorrectEntry()
        {
            // Two stacked left Y-axes and one right Y-axis; each series must bind to its declared axis.
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft1 = new AxisModel(new AxisId("y-left-1"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft2 = new AxisModel(new AxisId("y-left-2"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yField1 = new TestFieldDefinition("Y1", "y1", unit, new[] { 10d, 20d, 30d });
            var yField2 = new TestFieldDefinition("Y2", "y2", unit, new[] { 100d, 200d, 300d });
            var yFieldR = new TestFieldDefinition("YR", "yr", unit, new[] { 1000d, 2000d, 3000d });

            var s1 = new GraphSeriesModel(new SeriesId("1"), "left1-series", SeriesType.Line, xField, yField1, xAxis, yLeft1);
            var s2 = new GraphSeriesModel(new SeriesId("2"), "left2-series", SeriesType.Line, xField, yField2, xAxis, yLeft2);
            var s3 = new GraphSeriesModel(new SeriesId("3"), "right-series", SeriesType.Line, xField, yFieldR, xAxis, yRight);

            var model = new GraphModel(new[] { xAxis, yLeft1, yLeft2, yRight }, new[] { s1, s2, s3 });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            Assert.That(presentation.Series.Count, Is.EqualTo(3));

            var left1Entry = presentation.Series[0].YAxisEntry;
            var left2Entry = presentation.Series[1].YAxisEntry;
            var rightEntry = presentation.Series[2].YAxisEntry;

            Assert.That(left1Entry, Is.Not.Null);
            Assert.That(left2Entry, Is.Not.Null);
            Assert.That(rightEntry, Is.Not.Null);

            Assert.That(left1Entry.Axis.AxisId, Is.EqualTo("y-left-1"));
            Assert.That(left2Entry.Axis.AxisId, Is.EqualTo("y-left-2"));
            Assert.That(rightEntry.Axis.AxisId, Is.EqualTo("y-right"));

            // Stacked left axes must not span full height (two axes share the vertical space).
            Assert.That(left1Entry.NormalizedSpanEnd - left1Entry.NormalizedSpanStart,
                Is.LessThan(1d - 1e-12),
                "Stacked left axis should not span the full plot height.");
            Assert.That(left2Entry.NormalizedSpanEnd - left2Entry.NormalizedSpanStart,
                Is.LessThan(1d - 1e-12),
                "Stacked left axis should not span the full plot height.");

            // Right axis must span full height.
            Assert.That(rightEntry.NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12));
            Assert.That(rightEntry.NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12));
        }

        [Test]
        public void SeriesAxisBinding_AxisIdentityEnforced_IgnoresDataRangeOverlap()
        {
            // Two series whose Y data would both fit within the other axis's range;
            // each must be bound to its declared axis, not to the closest-range axis.
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft = new AxisModel(new AxisId("y-left"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            // Both series use identical Y data — a heuristic would match either axis.
            var ySharedField = new TestFieldDefinition("Y", "y", unit, new[] { 5d, 10d, 15d });

            var sLeft = new GraphSeriesModel(new SeriesId("1"), "left-series", SeriesType.Line, xField, ySharedField, xAxis, yLeft);
            var sRight = new GraphSeriesModel(new SeriesId("2"), "right-series", SeriesType.Line, xField, ySharedField, xAxis, yRight);

            var model = new GraphModel(new[] { xAxis, yLeft, yRight }, new[] { sLeft, sRight });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            Assert.That(presentation.Series.Count, Is.EqualTo(2));

            var leftSeriesYEntry = presentation.Series[0].YAxisEntry;
            var rightSeriesYEntry = presentation.Series[1].YAxisEntry;

            Assert.That(leftSeriesYEntry, Is.Not.Null);
            Assert.That(rightSeriesYEntry, Is.Not.Null);

            Assert.That(leftSeriesYEntry.Axis.AxisId, Is.EqualTo("y-left"),
                "Series declared on left axis must be bound to left axis, regardless of data range overlap.");
            Assert.That(rightSeriesYEntry.Axis.AxisId, Is.EqualTo("y-right"),
                "Series declared on right axis must be bound to right axis, regardless of data range overlap.");

            Assert.That(leftSeriesYEntry.Side, Is.EqualTo(PresentationAxisSide.Left));
            Assert.That(rightSeriesYEntry.Side, Is.EqualTo(PresentationAxisSide.Right));
        }

        [Test]
        public void SeriesAxisBinding_StackedLeftAxes_EachSeriesBindsToItsOwnSpan()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft1 = new AxisModel(new AxisId("y-left-1"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft2 = new AxisModel(new AxisId("y-left-2"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft3 = new AxisModel(new AxisId("y-left-3"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });

            var s1 = new GraphSeriesModel(new SeriesId("1"), "s1", SeriesType.Line, xField,
                new TestFieldDefinition("Y1", "y1", unit, new[] { 1d, 2d, 3d }), xAxis, yLeft1);
            var s2 = new GraphSeriesModel(new SeriesId("2"), "s2", SeriesType.Line, xField,
                new TestFieldDefinition("Y2", "y2", unit, new[] { 10d, 20d, 30d }), xAxis, yLeft2);
            var s3 = new GraphSeriesModel(new SeriesId("3"), "s3", SeriesType.Line, xField,
                new TestFieldDefinition("Y3", "y3", unit, new[] { 100d, 200d, 300d }), xAxis, yLeft3);

            var model = new GraphModel(new[] { xAxis, yLeft1, yLeft2, yLeft3 }, new[] { s1, s2, s3 });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            Assert.That(presentation.Series.Count, Is.EqualTo(3));

            var e1 = presentation.Series[0].YAxisEntry;
            var e2 = presentation.Series[1].YAxisEntry;
            var e3 = presentation.Series[2].YAxisEntry;

            Assert.That(e1.Axis.AxisId, Is.EqualTo("y-left-1"));
            Assert.That(e2.Axis.AxisId, Is.EqualTo("y-left-2"));
            Assert.That(e3.Axis.AxisId, Is.EqualTo("y-left-3"));

            // All three stacked axes must have distinct, non-overlapping spans.
            Assert.That(e1.NormalizedSpanStart, Is.Not.EqualTo(e2.NormalizedSpanStart).Within(1e-12));
            Assert.That(e2.NormalizedSpanStart, Is.Not.EqualTo(e3.NormalizedSpanStart).Within(1e-12));
        }

        // ── Legend framing regression tests (placement-specific rules) ────────────

        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void LegendFraming_LeftRight_FrameHeightTightened_ToContentHeight(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: placement));
            var legend = presentation.Layout.Legend;

            Assert.That(legend, Is.Not.Null);

            var frameHeight = legend.TopRight.Y - legend.BottomLeft.Y;

            // The allocated band height for a left/right legend spans most of the chart height.
            // After tightening, the frame height must be a small fraction of that — not the full band.
            Assert.That(frameHeight, Is.LessThan(0.15),
                "Left/right legend frame must be height-tightened to content, not span the full band.");
        }

        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void LegendFraming_LeftRight_FrameIsTopAligned(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: placement));
            var legend = presentation.Layout.Legend;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.GreaterThan(0));

            // The gap between the frame top and the first entry top should be small (inner padding only).
            var gapAboveFirstEntry = legend.TopRight.Y - legend.Entries[0].TopRight.Y;
            Assert.That(gapAboveFirstEntry, Is.GreaterThanOrEqualTo(0d),
                "Frame top must be above or equal to first entry top.");
            Assert.That(gapAboveFirstEntry, Is.LessThan(0.03),
                "Frame must be top-aligned: gap above first entry must be small (inner padding only).");
        }

        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void LegendFraming_LeftRight_WidthUnchanged_FullBandWidth(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var presentation = new GraphPresentationModel(
                snapshot, new GraphPresentationOptions(legendPlacement: placement, resizeChart: true));
            var legend = presentation.Layout.Legend;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.GreaterThan(0));

            // The frame must be strictly wider than any single entry — horizontal space is not tightened.
            var entry = legend.Entries[0];
            var entryWidth = entry.TopRight.X - entry.BottomLeft.X;
            var frameWidth = legend.TopRight.X - legend.BottomLeft.X;

            Assert.That(frameWidth, Is.GreaterThan(entryWidth),
                "Left/right legend frame width must exceed the entry width (full band width, not tightened).");

            // The frame left must be strictly left of the entry (inner padding is present on the left).
            Assert.That(legend.BottomLeft.X, Is.LessThan(entry.BottomLeft.X),
                "Frame left must be to the left of the entry (horizontal padding not removed).");
        }

        [TestCase(LegendPlacement.Bottom)]
        [TestCase(LegendPlacement.Top)]
        public void LegendFraming_TopBottom_FrameHorizontallyCentered_InBand(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: placement));
            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);

            // For top/bottom placement the legend band has the same horizontal extent as the plot area
            // (both are bounded by edge padding + axis bands on each side). The legend frame must
            // be centered within that band.
            var bandMidX = (plotArea.BottomLeft.X + plotArea.TopRight.X) / 2.0;
            var frameMidX = (legend.BottomLeft.X + legend.TopRight.X) / 2.0;

            Assert.That(frameMidX, Is.EqualTo(bandMidX).Within(1e-6),
                "Legend frame must be horizontally centered within the band.");
        }

        [TestCase(LegendPlacement.Bottom)]
        [TestCase(LegendPlacement.Top)]
        public void LegendFraming_TopBottom_FrameWidthNarrowerThanBand(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: placement));
            var legend = presentation.Layout.Legend;
            var plotArea = presentation.Layout.PlotArea;

            Assert.That(legend, Is.Not.Null);

            var bandWidth = plotArea.TopRight.X - plotArea.BottomLeft.X;
            var frameWidth = legend.TopRight.X - legend.BottomLeft.X;

            // A single-entry legend must produce a frame much narrower than the full band.
            Assert.That(frameWidth, Is.LessThan(bandWidth),
                "Top/bottom legend frame must be narrower than the full band (content-driven width).");
        }

        [TestCase(LegendPlacement.Bottom)]
        [TestCase(LegendPlacement.Top)]
        public void LegendFraming_TopBottom_EntriesHorizontallyCenteredWithinFrame(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: placement));
            var legend = presentation.Layout.Legend;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.GreaterThan(0));

            // All entry bounds must remain within the legend frame after centering.
            for (var i = 0; i < legend.Entries.Count; i++)
            {
                var entry = legend.Entries[i];
                Assert.That(entry.BottomLeft.X, Is.GreaterThanOrEqualTo(legend.BottomLeft.X).Within(1e-12),
                    $"Entry {i} left must be within frame left.");
                Assert.That(entry.TopRight.X, Is.LessThanOrEqualTo(legend.TopRight.X).Within(1e-12),
                    $"Entry {i} right must be within frame right.");
            }
        }

        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void LegendFraming_LeftRight_MultipleEntries_AllContainedWithinTightenedFrame(LegendPlacement placement)
        {
            // Build a model with 3 series to test multi-entry height tightening.
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yField1 = new TestFieldDefinition("Y1", "y1", unit, new[] { 10d, 20d, 30d });
            var yField2 = new TestFieldDefinition("Y2", "y2", unit, new[] { 5d, 15d, 25d });
            var yField3 = new TestFieldDefinition("Y3", "y3", unit, new[] { 2d, 12d, 22d });

            var s1 = new GraphSeriesModel(new SeriesId("1"), "alpha", SeriesType.Line, xField, yField1, xAxis, yAxis);
            var s2 = new GraphSeriesModel(new SeriesId("2"), "beta", SeriesType.Line, xField, yField2, xAxis, yAxis);
            var s3 = new GraphSeriesModel(new SeriesId("3"), "gamma", SeriesType.Line, xField, yField3, xAxis, yAxis);

            var graphModel = new GraphModel(new[] { xAxis, yAxis }, new[] { s1, s2, s3 });
            var snapshot = new GraphSnapshotBuilder().Build(graphModel);
            var presentation = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: placement));
            var legend = presentation.Layout.Legend;

            Assert.That(legend, Is.Not.Null);
            Assert.That(legend.Entries.Count, Is.EqualTo(3));

            // All entries must be within the tightened frame.
            for (var i = 0; i < legend.Entries.Count; i++)
            {
                var entry = legend.Entries[i];
                Assert.That(entry.BottomLeft.Y, Is.GreaterThanOrEqualTo(legend.BottomLeft.Y).Within(1e-12),
                    $"Entry {i} bottom must be within frame bottom.");
                Assert.That(entry.TopRight.Y, Is.LessThanOrEqualTo(legend.TopRight.Y).Within(1e-12),
                    $"Entry {i} top must be within frame top.");
            }

            // Frame must still be tighter than the full band height.
            var frameHeight = legend.TopRight.Y - legend.BottomLeft.Y;
            Assert.That(frameHeight, Is.LessThan(0.4),
                "3-entry left/right frame height must still be tightened to content, not span the full band.");
        }

        [TestCase(LegendPlacement.Bottom)]
        [TestCase(LegendPlacement.Top)]
        [TestCase(LegendPlacement.Left)]
        [TestCase(LegendPlacement.Right)]
        public void LegendFraming_PlotAndAxesUnchanged_AfterFramingApplied(LegendPlacement placement)
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);

            var withFraming = new GraphPresentationModel(snapshot, new GraphPresentationOptions(legendPlacement: placement));
            var withoutLegend = new GraphPresentationModel(
                snapshot, new GraphPresentationOptions(hiddenSeriesIds: new[] { new SeriesId("1") }, legendPlacement: placement));

            // Plot area must only differ in the direction the legend occupies.
            // This verifies no side-effects on axis geometry or series.
            Assert.That(withFraming.Axes.Count, Is.EqualTo(withoutLegend.Axes.Count));
            for (var i = 0; i < withFraming.Axes.Count; i++)
            {
                Assert.That(withFraming.Axes[i].AxisId, Is.EqualTo(withoutLegend.Axes[i].AxisId));
                Assert.That(withFraming.Axes[i].Ticks.Count, Is.EqualTo(withoutLegend.Axes[i].Ticks.Count));
            }
        }

        private static IGraphModel CreateModel(SeriesType seriesType)
        {
            return CreateModelWithAxisSides(seriesType, ModelAxisSide.Bottom, ModelAxisSide.Left);
        }

        private static IGraphModel CreateModelWithAxisSides(SeriesType seriesType, ModelAxisSide xAxisSide, ModelAxisSide yAxisSide)
        {
            var registry = UnitsRegistry.Default;
            var unit = Units.Length.Meter;
            var formatter = new NumericFormatter("formatter-y", registry, "F2");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, xAxisSide, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, yAxisSide, unit, "m", formatter);

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d });

            var series = new GraphSeriesModel(new SeriesId("1"), "series-1", seriesType, xField, yField, xAxis, yAxis);

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
                series.Add(new GraphSeriesModel(new SeriesId($"{index + 1}"), "series-" + index, SeriesType.Line, xField, yField, xAxis, yAxis));
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

        private sealed class StubLayoutMeasurementInput : IGraphLayoutMeasurementInput
        {
            private readonly LegendMeasurementAdvice _legendAdvice;

            public StubLayoutMeasurementInput(LegendMeasurementAdvice legendAdvice = null)
            {
                _legendAdvice = legendAdvice ?? new LegendMeasurementAdvice(0d, 0d, 0d, 0d, 0, 0);
            }

            public double MeasureAxisTickThickness(PresentationAxisSide side, System.Collections.Generic.IReadOnlyList<AxisTickPresentation> ticks)
            {
                return 0d;
            }

            public double MeasureAxisTitleThickness(PresentationAxisSide side, string title)
            {
                return 0d;
            }

            public double MeasureAxisEndpointLabelExtent(PresentationAxisSide side, System.Collections.Generic.IReadOnlyList<AxisTickPresentation> ticks)
            {
                return 0d;
            }

            public LegendMeasurementAdvice MeasureLegend(
                LegendPlacement placement,
                System.Collections.Generic.IReadOnlyList<SeriesPresentationGeometry> series,
                double availablePrimarySpan)
            {
                return new LegendMeasurementAdvice(
                    _legendAdvice.RequiredThickness,
                    _legendAdvice.ItemWidth,
                    _legendAdvice.ItemHeight,
                    availablePrimarySpan,
                    _legendAdvice.ItemsPerPrimarySpan,
                    _legendAdvice.SecondaryLineCount);
            }

            public double MeasureTitleThickness(string text, bool isSubtitle)
            {
                return 0d;
            }
        }
    }
}
