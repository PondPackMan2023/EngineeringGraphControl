using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using Graphing.Controls.Snapshot;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;
using PresentationAxisSide = Graphing.Controls.Presentation.AxisSide;

namespace Graphing.Tests
{
    /// <summary>
    /// Integration tests that render to a <see cref="Bitmap"/> and verify that each series
    /// Y pixels appear within the vertical sub-region dictated by its bound
    /// <see cref="AxisLayoutEntry"/>, not relative to the full plot rectangle.
    ///
    /// These tests would fail if <c>DomainToDeviceY</c> were changed to use the full
    /// <c>plotRect</c> instead of the axis-specific <c>seriesRect</c>.
    /// </summary>
    [TestFixture]
    public class WinFormsRendererPixelMappingTests
    {
        private const int W = 600;
        private const int H = 600;
        private const int PixelRadius = 5;
        private const int ColorThreshold = 50;

        [Test]
        public void Renderer_DefaultSeriesLineWidth_IsTwoPixels()
        {
            var seriesLineWidthField = typeof(WinFormsGraphRenderer)
                .GetField("SeriesLineWidth", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.That(seriesLineWidthField, Is.Not.Null,
                "Series line width constant should exist on the renderer.");

            var value = (float)seriesLineWidthField.GetRawConstantValue();
            Assert.That(value, Is.EqualTo(2.0f));
        }

        [Test]
        public void SeriesPixelMapping_UsesAxisSpanNotFullPlotRect()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft1 = new AxisModel(new AxisId("y-left-1"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft2 = new AxisModel(new AxisId("y-left-2"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });

            var s1 = new GraphSeriesModel(new SeriesId("1"), "upper", SeriesType.Line, xField, yField, xAxis, yLeft1);
            var s2 = new GraphSeriesModel(new SeriesId("2"), "lower", SeriesType.Line, xField, yField, xAxis, yLeft2);
            var model = new GraphModel(new IAxisModel[] { xAxis, yLeft1, yLeft2 }, new IGraphSeriesModel[] { s1, s2 });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);
            var deviceBounds = new Rectangle(0, 0, W, H);

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation);

                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var upperEntry = presentation.Series[0].YAxisEntry;
                var lowerEntry = presentation.Series[1].YAxisEntry;
                var expectedUpperY = ComputeExpectedDeviceY(plotRect, upperEntry, 0d, 100d, 50d);
                var expectedLowerY = ComputeExpectedDeviceY(plotRect, lowerEntry, 0d, 100d, 50d);

                Assert.That(expectedUpperY, Is.LessThan(expectedLowerY),
                    "Upper stacked-axis series must produce a smaller device Y than the lower series.");

                var sampleX = (int)(plotRect.Left + 0.5f * plotRect.Width);

                Assert.That(
                    HasColorNear(bmp, sampleX, (int)expectedUpperY, presentation.Series[0].SeriesColor),
                    Is.True,
                    "Upper stacked-axis series pixel must appear at its axis-span midpoint.");

                Assert.That(
                    HasColorNear(bmp, sampleX, (int)expectedLowerY, presentation.Series[1].SeriesColor),
                    Is.True,
                    "Lower stacked-axis series pixel must appear at its axis-span midpoint.");
            }
        }

