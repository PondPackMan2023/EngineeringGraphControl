using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Graphing.Controls;
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

namespace Graphing.Tests
{
    [TestFixture]
    public class EngineeringGraphControlZoomTests
    {
        [Test]
        public void ZoomEnabled_Toggle_UpdatesZoomCursorAndRestoresDefault()
        {
            using (var control = CreateInteractiveControl())
            {
                Assert.That(control.Cursor, Is.EqualTo(Cursors.Default));

                control.ZoomEnabled = true;
                Assert.That(control.Cursor, Is.EqualTo(Cursors.Cross));

                control.ZoomEnabled = false;
                Assert.That(control.Cursor, Is.EqualTo(Cursors.Default));
            }
        }

        [Test]
        public void ZoomDragLifecycle_WhenEnabled_TracksAndClearsRectangle()
        {
            using (var control = CreateInteractiveControl())
            {
                control.ZoomEnabled = true;

                var plotRect = GetPlotRect(control);
                var anchor = GetPlotInteriorPoint(control, 0.25d, 0.25d);
                var outside = new Point(control.ClientRectangle.Right + 120, control.ClientRectangle.Bottom + 120);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                Assert.That(control.ZoomDragOverlayVisible, Is.True);
                Assert.That(control.ZoomDragOverlayBounds.HasValue, Is.True);

                control.RaiseMouseMove(outside);

                Assert.That(control.ZoomDragOverlayVisible, Is.True);
                Assert.That(control.ZoomDragOverlayBounds.HasValue, Is.True);
                var dragRect = control.ZoomDragOverlayBounds.Value;
                Assert.That(dragRect.Left, Is.GreaterThanOrEqualTo(plotRect.Left));
                Assert.That(dragRect.Top, Is.GreaterThanOrEqualTo(plotRect.Top));
                Assert.That(dragRect.Right, Is.LessThanOrEqualTo(plotRect.Right));
                Assert.That(dragRect.Bottom, Is.LessThanOrEqualTo(plotRect.Bottom));

                control.RaiseMouseUp(MouseButtons.Left, outside);

                Assert.That(control.ZoomDragOverlayVisible, Is.False);
                Assert.That(control.ZoomDragOverlayBounds.HasValue, Is.False);
            }
        }

        [Test]
        public void ZoomDrag_WhenDisabled_DoesNotTrackRectangleState()
        {
            using (var control = CreateInteractiveControl())
            {
                control.ZoomEnabled = false;

                var anchor = GetPlotInteriorPoint(control, 0.3d, 0.3d);
                var target = GetPlotInteriorPoint(control, 0.8d, 0.7d);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                Assert.That(control.ZoomDragOverlayVisible, Is.False);
                Assert.That(control.ZoomDragOverlayBounds.HasValue, Is.False);
            }
        }

