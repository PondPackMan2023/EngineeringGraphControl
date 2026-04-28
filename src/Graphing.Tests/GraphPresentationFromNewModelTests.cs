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

            Assert.That(title.BottomLeft.X, Is.EqualTo(0d).Within(1e-12));
            Assert.That(title.TopRight.X, Is.EqualTo(1d).Within(1e-12));
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
            // A graph with one bottom (X) axis and one left (Y) axis should reserve
            // AxisSlotSize (0.1) on the left and bottom edges.
            const double AxisSlotSizeConst = 0.1;

            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var plotArea = presentation.Layout.PlotArea;

            Assert.That(plotArea.BottomLeft.X, Is.EqualTo(AxisSlotSizeConst).Within(1e-12), "Left margin should match axis slot size.");
            Assert.That(plotArea.BottomLeft.Y, Is.EqualTo(AxisSlotSizeConst).Within(1e-12), "Bottom margin should match axis slot size.");
            Assert.That(plotArea.TopRight.X, Is.EqualTo(1d).Within(1e-12), "Right edge should reach 1.0 when no right axis present.");
            Assert.That(plotArea.TopRight.Y, Is.EqualTo(1d).Within(1e-12), "Top edge should reach 1.0 when no title or top axis.");
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

            // Hiding the left axis should remove its slot, shifting the left bound of the plot area to 0.
            Assert.That(presentationHidden.Layout.PlotArea.BottomLeft.X,
                Is.LessThan(presentationVisible.Layout.PlotArea.BottomLeft.X),
                "Left plot area bound should move left when the left axis is hidden.");
            Assert.That(presentationHidden.Layout.PlotArea.BottomLeft.X, Is.EqualTo(0d).Within(1e-12));
        }

        [Test]
        public void Layout_PlotArea_BoundsUnchangedWithStackedLeftAxes()
        {
            // Stacked left axes affect internal axis span layout but should NOT change
            // the outer plot area bounds (a single AxisSlotSize margin is always used).
            var presentationSingle = CreatePresentationWithLeftAxisCount(1);
            var presentationStacked = CreatePresentationWithLeftAxisCount(3);

            var singlePlotArea = presentationSingle.Layout.PlotArea;
            var stackedPlotArea = presentationStacked.Layout.PlotArea;

            Assert.That(stackedPlotArea.BottomLeft.X, Is.EqualTo(singlePlotArea.BottomLeft.X).Within(1e-12),
                "Left margin should be the same regardless of how many left axes are stacked.");
            Assert.That(stackedPlotArea.BottomLeft.Y, Is.EqualTo(singlePlotArea.BottomLeft.Y).Within(1e-12));
            Assert.That(stackedPlotArea.TopRight.X, Is.EqualTo(singlePlotArea.TopRight.X).Within(1e-12));
            Assert.That(stackedPlotArea.TopRight.Y, Is.EqualTo(singlePlotArea.TopRight.Y).Within(1e-12));
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

            // Verify vertical lines span the plot area height
            var plotArea = presentation.Layout.PlotArea;
            for (var i = 0; i < verticalLines.Count; i++)
            {
                var line = verticalLines[i];
                Assert.That(line.Orientation, Is.EqualTo(PresentationAxisOrientation.Vertical),
                    "Lines should be vertical");
                Assert.That(line.Start.Y, Is.EqualTo(0d).Within(1e-12),
                    "Line should start at plot-local bottom bound");
                Assert.That(line.End.Y, Is.EqualTo(1d).Within(1e-12),
                    "Line should end at plot-local top bound");
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

            // Verify horizontal lines span the plot area width
            var plotArea = presentation.Layout.PlotArea;
            for (var i = 0; i < horizontalLines.Count; i++)
            {
                var line = horizontalLines[i];
                Assert.That(line.Orientation, Is.EqualTo(PresentationAxisOrientation.Horizontal),
                    "Lines should be horizontal");
                Assert.That(line.Start.X, Is.EqualTo(0d).Within(1e-12),
                    "Line should start at plot-local left bound");
                Assert.That(line.End.X, Is.EqualTo(1d).Within(1e-12),
                    "Line should end at plot-local right bound");
            }
        }

        [Test]
        public void Layout_GridLines_AlignWithTickPositions()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var verticalLines = presentation.Layout.GridLines.VerticalLines;
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;

            // Verify vertical lines are at normalized positions corresponding to tick values
            for (var i = 0; i < verticalLines.Count; i++)
            {
                var line = verticalLines[i];
                Assert.That(line.Start.X, Is.GreaterThanOrEqualTo(0d).Within(1e-12));
                Assert.That(line.Start.X, Is.LessThanOrEqualTo(1d).Within(1e-12));
            }

            // Verify horizontal lines are at normalized positions corresponding to tick values
            for (var i = 0; i < horizontalLines.Count; i++)
            {
                var line = horizontalLines[i];
                Assert.That(line.Start.Y, Is.GreaterThanOrEqualTo(0d).Within(1e-12));
                Assert.That(line.Start.Y, Is.LessThanOrEqualTo(1d).Within(1e-12));
            }
        }

        [Test]
        public void Layout_GridLines_AreClippedToPlotArea()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var verticalLines = presentation.Layout.GridLines.VerticalLines;
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;

            // All vertical line X coordinates should be within plot-local bounds
            for (var i = 0; i < verticalLines.Count; i++)
            {
                var line = verticalLines[i];
                Assert.That(line.Start.X, Is.GreaterThanOrEqualTo(0d).Within(1e-12));
                Assert.That(line.Start.X, Is.LessThanOrEqualTo(1d).Within(1e-12));
            }

            // All horizontal line Y coordinates should be within plot-local bounds
            for (var i = 0; i < horizontalLines.Count; i++)
            {
                var line = horizontalLines[i];
                Assert.That(line.Start.Y, Is.GreaterThanOrEqualTo(0d).Within(1e-12));
                Assert.That(line.Start.Y, Is.LessThanOrEqualTo(1d).Within(1e-12));
            }
        }

        [Test]
        public void Layout_GridLines_TouchPlotAreaBorderWithoutInset()
        {
            var model = CreateModel(seriesType: SeriesType.Line);
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var verticalLines = presentation.Layout.GridLines.VerticalLines;
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;

            Assert.That(verticalLines.Count, Is.GreaterThan(0));
            Assert.That(horizontalLines.Count, Is.GreaterThan(0));

            for (var i = 0; i < verticalLines.Count; i++)
            {
                var line = verticalLines[i];
                Assert.That(line.Start.Y, Is.EqualTo(0d).Within(1e-12));
                Assert.That(line.End.Y, Is.EqualTo(1d).Within(1e-12));
            }

            for (var i = 0; i < horizontalLines.Count; i++)
            {
                var line = horizontalLines[i];
                Assert.That(line.Start.X, Is.EqualTo(0d).Within(1e-12));
                Assert.That(line.End.X, Is.EqualTo(1d).Within(1e-12));
            }
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
                var spanStart = entry.NormalizedSpanStart;
                var spanEnd = entry.NormalizedSpanEnd;
                var domainMin = entry.Axis.MinimumValue.Value;
                var domainMax = entry.Axis.MaximumValue.Value;
                var domainRange = domainMax - domainMin;

                for (var tickIndex = 0; tickIndex < entry.Axis.Ticks.Count; tickIndex++)
                {
                    var tick = entry.Axis.Ticks[tickIndex];
                    var axisRelative = Math.Abs(domainRange) > double.Epsilon
                        ? (tick.Value - domainMin) / domainRange
                        : 0.5;
                    var expectedY = spanStart + (axisRelative * (spanEnd - spanStart));

                    var hasMatch = horizontalLines.Any(line =>
                        Math.Abs(line.Start.Y - expectedY) <= 1e-12 &&
                        Math.Abs(line.End.Y - expectedY) <= 1e-12);

                    Assert.That(hasMatch, Is.True,
                        "Horizontal line should align to tick within the owning axis span.");
                }
            }
        }

        [Test]
        public void Layout_GridLines_StackedYAxes_HorizontalLinesRemainWithinOwningSpans()
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
                var coveredBySomeAxisSpan = leftAxisEntries.Any(entry =>
                    y >= entry.NormalizedSpanStart - 1e-12 &&
                    y <= entry.NormalizedSpanEnd + 1e-12);

                Assert.That(coveredBySomeAxisSpan, Is.True,
                    "Horizontal grid line should not bleed outside stacked axis spans.");
            }
        }

        [Test]
        public void Layout_GridLines_SingleYAxisBehavior_RemainsUnchanged()
        {
            var presentation = CreatePresentationWithLeftAxisCount(1);
            var leftEntry = presentation.Layout.Axes.Single(a => a.Side == PresentationAxisSide.Left);
            var horizontalLines = presentation.Layout.GridLines.HorizontalLines;

            Assert.That(horizontalLines.Count, Is.EqualTo(leftEntry.Axis.Ticks.Count));

            for (var i = 0; i < horizontalLines.Count; i++)
            {
                Assert.That(horizontalLines[i].Start.X, Is.EqualTo(0d).Within(1e-12));
                Assert.That(horizontalLines[i].End.X, Is.EqualTo(1d).Within(1e-12));
                Assert.That(horizontalLines[i].Start.Y, Is.GreaterThanOrEqualTo(leftEntry.NormalizedSpanStart).Within(1e-12));
                Assert.That(horizontalLines[i].Start.Y, Is.LessThanOrEqualTo(leftEntry.NormalizedSpanEnd).Within(1e-12));
            }
        }

        private static IGraphModel CreateModel(SeriesType seriesType)
        {
            var registry = UnitsRegistry.Default;
            var unit = Units.Length.Meter;
            var formatter = new NumericFormatter("formatter-y", registry, "F2");

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", formatter);

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
    }
}
