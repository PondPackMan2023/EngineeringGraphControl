using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Graphing.Controls;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

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
        public void ZoomDrag_WhenEnabled_DoesNotChangeAxisRanges()
        {
            using (var control = CreateInteractiveControl())
            {
                control.ZoomEnabled = true;

                var before = control.ActiveSnapshot.Axes
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.AxisId) && a.MinimumValue.HasValue && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                var anchor = GetPlotInteriorPoint(control, 0.2d, 0.2d);
                var target = GetPlotInteriorPoint(control, 0.9d, 0.8d);

                control.RaiseMouseDown(MouseButtons.Left, anchor);
                control.RaiseMouseMove(target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                var after = control.ActiveSnapshot.Axes
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.AxisId) && a.MinimumValue.HasValue && a.MaximumValue.HasValue)
                    .ToDictionary(a => a.AxisId, a => (a.MinimumValue.Value, a.MaximumValue.Value), StringComparer.Ordinal);

                Assert.That(after.Keys, Is.EquivalentTo(before.Keys));
                foreach (var axisId in before.Keys)
                {
                    Assert.That(after[axisId].Item1, Is.EqualTo(before[axisId].Item1).Within(1e-9d));
                    Assert.That(after[axisId].Item2, Is.EqualTo(before[axisId].Item2).Within(1e-9d));
                }
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