        [Test]
        public void ZoomIn_WhenZoomRectOverlapsSingleYAxis_OnlyThatYAxisChanges()
        {
            using (var control = CreateInteractiveControl(CreateGraphModelWithStackedLeftYAxes()))
            {
                control.ZoomEnabled = true;

                var beforeY = control.ActiveSnapshot.Axes
                    .Where(a => a != null
                        && !string.IsNullOrWhiteSpace(a.AxisId)
                        && a.Orientation == ModelAxisOrientation.Y
                        && a.MinimumValue.HasValue
                        && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                var lowSpan = GetYAxisSpanInDevice(control, "y-left-low");

                var anchor = new Point(GetPlotInteriorPoint(control, 0.2d, 0.2d).X, (int)Math.Round(lowSpan.Top + (0.2f * (lowSpan.Bottom - lowSpan.Top)), MidpointRounding.AwayFromZero));
                var target = new Point(GetPlotInteriorPoint(control, 0.8d, 0.8d).X, (int)Math.Round(lowSpan.Top + (0.8f * (lowSpan.Bottom - lowSpan.Top)), MidpointRounding.AwayFromZero));

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                var afterY = control.ActiveSnapshot.Axes
                    .Where(a => a != null
                        && !string.IsNullOrWhiteSpace(a.AxisId)
                        && a.Orientation == ModelAxisOrientation.Y
                        && a.MinimumValue.HasValue
                        && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                Assert.That(afterY["y-left-low"].Item1, Is.GreaterThan(beforeY["y-left-low"].Item1 - 1e-9d));
                Assert.That(afterY["y-left-low"].Item2, Is.LessThan(beforeY["y-left-low"].Item2 + 1e-9d));
                Assert.That(afterY["y-left-high"].Item1, Is.EqualTo(beforeY["y-left-high"].Item1).Within(1e-9d));
                Assert.That(afterY["y-left-high"].Item2, Is.EqualTo(beforeY["y-left-high"].Item2).Within(1e-9d));
            }
        }

        [Test]
        public void ZoomEnabled_DefaultsToFalse()
        {
            using (var control = new EngineeringGraphControl())
            {
                Assert.That(control.ZoomEnabled, Is.False);
            }
        }

        [Test]
        public void ZoomExtents_RestoresAllAxisRangesToCapturedDefaults_AndInvalidates()
        {
            using (var control = new EngineeringGraphControl())
            {
                _ = control.Handle;
                control.Size = new Size(640, 480);

                var model = CreateGraphModelWithThreeAxes();
                control.SetGraphSource(model, new GraphPresentationOptions());

                var defaultRanges = control.ActiveSnapshot.Axes
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.AxisId) && a.MinimumValue.HasValue && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                var zoomedOptions = new GraphPresentationOptions(
                    axisOverrides: new Dictionary<AxisId, AxisOverrides>
                    {
                        { new AxisId("x-axis"), new AxisOverrides { HasFixedRange = true, Minimum = 0d, Maximum = 1d } },
                        { new AxisId("y-left"), new AxisOverrides { HasFixedRange = true, Minimum = 10d, Maximum = 20d } },
                        { new AxisId("y-right"), new AxisOverrides { HasFixedRange = true, Minimum = 100d, Maximum = 200d } }
                    });

                control.SetGraphSource(model, zoomedOptions);

                var invalidatedCount = 0;
                control.Invalidated += (_, __) => invalidatedCount++;

                control.ZoomExtents();

                var resolvedRanges = control.ActiveSnapshot.Axes
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.AxisId) && a.MinimumValue.HasValue && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                Assert.That(invalidatedCount, Is.GreaterThan(0));
                Assert.That(resolvedRanges.Keys, Is.EquivalentTo(defaultRanges.Keys));

                foreach (var axisId in defaultRanges.Keys)
                {
                    var expected = defaultRanges[axisId];
                    var actual = resolvedRanges[axisId];

                    Assert.That(actual.Item1, Is.EqualTo(expected.Item1).Within(1e-9d), $"Axis '{axisId}' minimum did not reset to default.");
                    Assert.That(actual.Item2, Is.EqualTo(expected.Item2).Within(1e-9d), $"Axis '{axisId}' maximum did not reset to default.");
                }
            }
        }