        [Test]
        public void RightAxisSeries_PixelMapsToFullPlotHeight()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });

            var s = new GraphSeriesModel(new SeriesId("1"), "right-series", SeriesType.Line, xField, yField, xAxis, yRight);
            var model = new GraphModel(new IAxisModel[] { xAxis, yRight }, new IGraphSeriesModel[] { s });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);
            var deviceBounds = new Rectangle(0, 0, W, H);

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation);

                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var yEntry = presentation.Series[0].YAxisEntry;

                Assert.That(yEntry.NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12));
                Assert.That(yEntry.NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12));

                var expectedY = ComputeExpectedDeviceY(plotRect, yEntry, 0d, 100d, 50d);
                var plotMidY = plotRect.Top + plotRect.Height / 2f;

                Assert.That((double)expectedY, Is.EqualTo((double)plotMidY).Within(1.0),
                    "Right-axis series at Y midpoint must map to the vertical centre of the full plot rect.");

                var sampleX = (int)(plotRect.Left + 0.5f * plotRect.Width);
                Assert.That(
                    HasColorNear(bmp, sampleX, (int)expectedY, presentation.Series[0].SeriesColor),
                    Is.True,
                    "Right-axis series must render at the vertical midpoint of the full plot height.");
            }
        }

        [Test]
        public void RightAxisSeries_WithSingleLeftAxis_MapsCorrectly()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft = new AxisModel(new AxisId("y-left"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });

            var sLeft = new GraphSeriesModel(new SeriesId("1"), "left", SeriesType.Line, xField, yField, xAxis, yLeft);
            var sRight = new GraphSeriesModel(new SeriesId("2"), "right", SeriesType.Line, xField, yField, xAxis, yRight);

            var model = new GraphModel(new IAxisModel[] { xAxis, yLeft, yRight }, new IGraphSeriesModel[] { sLeft, sRight });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);
            var deviceBounds = new Rectangle(0, 0, W, H);

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation);

                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var rightSeries = presentation.Series.Single(ps => ps.YAxisEntry.Axis.AxisId == "y-right");
                var rightEntry = rightSeries.YAxisEntry;

                Assert.That(rightEntry.Side, Is.EqualTo(PresentationAxisSide.Right));
                Assert.That(rightEntry.NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12));
                Assert.That(rightEntry.NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12));

                var expectedY = ComputeExpectedDeviceY(plotRect, rightEntry, 0d, 100d, 50d);
                var plotMidY = plotRect.Top + plotRect.Height / 2f;
                var sampleX = (int)(plotRect.Left + 0.5f * plotRect.Width);

                Assert.That((double)expectedY, Is.EqualTo((double)plotMidY).Within(1.0));
                Assert.That(HasColorNear(bmp, sampleX, (int)expectedY, rightSeries.SeriesColor), Is.True,
                    "Right-axis series must map to full-height axis space when only one left axis exists.");
            }
        }

        [Test]
        public void RightAxisSeries_UnaffectedByLeftAxisStacking()
        {
            var unit = Units.Length.Meter;
            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });

            float RenderAndFindRightSeriesY(int leftAxisCount)
            {
                var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
                var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

                var axes = new System.Collections.Generic.List<IAxisModel> { xAxis, yRight };
                var series = new System.Collections.Generic.List<IGraphSeriesModel>();

                for (var i = 0; i < leftAxisCount; i++)
                {
                    var leftAxis = new AxisModel(new AxisId($"y-left-{i}"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
                    axes.Add(leftAxis);
                    series.Add(new GraphSeriesModel(new SeriesId($"left-{i}"), $"left-{i}", SeriesType.Line, xField, yField, xAxis, leftAxis));
                }

                series.Add(new GraphSeriesModel(new SeriesId("right"), "right", SeriesType.Line, xField, yField, xAxis, yRight));

                var model = new GraphModel(axes.ToArray(), series.ToArray());
                var snapshot = new GraphSnapshotBuilder().Build(model);
                var presentation = new GraphPresentationModel(snapshot);

                using (var bmp = new Bitmap(W, H))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    new WinFormsGraphRenderer().Render(g, new Rectangle(0, 0, W, H), presentation);

                    var plotRect = ComputePlotRect(new Rectangle(0, 0, W, H), presentation);
                    var rightSeries = presentation.Series.Single(ps => ps.YAxisEntry.Axis.AxisId == "y-right");
                    var rightEntry = rightSeries.YAxisEntry;
                    var expectedY = ComputeExpectedDeviceY(plotRect, rightEntry, 0d, 100d, 50d);
                    var sampleX = (int)(plotRect.Left + 0.5f * plotRect.Width);

                    Assert.That(rightEntry.Side, Is.EqualTo(PresentationAxisSide.Right));
                    Assert.That(rightEntry.NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12));
                    Assert.That(rightEntry.NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12));
                    Assert.That(HasColorNear(bmp, sampleX, (int)expectedY, rightSeries.SeriesColor), Is.True,
                        "Right-axis series must render at the expected Y mapping.");

                    return expectedY;
                }
            }

            var yWithOneLeftAxis = RenderAndFindRightSeriesY(1);
            var yWithThreeLeftAxes = RenderAndFindRightSeriesY(3);

            Assert.That((double)yWithOneLeftAxis, Is.EqualTo((double)yWithThreeLeftAxes).Within(1.0),
                "Right-axis series Y mapping must remain stable when left-axis stacking depth changes.");
        }

        [Test]
        public void XAxisTicks_AlignWithPlotBounds_WhenEndpointInsetModeIsFixed()
        {
            const double inset = 0.05;
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var model = new GraphModel(new IAxisModel[] { xAxis, yAxis }, Array.Empty<IGraphSeriesModel>());
            var options = new GraphPresentationOptions(showGraphBorder: false, axisEndpointInsetMode: AxisEndpointInsetMode.Fixed, axisEndpointInsetFixedValue: inset);
            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model), options);
            var deviceBounds = new Rectangle(0, 0, W, H);

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation, options);

                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var bottomEntry = presentation.Layout.Axes.Single(a => a.Side == PresentationAxisSide.Bottom);
                var axisRect = ComputeAxisRectForEntry(plotRect, bottomEntry);
                var axisY = (int)Math.Round(axisRect.Bottom);

                Assert.That(bottomEntry.TickEndpointInset, Is.EqualTo(0d).Within(1e-12));
                Assert.That(axisRect.Left, Is.EqualTo(plotRect.Left).Within(1.0));
                Assert.That(axisRect.Right, Is.EqualTo(plotRect.Right).Within(1.0));

                Assert.That(HasColorNear2D(bmp, (int)Math.Round(axisRect.Left), axisY, Color.Black), Is.True,
                    "First bottom-axis tick should render at the plot-left / Y-axis intersection.");
                Assert.That(HasColorNear2D(bmp, (int)Math.Round(axisRect.Right), axisY, Color.Black), Is.True,
                    "Last bottom-axis tick should render at the plot-right endpoint.");
            }
        }

        [Test]
        public void XAxisGeometry_DoesNotShift_WhenTickLabelLengthChanges()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;
            var formatterShort = new NumericFormatter("fmt-short", registry, "X", "F0");
            var formatterLong = new NumericFormatter("fmt-long", registry, "X", "F6");

            GraphPresentationModel BuildPresentation(NumericFormatter formatter)
            {
                var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", formatter);
                var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
                var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
                var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });
                var series = new GraphSeriesModel(new SeriesId("1"), "s", SeriesType.Line, xField, yField, xAxis, yAxis);
                var model = new GraphModel(new IAxisModel[] { xAxis, yAxis }, new IGraphSeriesModel[] { series });
                return new GraphPresentationModel(
                    new GraphSnapshotBuilder().Build(model),
                    new GraphPresentationOptions(axisEndpointInsetMode: AxisEndpointInsetMode.Fixed, axisEndpointInsetFixedValue: 0.05));
            }

            var shortPresentation = BuildPresentation(formatterShort);
            var longPresentation = BuildPresentation(formatterLong);
            var deviceBounds = new Rectangle(0, 0, W, H);

            var shortPlotRect = ComputePlotRect(deviceBounds, shortPresentation);
            var longPlotRect = ComputePlotRect(deviceBounds, longPresentation);
            var shortBottomEntry = shortPresentation.Layout.Axes.Single(a => a.Side == PresentationAxisSide.Bottom);
            var longBottomEntry = longPresentation.Layout.Axes.Single(a => a.Side == PresentationAxisSide.Bottom);
            var shortAxisRect = ComputeAxisRectForEntry(shortPlotRect, shortBottomEntry);
            var longAxisRect = ComputeAxisRectForEntry(longPlotRect, longBottomEntry);
            var shortAxis = shortBottomEntry.Axis;
            var longAxis = longBottomEntry.Axis;

            Assert.That(shortBottomEntry.TickEndpointInset, Is.EqualTo(0d).Within(1e-12));
            Assert.That(longBottomEntry.TickEndpointInset, Is.EqualTo(0d).Within(1e-12));
            Assert.That(shortAxisRect.Left, Is.EqualTo(longAxisRect.Left).Within(1.0));
            Assert.That(shortAxisRect.Right, Is.EqualTo(longAxisRect.Right).Within(1.0));
            Assert.That(
                DomainToDeviceX(shortAxis.Ticks[0].Value, shortAxis.MinimumValue.Value, shortAxis.MaximumValue.Value, shortAxisRect),
                Is.EqualTo(DomainToDeviceX(longAxis.Ticks[0].Value, longAxis.MinimumValue.Value, longAxis.MaximumValue.Value, longAxisRect)).Within(1.0));
            Assert.That(
                DomainToDeviceX(shortAxis.Ticks[shortAxis.Ticks.Count - 1].Value, shortAxis.MinimumValue.Value, shortAxis.MaximumValue.Value, shortAxisRect),
                Is.EqualTo(DomainToDeviceX(longAxis.Ticks[longAxis.Ticks.Count - 1].Value, longAxis.MinimumValue.Value, longAxis.MaximumValue.Value, longAxisRect)).Within(1.0));
        }

        [Test]
        public void LeftAxisTicks_NotClipped_WithInsets()
        {
            const double inset = 0.05;
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft1 = new AxisModel(new AxisId("y-left-1"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft2 = new AxisModel(new AxisId("y-left-2"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField1 = new TestFieldDef("Y1", "y1", unit, new double[] { 0d, 50d, 100d });
            var yField2 = new TestFieldDef("Y2", "y2", unit, new double[] { 0d, 50d, 100d });
            var s1 = new GraphSeriesModel(new SeriesId("1"), "left-1", SeriesType.Line, xField, yField1, xAxis, yLeft1);
            var s2 = new GraphSeriesModel(new SeriesId("2"), "left-2", SeriesType.Line, xField, yField2, xAxis, yLeft2);

            var model = new GraphModel(new IAxisModel[] { xAxis, yLeft1, yLeft2 }, new IGraphSeriesModel[] { s1, s2 });
            var options = new GraphPresentationOptions(showGraphBorder: false, axisEndpointInsetMode: AxisEndpointInsetMode.Fixed, axisEndpointInsetFixedValue: inset);
            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model), options);
            var deviceBounds = new Rectangle(0, 0, W, H);

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation, options);

                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var leftEntries = presentation.Layout.Axes.Where(a => a.Side == PresentationAxisSide.Left).ToArray();

                for (var i = 0; i < leftEntries.Length; i++)
                {
                    var entry = leftEntries[i];
                    var axisRect = ComputeAxisRectForEntry(plotRect, entry);
                    var axis = entry.Axis;
                    var firstY = DomainToDeviceY(axis.Ticks[0].Value, axis.MinimumValue.Value, axis.MaximumValue.Value, axisRect);
                    var lastY = DomainToDeviceY(axis.Ticks[axis.Ticks.Count - 1].Value, axis.MinimumValue.Value, axis.MaximumValue.Value, axisRect);
                    var insidePlotX = (int)Math.Round(axisRect.Left + 2f);

                    Assert.That(axisRect.Top, Is.GreaterThanOrEqualTo(plotRect.Top));
                    Assert.That(axisRect.Bottom, Is.LessThanOrEqualTo(plotRect.Bottom));
                    Assert.That(HasColorNear2D(bmp, insidePlotX, (int)Math.Round(firstY), Color.Black), Is.True,
                        "First left-axis tick should render into the plot and remain visible with inset.");
                    Assert.That(HasColorNear2D(bmp, insidePlotX, (int)Math.Round(lastY), Color.Black), Is.True,
                        "Last left-axis tick should render into the plot and remain visible with inset.");
                }
            }
        }

        [Test]
        public void ThreeStackedLeftAxes_PixelVerticalOrderIsPreserved()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft1 = new AxisModel(new AxisId("y1"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft2 = new AxisModel(new AxisId("y2"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft3 = new AxisModel(new AxisId("y3"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });

            var s1 = new GraphSeriesModel(new SeriesId("1"), "upper", SeriesType.Line, xField, yField, xAxis, yLeft1);
            var s2 = new GraphSeriesModel(new SeriesId("2"), "middle", SeriesType.Line, xField, yField, xAxis, yLeft2);
            var s3 = new GraphSeriesModel(new SeriesId("3"), "lower", SeriesType.Line, xField, yField, xAxis, yLeft3);

            var model = new GraphModel(new IAxisModel[] { xAxis, yLeft1, yLeft2, yLeft3 }, new IGraphSeriesModel[] { s1, s2, s3 });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);
            var deviceBounds = new Rectangle(0, 0, W, H);

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation);

                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var entry0 = presentation.Series[0].YAxisEntry;
                var entry1 = presentation.Series[1].YAxisEntry;
                var entry2 = presentation.Series[2].YAxisEntry;

                var expectedY0 = ComputeExpectedDeviceY(plotRect, entry0, 0d, 100d, 50d);
                var expectedY1 = ComputeExpectedDeviceY(plotRect, entry1, 0d, 100d, 50d);
                var expectedY2 = ComputeExpectedDeviceY(plotRect, entry2, 0d, 100d, 50d);

                Assert.That(expectedY0, Is.LessThan(expectedY1),
                    "Series 0 (upper span) must render above series 1 (middle span).");
                Assert.That(expectedY1, Is.LessThan(expectedY2),
                    "Series 1 (middle span) must render above series 2 (lower span).");

                var sampleX = (int)(plotRect.Left + 0.5f * plotRect.Width);

                Assert.That(HasColorNear(bmp, sampleX, (int)expectedY0, presentation.Series[0].SeriesColor),
                    Is.True, "Upper series pixel must be at its axis-span midpoint.");
                Assert.That(HasColorNear(bmp, sampleX, (int)expectedY1, presentation.Series[1].SeriesColor),
                    Is.True, "Middle series pixel must be at its axis-span midpoint.");
                Assert.That(HasColorNear(bmp, sampleX, (int)expectedY2, presentation.Series[2].SeriesColor),
                    Is.True, "Lower series pixel must be at its axis-span midpoint.");
            }
        }

        [Test]
        public void RightAxisSeries_RenderFullHeight_WithGridLines()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft1 = new AxisModel(new AxisId("y-left-1"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft2 = new AxisModel(new AxisId("y-left-2"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });

            var sLeft1 = new GraphSeriesModel(new SeriesId("1"), "left-1", SeriesType.Line, xField, yField, xAxis, yLeft1);
            var sLeft2 = new GraphSeriesModel(new SeriesId("2"), "left-2", SeriesType.Line, xField, yField, xAxis, yLeft2);
            var sRight = new GraphSeriesModel(new SeriesId("3"), "right", SeriesType.Line, xField, yField, xAxis, yRight);

            var model = new GraphModel(
                new IAxisModel[] { xAxis, yLeft1, yLeft2, yRight },
                new IGraphSeriesModel[] { sLeft1, sLeft2, sRight });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);
            var deviceBounds = new Rectangle(0, 0, W, H);

            Assert.That(presentation.Layout.GridLines.VerticalLines.Count, Is.GreaterThan(0));
            Assert.That(presentation.Layout.GridLines.HorizontalLines.Count, Is.GreaterThan(0));

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation);

                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var rightSeries = presentation.Series[2];
                var rightEntry = rightSeries.YAxisEntry;

                Assert.That(rightEntry.NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12));
                Assert.That(rightEntry.NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12));

                var expectedY = ComputeExpectedDeviceY(plotRect, rightEntry, 0d, 100d, 50d);
                var sampleX = (int)(plotRect.Left + 0.5f * plotRect.Width);

                Assert.That(HasColorNear(bmp, sampleX, (int)expectedY, rightSeries.SeriesColor), Is.True,
                    "Right-axis series should still map across full plot height when grid lines are enabled.");
            }
        }

        [Test]
        public void RightAxisSeries_RenderFullHeight_WhenRightAxisGridLinesAreHidden()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft1 = new AxisModel(new AxisId("y-left-1"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft2 = new AxisModel(new AxisId("y-left-2"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });

            var sLeft1 = new GraphSeriesModel(new SeriesId("1"), "left-1", SeriesType.Line, xField, yField, xAxis, yLeft1);
            var sLeft2 = new GraphSeriesModel(new SeriesId("2"), "left-2", SeriesType.Line, xField, yField, xAxis, yLeft2);
            var sRight = new GraphSeriesModel(new SeriesId("3"), "right", SeriesType.Line, xField, yField, xAxis, yRight);

            var model = new GraphModel(
                new IAxisModel[] { xAxis, yLeft1, yLeft2, yRight },
                new IGraphSeriesModel[] { sLeft1, sLeft2, sRight });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(
                snapshot,
                new GraphPresentationOptions(hiddenAxisGridLineIds: new[] { new AxisId("y-right") }));
            var deviceBounds = new Rectangle(0, 0, W, H);

            Assert.That(presentation.Layout.GridLines.VerticalLines.Count, Is.GreaterThan(0));
            Assert.That(presentation.Layout.GridLines.HorizontalLines.Count, Is.GreaterThan(0));
            Assert.That(presentation.Layout.GridLines.HorizontalLines.All(l => l.AxisEntry.Axis.AxisId != "y-right"), Is.True,
                "Renderer should receive only the geometry emitted by the presentation model.");

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation);

                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var rightSeries = presentation.Series[2];
                var rightEntry = rightSeries.YAxisEntry;

                Assert.That(rightEntry.NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12));
                Assert.That(rightEntry.NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12));

                var expectedY = ComputeExpectedDeviceY(plotRect, rightEntry, 0d, 100d, 50d);
                var sampleX = (int)(plotRect.Left + 0.5f * plotRect.Width);

                Assert.That(HasColorNear(bmp, sampleX, (int)expectedY, rightSeries.SeriesColor), Is.True,
                    "Right-axis series should still map across full plot height when right-axis grid lines are hidden.");
            }
        }

        [Test]
        public void Ticks_GridLines_Series_UseSameAxisMapping()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });
            var series = new GraphSeriesModel(new SeriesId("1"), "s", SeriesType.Line, xField, yField, xAxis, yAxis);

            var model = new GraphModel(new IAxisModel[] { xAxis, yAxis }, new IGraphSeriesModel[] { series });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var s = presentation.Series[0];
            var yEntry = s.YAxisEntry;
            var yPresentationAxis = yEntry.Axis;
            var plotRect = ComputePlotRect(new Rectangle(0, 0, W, H), presentation);
            var axisRect = ComputeAxisRectForEntry(plotRect, yEntry);

            var tickValue = yPresentationAxis.Ticks[0].Value;
            var horizontalGrid = presentation.Layout.GridLines.HorizontalLines
                .Single(l => l.AxisEntry.Axis.AxisId == yPresentationAxis.AxisId && Math.Abs(l.Start.Y - tickValue) < 1e-12);
            var seriesY = s.Points[0].Y;

            var yFromTick = DomainToDeviceY(tickValue, yPresentationAxis.MinimumValue.Value, yPresentationAxis.MaximumValue.Value, axisRect);
            var yFromGrid = DomainToDeviceY(horizontalGrid.Start.Y, yPresentationAxis.MinimumValue.Value, yPresentationAxis.MaximumValue.Value, axisRect);
            var yFromSeries = DomainToDeviceY(seriesY, yPresentationAxis.MinimumValue.Value, yPresentationAxis.MaximumValue.Value, axisRect);

            Assert.That(yFromTick, Is.EqualTo(yFromGrid).Within(1e-6));
            Assert.That(yFromTick, Is.EqualTo(yFromSeries).Within(1e-6));
        }

        [Test]
        public void RightAxisSeries_AndTicks_AlignCorrectly()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft1 = new AxisModel(new AxisId("y-left-1"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yLeft2 = new AxisModel(new AxisId("y-left-2"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });

            var sLeft1 = new GraphSeriesModel(new SeriesId("1"), "left-1", SeriesType.Line, xField, yField, xAxis, yLeft1);
            var sLeft2 = new GraphSeriesModel(new SeriesId("2"), "left-2", SeriesType.Line, xField, yField, xAxis, yLeft2);
            var sRight = new GraphSeriesModel(new SeriesId("3"), "right", SeriesType.Line, xField, yField, xAxis, yRight);

            var model = new GraphModel(
                new IAxisModel[] { xAxis, yLeft1, yLeft2, yRight },
                new IGraphSeriesModel[] { sLeft1, sLeft2, sRight });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);

            var rightSeries = presentation.Series.Single(ps => ps.YAxisEntry.Axis.AxisId == "y-right");
            var rightAxis = presentation.Layout.Axes.Single(e => e.Axis.AxisId == "y-right").Axis;
            var rightEntry = rightSeries.YAxisEntry;

            var plotRect = ComputePlotRect(new Rectangle(0, 0, W, H), presentation);
            var rightAxisRect = ComputeAxisRectForEntry(plotRect, rightEntry);

            var tickValue = rightAxis.Ticks[0].Value;
            var seriesY = rightSeries.Points[0].Y;

            var yFromTick = DomainToDeviceY(tickValue, rightAxis.MinimumValue.Value, rightAxis.MaximumValue.Value, rightAxisRect);
            var yFromSeries = DomainToDeviceY(seriesY, rightAxis.MinimumValue.Value, rightAxis.MaximumValue.Value, rightAxisRect);

            Assert.That(rightEntry.Side, Is.EqualTo(PresentationAxisSide.Right));
            Assert.That(rightEntry.NormalizedSpanStart, Is.EqualTo(0d).Within(1e-12));
            Assert.That(rightEntry.NormalizedSpanEnd, Is.EqualTo(1d).Within(1e-12));
            Assert.That(yFromTick, Is.EqualTo(yFromSeries).Within(1e-6));
        }

        [Test]
        public void AxisTickGeometry_IsExplicitAndOrientationCorrect()
        {
            var unit = Units.Length.Meter;
            var xBottom = new AxisModel(new AxisId("x-bottom"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft = new AxisModel(new AxisId("y-left"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });
            var leftSeries = new GraphSeriesModel(new SeriesId("left"), "left", SeriesType.Line, xField, yField, xBottom, yLeft);
            var rightSeries = new GraphSeriesModel(new SeriesId("right"), "right", SeriesType.Line, xField, yField, xBottom, yRight);

            var model = new GraphModel(
                new IAxisModel[] { xBottom, yLeft, yRight },
                new IGraphSeriesModel[] { leftSeries, rightSeries });
            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model));

            var bottomAxis = presentation.Axes.Single(a => a.AxisId == "x-bottom");
            var leftAxis = presentation.Axes.Single(a => a.AxisId == "y-left");
            var rightAxis = presentation.Axes.Single(a => a.AxisId == "y-right");

            Assert.That(bottomAxis.Ticks.Count, Is.GreaterThan(0));
            Assert.That(leftAxis.Ticks.Count, Is.GreaterThan(0));
            Assert.That(rightAxis.Ticks.Count, Is.GreaterThan(0));

            for (var i = 0; i < bottomAxis.Ticks.Count; i++)
            {
                var tick = bottomAxis.Ticks[i];
                var dx = tick.End.X - tick.Start.X;
                var dy = tick.End.Y - tick.Start.Y;

                Assert.That(Math.Abs(dx), Is.LessThanOrEqualTo(1e-12), "X-axis ticks must not have horizontal span.");
                Assert.That(Math.Abs(dy), Is.GreaterThan(1e-12), "X-axis ticks must have non-zero vertical span.");
            }

            for (var i = 0; i < leftAxis.Ticks.Count; i++)
            {
                var tick = leftAxis.Ticks[i];
                var dx = tick.End.X - tick.Start.X;
                var dy = tick.End.Y - tick.Start.Y;

                Assert.That(Math.Abs(dy), Is.LessThanOrEqualTo(1e-12), "Y-axis ticks must not have vertical span.");
                Assert.That(dx, Is.LessThan(1e-12), "Left Y-axis ticks must point -X (away from plot).");
            }

            for (var i = 0; i < rightAxis.Ticks.Count; i++)
            {
                var tick = rightAxis.Ticks[i];
                var dx = tick.End.X - tick.Start.X;
                var dy = tick.End.Y - tick.Start.Y;

                Assert.That(Math.Abs(dy), Is.LessThanOrEqualTo(1e-12), "Y-axis ticks must not have vertical span.");
                Assert.That(dx, Is.GreaterThan(-1e-12), "Right Y-axis ticks must point +X (away from plot).");
            }
        }

        [Test]
        public void Renderer_Rerender_IsPixelDeterministic()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yLeft = new AxisModel(new AxisId("y-left"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });
            var leftSeries = new GraphSeriesModel(new SeriesId("left"), "left", SeriesType.Line, xField, yField, xAxis, yLeft);
            var rightSeries = new GraphSeriesModel(new SeriesId("right"), "right", SeriesType.Line, xField, yField, xAxis, yRight);

            var model = new GraphModel(
                new IAxisModel[] { xAxis, yLeft, yRight },
                new IGraphSeriesModel[] { leftSeries, rightSeries });
            var presentation = new GraphPresentationModel(new GraphSnapshotBuilder().Build(model));
            var renderer = new WinFormsGraphRenderer();
            var bounds = new Rectangle(0, 0, W, H);

            using (var first = new Bitmap(W, H))
            using (var second = new Bitmap(W, H))
            using (var g1 = Graphics.FromImage(first))
            using (var g2 = Graphics.FromImage(second))
            {
                g1.Clear(Color.White);
                g2.Clear(Color.White);

                renderer.Render(g1, bounds, presentation);
                renderer.Render(g2, bounds, presentation);

                Assert.That(BitmapsAreEqual(first, second), Is.True,
                    "Rendering the same presentation model twice should produce identical pixels.");
            }
        }

        [Test]
        public void DiscreteSeries_MultiplePoints_RendersVisibleMarkers()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xField = new TestFieldDef("X", "x", unit, new double[] { 0d, 0.5d, 1d });
            var yField = new TestFieldDef("Y", "y", unit, new double[] { 0d, 50d, 100d });

            var s = new GraphSeriesModel(new SeriesId("1"), "pts", SeriesType.Line, xField, yField, xAxis, yAxis,
                lineRenderMode: LineRenderMode.PointsOnly);
            var model = new GraphModel(new IAxisModel[] { xAxis, yAxis }, new IGraphSeriesModel[] { s });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);
            var deviceBounds = new Rectangle(0, 0, W, H);

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation);

                var discreteSeries = presentation.Series.Single(ps => ps.ConnectivityIntent == SeriesConnectivityIntent.Discrete);
                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var xEntry = discreteSeries.XAxisEntry;
                var yEntry = discreteSeries.YAxisEntry;
                var axisRect = ComputeAxisRectForEntry(plotRect, yEntry);

                // X=0.5 in domain [0,1], Y=50 in domain [0,100] → midpoint of plot
                var midX = (int)Math.Round(DomainToDeviceX(0.5d, xEntry.Axis.MinimumValue.Value, xEntry.Axis.MaximumValue.Value, plotRect));
                var midY = (int)Math.Round(DomainToDeviceY(50d, yEntry.Axis.MinimumValue.Value, yEntry.Axis.MaximumValue.Value, axisRect));

                Assert.That(HasColorNear2D(bmp, midX, midY, discreteSeries.SeriesColor), Is.True,
                    "A PointsOnly series must render a visible marker at the midpoint data point.");
            }
        }

        [Test]
        public void DiscreteSeries_SinglePoint_RendersVisibleMarker()
        {
            // A companion continuous series anchors the axis range so the renderer does not
            // skip the single-point PointsOnly series due to a degenerate (min==max) axis range.
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(new AxisId("x"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(new AxisId("y"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", null);

            var xFieldFull = new TestFieldDef("X", "x", unit, new double[] { 0d, 1d });
            var yFieldFull = new TestFieldDef("Y", "y", unit, new double[] { 0d, 100d });
            var xFieldSingle = new TestFieldDef("X1", "x1", unit, new double[] { 0.5d });
            var yFieldSingle = new TestFieldDef("Y1", "y1", unit, new double[] { 50d });

            // Companion LineOnly series to give axes a valid [0,100] range.
            var companion = new GraphSeriesModel(new SeriesId("anchor"), "anchor", SeriesType.Line,
                xFieldFull, yFieldFull, xAxis, yAxis);
            // Single-point PointsOnly series that would produce Points.Count==1 geometry.
            var single = new GraphSeriesModel(new SeriesId("pts"), "pts", SeriesType.Line,
                xFieldSingle, yFieldSingle, xAxis, yAxis, lineRenderMode: LineRenderMode.PointsOnly);

            var model = new GraphModel(new IAxisModel[] { xAxis, yAxis }, new IGraphSeriesModel[] { companion, single });
            var snapshot = new GraphSnapshotBuilder().Build(model);
            var presentation = new GraphPresentationModel(snapshot);
            var deviceBounds = new Rectangle(0, 0, W, H);

            using (var bmp = new Bitmap(W, H))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                new WinFormsGraphRenderer().Render(g, deviceBounds, presentation);

                // The single-point PointsOnly series emits exactly 1 geometry with 1 point.
                var discreteSeries = presentation.Series
                    .Where(ps => ps.ConnectivityIntent == SeriesConnectivityIntent.Discrete)
                    .Single(ps => ps.Points.Count == 1);

                var plotRect = ComputePlotRect(deviceBounds, presentation);
                var xEntry = discreteSeries.XAxisEntry;
                var yEntry = discreteSeries.YAxisEntry;
                var axisRect = ComputeAxisRectForEntry(plotRect, yEntry);

                // X=0.5 in [0,1], Y=50 in [0,100] → centre of plot
                var centerX = (int)Math.Round(DomainToDeviceX(0.5d, xEntry.Axis.MinimumValue.Value, xEntry.Axis.MaximumValue.Value, plotRect));
                var centerY = (int)Math.Round(DomainToDeviceY(50d, yEntry.Axis.MinimumValue.Value, yEntry.Axis.MaximumValue.Value, axisRect));

                Assert.That(HasColorNear2D(bmp, centerX, centerY, discreteSeries.SeriesColor), Is.True,
                    "A PointsOnly series with a single data point must still render a visible marker.");
            }
        }

        private static RectangleF ComputePlotRect(Rectangle deviceBounds, GraphPresentationModel model)
        {
            var pa = model.Layout.PlotArea;
            var left   = (float)(deviceBounds.Left + pa.BottomLeft.X * deviceBounds.Width);
            var right  = (float)(deviceBounds.Left + pa.TopRight.X   * deviceBounds.Width);
            var top    = (float)(deviceBounds.Bottom - pa.TopRight.Y   * deviceBounds.Height);
            var bottom = (float)(deviceBounds.Bottom - pa.BottomLeft.Y * deviceBounds.Height);
            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        /// <summary>
        /// Mirrors the renderer formula:
        ///   seriesBottom = plotRect.Bottom - spanStart * plotRect.Height
        ///   seriesHeight  = (spanEnd - spanStart) * plotRect.Height
        ///   deviceY       = seriesBottom - t * seriesHeight
        /// </summary>
        private static float ComputeExpectedDeviceY(
            RectangleF plotRect,
            AxisLayoutEntry axisEntry,
            double domainMin,
            double domainMax,
            double y)
        {
            var axisRect = ComputeAxisRectForEntry(plotRect, axisEntry);
            return DomainToDeviceY(y, domainMin, domainMax, axisRect);
        }

        private static RectangleF ComputeAxisRectForEntry(RectangleF plotRect, AxisLayoutEntry entry)
        {
            if (entry == null)
            {
                return plotRect;
            }

            var rect = ApplyAxisInsetForEntry(plotRect, entry);
            if (entry.Side != PresentationAxisSide.Left)
            {
                return rect;
            }

            var spanStart = Math.Max(0d, Math.Min(1d, entry.NormalizedSpanStart));
            var spanEnd = Math.Max(0d, Math.Min(1d, entry.NormalizedSpanEnd));
            if (spanEnd <= spanStart)
            {
                return rect;
            }

            var top = rect.Bottom - (float)(spanEnd * rect.Height);
            var bottom = rect.Bottom - (float)(spanStart * rect.Height);
            return RectangleF.FromLTRB(rect.Left, top, rect.Right, bottom);
        }

        private static RectangleF ApplyAxisInsetForEntry(RectangleF plotRect, AxisLayoutEntry entry)
        {
            if (entry == null)
            {
                return plotRect;
            }

            var inset = Math.Min(0.49d, Math.Max(0d, Math.Min(1d, entry.TickEndpointInset)));
            if (inset <= 0d)
            {
                return plotRect;
            }

            if (entry.Side == PresentationAxisSide.Left || entry.Side == PresentationAxisSide.Right)
            {
                var delta = (float)(inset * plotRect.Height);
                var top = plotRect.Top + delta;
                var bottom = plotRect.Bottom - delta;
                if (bottom <= top)
                {
                    return plotRect;
                }

                return RectangleF.FromLTRB(plotRect.Left, top, plotRect.Right, bottom);
            }

            var horizontalDelta = (float)(inset * plotRect.Width);
            var left = plotRect.Left + horizontalDelta;
            var right = plotRect.Right - horizontalDelta;
            if (right <= left)
            {
                return plotRect;
            }

            return RectangleF.FromLTRB(left, plotRect.Top, right, plotRect.Bottom);
        }

        private static float DomainToDeviceY(
            double domainValue,
            double domainMin,
            double domainMax,
            RectangleF axisRect)
        {
            var range = domainMax - domainMin;
            if (Math.Abs(range) < double.Epsilon)
            {
                return axisRect.Top + axisRect.Height / 2f;
            }

            var t = (domainValue - domainMin) / range;
            return axisRect.Bottom - (float)(t * axisRect.Height);
        }

        private static float DomainToDeviceX(
            double domainValue,
            double domainMin,
            double domainMax,
            RectangleF axisRect)
        {
            var range = domainMax - domainMin;
            if (Math.Abs(range) < double.Epsilon)
            {
                return axisRect.Left + axisRect.Width / 2f;
            }

            var t = (domainValue - domainMin) / range;
            return axisRect.Left + (float)(t * axisRect.Width);
        }

        private static bool HasColorNear(Bitmap bmp, int x, int y, Color expected)
        {
            for (var dy = -PixelRadius; dy <= PixelRadius; dy++)
            {
                var py = y + dy;
                if (py < 0 || py >= bmp.Height) { continue; }
                if (x < 0 || x >= bmp.Width) { continue; }
                var px = bmp.GetPixel(x, py);
                if (Math.Abs(px.R - expected.R) <= ColorThreshold &&
                    Math.Abs(px.G - expected.G) <= ColorThreshold &&
                    Math.Abs(px.B - expected.B) <= ColorThreshold)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasColorNear2D(Bitmap bmp, int x, int y, Color expected)
        {
            for (var dx = -PixelRadius; dx <= PixelRadius; dx++)
            {
                for (var dy = -PixelRadius; dy <= PixelRadius; dy++)
                {
                    var px = x + dx;
                    var py = y + dy;
                    if (py < 0 || py >= bmp.Height || px < 0 || px >= bmp.Width)
                    {
                        continue;
                    }

                    var pixel = bmp.GetPixel(px, py);
                    if (Math.Abs(pixel.R - expected.R) <= ColorThreshold &&
                        Math.Abs(pixel.G - expected.G) <= ColorThreshold &&
                        Math.Abs(pixel.B - expected.B) <= ColorThreshold)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool BitmapsAreEqual(Bitmap first, Bitmap second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            if (first.Width != second.Width || first.Height != second.Height)
            {
                return false;
            }

            for (var x = 0; x < first.Width; x++)
            {
                for (var y = 0; y < first.Height; y++)
                {
                    if (first.GetPixel(x, y) != second.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private sealed class TestFieldDef : GraphFieldDefinitionBase
        {
            private readonly Array _values;

            public TestFieldDef(string label, string name, Unit unit, Array values)
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