        private static IGraphModel CreateGraphModelWithThreeAxes()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", new NumericFormatter("fmt-x", registry, "X", "F1"));
            var yLeft = new AxisModel(new AxisId("y-left"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", new NumericFormatter("fmt-y-left", registry, "YLeft", "F1"));
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", new NumericFormatter("fmt-y-right", registry, "YRight", "F1"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d, 3d });
            var yFieldLeft = new TestFieldDefinition("YLeft", "yl", unit, new[] { -5d, 0d, 5d, 10d });
            var yFieldRight = new TestFieldDefinition("YRight", "yr", unit, new[] { 100d, 150d, 200d, 250d });

            var s1 = new GraphSeriesModel(new SeriesId("s1"), "s1", SeriesType.Line, xField, yFieldLeft, xAxis, yLeft);
            var s2 = new GraphSeriesModel(new SeriesId("s2"), "s2", SeriesType.Line, xField, yFieldRight, xAxis, yRight);

            return new GraphModel(new[] { xAxis, yLeft, yRight }, new[] { s1, s2 });
        }

        [Test]
        public void ZoomGesture_DownRight_ClassifiesAsZoomIn()
        {
            using (var control = CreateInteractiveControl())
            {
                control.ZoomEnabled = true;

                var anchor = GetPlotInteriorPoint(control, 0.2d, 0.2d);
                var target = GetPlotInteriorPoint(control, 0.8d, 0.8d);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                Assert.That(control.LastZoomGesture, Is.EqualTo(EngineeringGraphControl.ZoomGestureKind.ZoomIn));
            }
        }

        [Test]
        public void ZoomIn_DownRightDrag_NarrowsXAxisRange()
        {
            using (var control = CreateInteractiveControl())
            {
                control.ZoomEnabled = true;

                var defaultXAxis = control.ActiveSnapshot.Axes
                    .First(a => a != null && a.Orientation == ModelAxisOrientation.X && a.MinimumValue.HasValue);
                var defaultXMin = defaultXAxis.MinimumValue.Value;
                var defaultXMax = defaultXAxis.MaximumValue.Value;

                // Drag the left quarter → right three-quarters: zooms into a sub-range
                var anchor = GetPlotInteriorPoint(control, 0.25d, 0.25d);
                var target = GetPlotInteriorPoint(control, 0.75d, 0.75d);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                var zoomedXAxis = control.ActiveSnapshot.Axes
                    .First(a => a != null && a.Orientation == ModelAxisOrientation.X && a.MinimumValue.HasValue);

                Assert.That(zoomedXAxis.MinimumValue.Value, Is.GreaterThan(defaultXMin - 1e-9d), "Zoomed X min should be >= default X min");
                Assert.That(zoomedXAxis.MaximumValue.Value, Is.LessThan(defaultXMax + 1e-9d), "Zoomed X max should be <= default X max");
                Assert.That(zoomedXAxis.MinimumValue.Value, Is.LessThan(zoomedXAxis.MaximumValue.Value), "Zoomed X range must be non-degenerate");
                Assert.That(zoomedXAxis.MaximumValue.Value - zoomedXAxis.MinimumValue.Value,
                    Is.LessThan(defaultXMax - defaultXMin), "Zoomed X range should be narrower than default");
            }
        }

        [Test]
        public void ZoomIn_DownRightDrag_AppliesPrimaryYAxisRangeFromRectangle()
        {
            using (var control = CreateInteractiveControl())
            {
                control.ZoomEnabled = true;

                var defaultPrimaryYAxis = GetPrimaryYAxis(control);
                var defaultYMin = defaultPrimaryYAxis.MinimumValue.Value;
                var defaultYMax = defaultPrimaryYAxis.MaximumValue.Value;

                var anchor = GetPlotInteriorPoint(control, 0.25d, 0.25d);
                var target = GetPlotInteriorPoint(control, 0.75d, 0.75d);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                var zoomedPrimaryYAxis = GetPrimaryYAxis(control);

                var zoomTop = (float)Math.Min(anchor.Y, target.Y);
                var zoomBottom = (float)Math.Max(anchor.Y, target.Y);
                var expected = ResolveExpectedYAxisRange(
                    control,
                    zoomedPrimaryYAxis.AxisId,
                    zoomTop,
                    zoomBottom,
                    defaultYMin,
                    defaultYMax);

                Assert.That(zoomedPrimaryYAxis.MinimumValue.Value, Is.EqualTo(expected.Minimum).Within(1e-6d));
                Assert.That(zoomedPrimaryYAxis.MaximumValue.Value, Is.EqualTo(expected.Maximum).Within(1e-6d));
                Assert.That(zoomedPrimaryYAxis.MaximumValue.Value - zoomedPrimaryYAxis.MinimumValue.Value,
                    Is.LessThan(defaultYMax - defaultYMin));
            }
        }

        [Test]
        public void ZoomIn_DegenerateRectWidth_DoesNotChangeXAxis()
        {
            using (var control = CreateInteractiveControl())
            {
                control.ZoomEnabled = true;

                var beforeXAxis = control.ActiveSnapshot.Axes
                    .First(a => a != null && a.Orientation == ModelAxisOrientation.X && a.MinimumValue.HasValue && a.MaximumValue.HasValue);
                var beforeXMin = beforeXAxis.MinimumValue.Value;
                var beforeXMax = beforeXAxis.MaximumValue.Value;

                // Drag only 2 pixels to the right (below MinZoomWidthPixels = 4f) and downward
                var anchor = GetPlotInteriorPoint(control, 0.5d, 0.4d);
                var target = new Point(anchor.X + 2, anchor.Y + 20);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                // Gesture should still be recognized as ZoomIn (dx > 0, dy > 0)
                Assert.That(control.LastZoomGesture, Is.EqualTo(EngineeringGraphControl.ZoomGestureKind.ZoomIn));

                var afterXAxis = control.ActiveSnapshot.Axes
                    .First(a => a != null && a.Orientation == ModelAxisOrientation.X && a.MinimumValue.HasValue && a.MaximumValue.HasValue);

                Assert.That(afterXAxis.MinimumValue.Value, Is.EqualTo(beforeXMin).Within(1e-9d));
                Assert.That(afterXAxis.MaximumValue.Value, Is.EqualTo(beforeXMax).Within(1e-9d));
            }
        }

        [Test]
        public void ZoomIn_DegenerateRectHeight_DoesNotChangePrimaryYAxis_ButStillAllowsXAxisZoom()
        {
            using (var control = CreateInteractiveControl())
            {
                control.ZoomEnabled = true;

                var beforePrimaryY = GetPrimaryYAxis(control);
                var beforeYMin = beforePrimaryY.MinimumValue.Value;
                var beforeYMax = beforePrimaryY.MaximumValue.Value;

                var beforeXAxis = control.ActiveSnapshot.Axes
                    .First(a => a != null && a.Orientation == ModelAxisOrientation.X && a.MinimumValue.HasValue && a.MaximumValue.HasValue);
                var beforeXMin = beforeXAxis.MinimumValue.Value;
                var beforeXMax = beforeXAxis.MaximumValue.Value;

                // Height is only 2 pixels (< MinZoomHeightPixels), width is large enough for X zoom.
                var anchor = GetPlotInteriorPoint(control, 0.2d, 0.5d);
                var target = new Point(GetPlotInteriorPoint(control, 0.8d, 0.5d).X, anchor.Y + 2);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                var afterPrimaryY = GetPrimaryYAxis(control);
                Assert.That(afterPrimaryY.MinimumValue.Value, Is.EqualTo(beforeYMin).Within(1e-9d));
                Assert.That(afterPrimaryY.MaximumValue.Value, Is.EqualTo(beforeYMax).Within(1e-9d));

                var afterXAxis = control.ActiveSnapshot.Axes
                    .First(a => a != null && a.Orientation == ModelAxisOrientation.X && a.MinimumValue.HasValue && a.MaximumValue.HasValue);
                Assert.That(afterXAxis.MaximumValue.Value - afterXAxis.MinimumValue.Value,
                    Is.LessThan(beforeXMax - beforeXMin));
            }
        }

        [Test]
        public void ZoomIn_WhenZoomRectOverlapsBothYAxes_ComputesIndependentRangesPerAxis()
        {
            using (var control = CreateInteractiveControl(CreateGraphModelWithStackedLeftYAxes()))
            {
                control.ZoomEnabled = true;

                var beforeById = control.ActiveSnapshot.Axes
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.AxisId) && a.MinimumValue.HasValue && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                var lowSpan = GetYAxisSpanInDevice(control, "y-left-low");
                var highSpan = GetYAxisSpanInDevice(control, "y-left-high");

                var topSpan = lowSpan.Top <= highSpan.Top ? lowSpan : highSpan;
                var bottomSpan = lowSpan.Bottom >= highSpan.Bottom ? lowSpan : highSpan;

                var yTop = (int)Math.Round(topSpan.Top + (0.25f * (topSpan.Bottom - topSpan.Top)), MidpointRounding.AwayFromZero);
                var yBottom = (int)Math.Round(bottomSpan.Bottom - (0.25f * (bottomSpan.Bottom - bottomSpan.Top)), MidpointRounding.AwayFromZero);

                var anchor = new Point(GetPlotInteriorPoint(control, 0.2d, 0.2d).X, yTop);
                var target = new Point(GetPlotInteriorPoint(control, 0.8d, 0.8d).X, yBottom);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                var afterById = control.ActiveSnapshot.Axes
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.AxisId) && a.MinimumValue.HasValue && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                var lowExpected = ResolveExpectedYAxisRange(control, "y-left-low", yTop, yBottom, beforeById["y-left-low"].Item1, beforeById["y-left-low"].Item2);
                var highExpected = ResolveExpectedYAxisRange(control, "y-left-high", yTop, yBottom, beforeById["y-left-high"].Item1, beforeById["y-left-high"].Item2);

                Assert.That(afterById["y-left-low"].Item1, Is.EqualTo(lowExpected.Minimum).Within(1e-4d));
                Assert.That(afterById["y-left-low"].Item2, Is.EqualTo(lowExpected.Maximum).Within(1e-4d));
                Assert.That(afterById["y-left-high"].Item1, Is.EqualTo(highExpected.Minimum).Within(1e-4d));
                Assert.That(afterById["y-left-high"].Item2, Is.EqualTo(highExpected.Maximum).Within(1e-4d));

                // Independent ranges should generally differ because source axis domains differ.
                Assert.That(Math.Abs(afterById["y-left-low"].Item1 - afterById["y-left-high"].Item1), Is.GreaterThan(1e-3d));
                Assert.That(Math.Abs(afterById["y-left-low"].Item2 - afterById["y-left-high"].Item2), Is.GreaterThan(1e-3d));
            }
        }

        [Test]
        public void ZoomGesture_UpLeft_ClassifiesAsZoomReset_InvokesZoomExtents()
        {
            using (var control = new TestEngineeringGraphControl { Size = new Size(640, 480) })
            {
                _ = control.Handle;

                // Load with the same model object — first call captures defaults
                var model = CreateGraphModelWithThreeAxes();
                control.SetGraphSource(model, new GraphPresentationOptions());

                var defaultRanges = control.ActiveSnapshot.Axes
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.AxisId) && a.MinimumValue.HasValue && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                // Apply a zoomed state using the SAME model — same lifecycle, defaults are not overwritten
                var zoomedOptions = new GraphPresentationOptions(
                    axisOverrides: new Dictionary<AxisId, AxisOverrides>
                    {
                        { new AxisId("x-axis"), new AxisOverrides { HasFixedRange = true, Minimum = 0d, Maximum = 1d } },
                        { new AxisId("y-left"), new AxisOverrides { HasFixedRange = true, Minimum = 10d, Maximum = 20d } },
                        { new AxisId("y-right"), new AxisOverrides { HasFixedRange = true, Minimum = 100d, Maximum = 200d } }
                    });
                control.SetGraphSource(model, zoomedOptions);

                control.ZoomEnabled = true;

                // Up+Left drag: dx < 0, dy < 0
                var anchor = GetPlotInteriorPoint(control, 0.8d, 0.8d);
                var target = GetPlotInteriorPoint(control, 0.2d, 0.2d);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                Assert.That(control.LastZoomGesture, Is.EqualTo(EngineeringGraphControl.ZoomGestureKind.ZoomReset));

                // Axes should be back to defaults
                var resolvedRanges = control.ActiveSnapshot.Axes
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.AxisId) && a.MinimumValue.HasValue && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                Assert.That(resolvedRanges.Keys, Is.EquivalentTo(defaultRanges.Keys));
                foreach (var axisId in defaultRanges.Keys)
                {
                    Assert.That(resolvedRanges[axisId].Item1, Is.EqualTo(defaultRanges[axisId].Item1).Within(1e-9d), $"Axis '{axisId}' min not reset.");
                    Assert.That(resolvedRanges[axisId].Item2, Is.EqualTo(defaultRanges[axisId].Item2).Within(1e-9d), $"Axis '{axisId}' max not reset.");
                }
            }
        }

        [Test]
        public void ZoomGesture_OtherDirections_ClassifyAsNone()
        {
            // Horizontal (dx > 0, dy == 0) and vertical (dx == 0, dy > 0) should be None
            var directions = new[]
            {
                (0.2d, 0.5d, 0.8d, 0.5d),  // purely horizontal right
                (0.8d, 0.5d, 0.2d, 0.5d),  // purely horizontal left
                (0.5d, 0.2d, 0.5d, 0.8d),  // purely vertical down
                (0.5d, 0.8d, 0.5d, 0.2d),  // purely vertical up
                (0.2d, 0.8d, 0.8d, 0.2d),  // down+left diagonal
                (0.8d, 0.2d, 0.2d, 0.8d),  // up+right diagonal
            };

            foreach (var (ax, ay, tx, ty) in directions)
            {
                using (var control = CreateInteractiveControl())
                {
                    control.ZoomEnabled = true;

                    var anchor = GetPlotInteriorPoint(control, ax, ay);
                    var target = GetPlotInteriorPoint(control, tx, ty);

                    // If anchor == target after clamping (e.g. purely horizontal on 1px boundary), skip
                    if (anchor == target)
                        continue;

                    control.RaiseMouseDown(MouseButtons.Left, anchor);
                    control.RaiseMouseMove(target);
                    control.RaiseMouseUp(MouseButtons.Left, target);

                    Assert.That(control.LastZoomGesture, Is.EqualTo(EngineeringGraphControl.ZoomGestureKind.None),
                        $"Expected None for drag ({ax},{ay})->({tx},{ty})");
                }
            }
        }

        private static TestEngineeringGraphControl CreateInteractiveControl()
        {
            var control = new TestEngineeringGraphControl
            {
                Size = new Size(640, 480)
            };

            _ = control.Handle;
            control.SetGraphSource(CreateGraphModelWithThreeAxes(), new GraphPresentationOptions());
            return control;
        }

        private static TestEngineeringGraphControl CreateInteractiveControl(IGraphModel model)
        {
            var control = new TestEngineeringGraphControl
            {
                Size = new Size(640, 480)
            };

            _ = control.Handle;
            control.SetGraphSource(model, new GraphPresentationOptions());
            return control;
        }

        private static IGraphModel CreateGraphModelWithStackedLeftYAxes()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", new NumericFormatter("fmt-x", registry, "X", "F1"));
            var yLow = new AxisModel(new AxisId("y-left-low"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", new NumericFormatter("fmt-y-low", registry, "YLow", "F1"));
            var yHigh = new AxisModel(new AxisId("y-left-high"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", new NumericFormatter("fmt-y-high", registry, "YHigh", "F1"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d, 3d, 4d, 5d });
            var yFieldLow = new TestFieldDefinition("YLow", "yl", unit, new[] { -100d, -50d, 0d, 50d, 100d, 125d });
            var yFieldHigh = new TestFieldDefinition("YHigh", "yh", unit, new[] { 200d, 225d, 250d, 300d, 350d, 400d });

            var sLow = new GraphSeriesModel(new SeriesId("s-low"), "s-low", SeriesType.Line, xField, yFieldLow, xAxis, yLow);
            var sHigh = new GraphSeriesModel(new SeriesId("s-high"), "s-high", SeriesType.Line, xField, yFieldHigh, xAxis, yHigh);

            return new GraphModel(new[] { xAxis, yLow, yHigh }, new[] { sLow, sHigh });
        }

        private static RectangleF GetPlotRect(TestEngineeringGraphControl control)
        {
            var plotArea = control.ActivePresentation.Layout.PlotArea;
            var clientBounds = control.ClientRectangle;

            var left = (float)(clientBounds.Left + (plotArea.BottomLeft.X * clientBounds.Width));
            var right = (float)(clientBounds.Left + (plotArea.TopRight.X * clientBounds.Width));
            var top = (float)(clientBounds.Bottom - (plotArea.TopRight.Y * clientBounds.Height));
            var bottom = (float)(clientBounds.Bottom - (plotArea.BottomLeft.Y * clientBounds.Height));

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        private static Point GetPlotInteriorPoint(TestEngineeringGraphControl control, double normalizedPlotX, double normalizedPlotY)
        {
            var plotRect = GetPlotRect(control);
            var x = (int)Math.Round(plotRect.Left + (normalizedPlotX * plotRect.Width), MidpointRounding.AwayFromZero);
            var y = (int)Math.Round(plotRect.Top + (normalizedPlotY * plotRect.Height), MidpointRounding.AwayFromZero);
            return new Point(x, y);
        }

        private static IAxisSnapshot GetPrimaryYAxis(TestEngineeringGraphControl control)
        {
            var axes = control.ActiveSnapshot.Axes
                .Where(a => a != null && a.Orientation == ModelAxisOrientation.Y && a.MinimumValue.HasValue && a.MaximumValue.HasValue)
                .ToList();

            var left = axes.FirstOrDefault(a => a.Side == ModelAxisSide.Left);
            return left ?? axes.First();
        }

        private static double DeviceYToDomainForTest(TestEngineeringGraphControl control, int deviceY, double axisMin, double axisMax)
        {
            var clientBounds = control.ClientRectangle;
            var plotArea = control.ActivePresentation.Layout.PlotArea;

            var abstractY = (clientBounds.Bottom - deviceY) / (double)clientBounds.Height;
            var plotHeight = plotArea.TopRight.Y - plotArea.BottomLeft.Y;
            var t = (abstractY - plotArea.BottomLeft.Y) / plotHeight;
            t = Math.Max(0d, Math.Min(1d, t));

            return axisMin + (t * (axisMax - axisMin));
        }

        private static (float Top, float Bottom) GetYAxisSpanInDevice(TestEngineeringGraphControl control, string axisId)
        {
            var entry = control.ActivePresentation.Layout.Axes
                .First(a => a?.Axis != null
                    && string.Equals(a.Axis.AxisId, axisId, StringComparison.Ordinal)
                    && a.Axis.Orientation == PresentationAxisOrientation.Vertical);

            var plotArea = control.ActivePresentation.Layout.PlotArea;
            var clientBounds = control.ClientRectangle;
            var plotBottom = plotArea.BottomLeft.Y;
            var plotTop = plotArea.TopRight.Y;
            var plotHeight = plotTop - plotBottom;

            var normalizedStart = Math.Max(0d, Math.Min(1d, entry.NormalizedSpanStart));
            var normalizedEnd = Math.Max(0d, Math.Min(1d, entry.NormalizedSpanEnd));
            if (normalizedEnd < normalizedStart)
            {
                var swap = normalizedStart;
                normalizedStart = normalizedEnd;
                normalizedEnd = swap;
            }

            var inset = Math.Max(0d, entry.TickEndpointInset);
            var abstractBottom = plotBottom + (normalizedStart * plotHeight) + inset;
            var abstractTop = plotBottom + (normalizedEnd * plotHeight) - inset;
            if (abstractTop < abstractBottom)
            {
                var center = plotBottom + (((normalizedStart + normalizedEnd) * 0.5d) * plotHeight);
                abstractBottom = center;
                abstractTop = center;
            }

            var top = (float)(clientBounds.Bottom - (abstractTop * clientBounds.Height));
            var bottom = (float)(clientBounds.Bottom - (abstractBottom * clientBounds.Height));
            if (bottom < top)
            {
                var swap = bottom;
                bottom = top;
                top = swap;
            }

            return (top, bottom);
        }

        private static (double Minimum, double Maximum) ResolveExpectedYAxisRange(
            TestEngineeringGraphControl control,
            string axisId,
            float zoomTop,
            float zoomBottom,
            double axisMinimum,
            double axisMaximum)
        {
            var span = GetYAxisSpanInDevice(control, axisId);
            var intersectionTop = Math.Max(zoomTop, span.Top);
            var intersectionBottom = Math.Min(zoomBottom, span.Bottom);

            var yTop = DeviceYToDomainForAxisSpanForTest(control, intersectionTop, axisMinimum, axisMaximum, span.Top, span.Bottom);
            var yBottom = DeviceYToDomainForAxisSpanForTest(control, intersectionBottom, axisMinimum, axisMaximum, span.Top, span.Bottom);

            return (Math.Min(yTop, yBottom), Math.Max(yTop, yBottom));
        }

        private static double DeviceYToDomainForAxisSpanForTest(
            TestEngineeringGraphControl control,
            float deviceY,
            double axisMinimum,
            double axisMaximum,
            float axisTop,
            float axisBottom)
        {
            var clientBounds = control.ClientRectangle;
            var abstractY = (clientBounds.Bottom - deviceY) / (double)clientBounds.Height;
            var axisAbstractTop = (clientBounds.Bottom - axisTop) / (double)clientBounds.Height;
            var axisAbstractBottom = (clientBounds.Bottom - axisBottom) / (double)clientBounds.Height;
            var axisHeight = axisAbstractTop - axisAbstractBottom;
            var t = (abstractY - axisAbstractBottom) / axisHeight;
            t = Math.Max(0d, Math.Min(1d, t));
            return axisMinimum + (t * (axisMaximum - axisMinimum));
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

        private sealed class TestEngineeringGraphControl : EngineeringGraphControl
        {
            public void RaiseMouseDown(MouseButtons button, Point location)
            {
                OnMouseDown(new MouseEventArgs(button, 1, location.X, location.Y, 0));
            }

            public void RaiseMouseMove(Point location)
            {
                OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, location.X, location.Y, 0));
            }

            public void RaiseMouseUp(MouseButtons button, Point location)
            {
                OnMouseUp(new MouseEventArgs(button, 1, location.X, location.Y, 0));
            }
        }
    }
}
