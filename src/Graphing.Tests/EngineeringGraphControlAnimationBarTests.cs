using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Graphing.Controls;
using Graphing.Controls.Interaction;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Tests
{
    [TestFixture]
    public class EngineeringGraphControlAnimationBarTests
    {
        [Test]
        public void AnimationBarEnabled_SetValue_UpdatesStateAndInvalidates()
        {
            using (var control = new EngineeringGraphControl())
            {
                _ = control.Handle;
                var invalidatedCount = 0;
                control.Invalidated += (_, __) => invalidatedCount++;

                control.AnimationBarEnabled = true;

                Assert.That(control.AnimationBarEnabled, Is.True);
                Assert.That(invalidatedCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void AnimationBarColor_Default_IsOrangeRed()
        {
            using (var control = new EngineeringGraphControl())
            {
                Assert.That(control.AnimationBarColor, Is.EqualTo(Color.OrangeRed));
            }
        }

        [Test]
        public void AnimationBarColor_SetValue_UpdatesStateAndInvalidates()
        {
            using (var control = new EngineeringGraphControl())
            {
                _ = control.Handle;
                var invalidatedCount = 0;
                control.Invalidated += (_, __) => invalidatedCount++;

                control.AnimationBarColor = Color.LimeGreen;

                Assert.That(control.AnimationBarColor, Is.EqualTo(Color.LimeGreen));
                Assert.That(invalidatedCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void AnimationBarXIndex_SetNegative_ThrowsArgumentOutOfRange()
        {
            using (var control = new EngineeringGraphControl())
            {
                Assert.That(
                    () => control.AnimationBarXIndex = -1,
                    Throws.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [Test]
        public void AnimationBarXIndex_SetValue_RaisesChangedEventWithExpectedPayload()
        {
            using (var control = new EngineeringGraphControl())
            {
                _ = control.Handle;
                AnimationBarIndexChangedEventArgs captured = null;
                var invalidatedCount = 0;
                control.AnimationBarXIndexChanged += (_, args) => captured = args;
                control.Invalidated += (_, __) => invalidatedCount++;

                control.AnimationBarXIndex = 4;

                Assert.That(control.AnimationBarXIndex, Is.EqualTo(4));
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured.XIndex, Is.EqualTo(4));
                Assert.That(captured.PreviousXIndex, Is.Null);
                Assert.That(captured.IsUserInitiated, Is.False);
                Assert.That(invalidatedCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void AnimationBarXIndex_SecondSetValue_IncludesPreviousIndex()
        {
            using (var control = new EngineeringGraphControl())
            {
                AnimationBarIndexChangedEventArgs captured = null;
                control.AnimationBarXIndex = 2;
                control.AnimationBarXIndexChanged += (_, args) => captured = args;

                control.AnimationBarXIndex = 5;

                Assert.That(captured, Is.Not.Null);
                Assert.That(captured.XIndex, Is.EqualTo(5));
                Assert.That(captured.PreviousXIndex, Is.EqualTo(2));
            }
        }

        [Test]
        public void AnimationBarXIndex_SetSameValue_DoesNotRaiseChangedEvent()
        {
            using (var control = new EngineeringGraphControl())
            {
                var raised = 0;
                control.AnimationBarXIndex = 3;
                control.AnimationBarXIndexChanged += (_, __) => raised++;

                control.AnimationBarXIndex = 3;

                Assert.That(raised, Is.EqualTo(0));
            }
        }

        [Test]
        public void AnimationBarEnabledAndIndexed_OnPaint_DoesNotThrow()
        {
            using (var control = new TestEngineeringGraphControl())
            using (var bitmap = new Bitmap(640, 480))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                control.Size = new Size(640, 480);
                control.SetGraphSource(CreateSimpleGraphModel());
                control.AnimationBarEnabled = true;
                control.AnimationBarXIndex = 1;

                Assert.DoesNotThrow(() => control.RaisePaint(graphics));
            }
        }

        [Test]
        public void AnimationBar_IntersectionMarkers_IneligibleBarSeriesOnly_DoesNotThrow()
        {
            using (var control = new TestEngineeringGraphControl())
            using (var bitmap = new Bitmap(640, 480))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                control.Size = new Size(640, 480);
                control.SetGraphSource(CreateBarSeriesGraphModel());
                control.AnimationBarEnabled = true;
                control.AnimationBarXIndex = 1;

                Assert.DoesNotThrow(() => control.RaisePaint(graphics));
            }
        }

        [Test]
        public void AnimationBar_IntersectionMarkers_MultiSeriesLinePaint_DoesNotThrow()
        {
            using (var control = new TestEngineeringGraphControl())
            using (var bitmap = new Bitmap(640, 480))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                control.Size = new Size(640, 480);
                control.SetGraphSource(CreateMultiSeriesGraphModel());
                control.AnimationBarEnabled = true;
                control.AnimationBarXIndex = 1;

                Assert.DoesNotThrow(() => control.RaisePaint(graphics));
            }
        }

        [Test]
        public void AnimationBar_GeometricIntersection_InterpolatesAlongPolylineSegment()
        {
            var polyline = new[]
            {
                new PointF(10f, 100f),
                new PointF(30f, 60f),
                new PointF(50f, 20f)
            };

            var hit = EngineeringGraphControl.TryResolveVerticalPolylineIntersection(20f, polyline, out var intersectionY);

            Assert.That(hit, Is.True);
            Assert.That(intersectionY, Is.EqualTo(80f).Within(0.001f));
        }

        [Test]
        public void AnimationBar_ClickInPlot_DoesNotReposition()
        {
            using (var control = CreateInteractiveControl())
            {
                control.AnimationBarEnabled = true;
                control.AnimationBarXIndex = 0;

                var capturedEvents = new List<AnimationBarIndexChangedEventArgs>();
                control.AnimationBarXIndexChanged += (_, args) => capturedEvents.Add(args);

                var target = GetPlotInteriorPoint(control);
                control.RaiseMouseDown(MouseButtons.Left, target);
                control.RaiseMouseUp(MouseButtons.Left, target);

                Assert.That(control.AnimationBarXIndex, Is.EqualTo(0));
                Assert.That(capturedEvents.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void AnimationBar_Dragging_UpdatesIndexWithUserInitiatedEvents()
        {
            using (var control = CreateInteractiveControl())
            {
                control.AnimationBarEnabled = true;
                control.AnimationBarXIndex = 1;

                var capturedEvents = new List<AnimationBarIndexChangedEventArgs>();
                control.AnimationBarXIndexChanged += (_, args) => capturedEvents.Add(args);

                var dragStart = GetClientPointForIndex(control, 1);
                var dragTarget = GetClientPointForIndex(control, 3);

                control.RaiseMouseDown(MouseButtons.Left, dragStart);
                control.RaiseMouseMove(dragTarget);
                control.RaiseMouseUp(MouseButtons.Left, dragTarget);

                Assert.That(control.AnimationBarXIndex, Is.EqualTo(3));
                Assert.That(capturedEvents.Count, Is.GreaterThanOrEqualTo(1));
                var last = capturedEvents[capturedEvents.Count - 1];
                Assert.That(last.XIndex, Is.EqualTo(3));
                Assert.That(last.IsUserInitiated, Is.True);
            }
        }

        [Test]
        public void AnimationBar_HoveringOverBar_SetsMoveCursor_AndMovingAwayRestoresDefault()
        {
            using (var control = CreateInteractiveControl())
            {
                control.AnimationBarEnabled = true;
                control.AnimationBarXIndex = 1;

                control.RaiseMouseMove(GetClientPointForIndex(control, 1));
                Assert.That(control.Cursor, Is.EqualTo(Cursors.SizeAll));

                control.RaiseMouseMove(GetPlotInteriorPoint(control));
                Assert.That(control.Cursor, Is.EqualTo(Cursors.Default));
            }
        }

        private static TestEngineeringGraphControl CreateInteractiveControl()
        {
            var control = new TestEngineeringGraphControl
            {
                Size = new Size(640, 480)
            };

            control.SetGraphSource(CreateSimpleGraphModel());
            return control;
        }

        private static Point GetClientPointForIndex(TestEngineeringGraphControl control, int index)
        {
            var presentation = control.ActivePresentation;
            var series = presentation.Layout.Series[0];
            var xAxis = series.XAxisEntry.Axis;
            var plotArea = presentation.Layout.PlotArea;

            var xMin = xAxis.MinimumValue.Value;
            var xMax = xAxis.MaximumValue.Value;
            var xValue = series.Points[index].X;
            var normalized = (xValue - xMin) / (xMax - xMin);
            var abstractX = plotArea.BottomLeft.X + (normalized * (plotArea.TopRight.X - plotArea.BottomLeft.X));
            var abstractY = (plotArea.BottomLeft.Y + plotArea.TopRight.Y) * 0.5d;

            var x = (int)Math.Round(control.ClientRectangle.Left + (abstractX * control.ClientRectangle.Width), MidpointRounding.AwayFromZero);
            var y = (int)Math.Round(control.ClientRectangle.Bottom - (abstractY * control.ClientRectangle.Height), MidpointRounding.AwayFromZero);
            return new Point(x, y);
        }

        private static Point GetPlotInteriorPoint(TestEngineeringGraphControl control)
        {
            var presentation = control.ActivePresentation;
            var plotArea = presentation.Layout.PlotArea;
            var abstractX = plotArea.BottomLeft.X + ((plotArea.TopRight.X - plotArea.BottomLeft.X) * 0.75d);
            var abstractY = plotArea.BottomLeft.Y + ((plotArea.TopRight.Y - plotArea.BottomLeft.Y) * 0.5d);

            var x = (int)Math.Round(control.ClientRectangle.Left + (abstractX * control.ClientRectangle.Width), MidpointRounding.AwayFromZero);
            var y = (int)Math.Round(control.ClientRectangle.Bottom - (abstractY * control.ClientRectangle.Height), MidpointRounding.AwayFromZero);
            return new Point(x, y);
        }

        private static IGraphModel CreateSimpleGraphModel()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;
            var xAxis = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", new NumericFormatter("fmt-x", registry, "X", "F1"));
            var yAxis = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", new NumericFormatter("fmt-y", registry, "Y", "F1"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d, 3d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d, 40d });
            var series = new GraphSeriesModel(new SeriesId("1"), "s1", SeriesType.Line, xField, yField, xAxis, yAxis);

            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static IGraphModel CreateBarSeriesGraphModel()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;
            var xAxis = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", new NumericFormatter("fmt-x", registry, "X", "F1"));
            var yAxis = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", new NumericFormatter("fmt-y", registry, "Y", "F1"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d, 3d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d, 40d });
            var series = new GraphSeriesModel(new SeriesId("bar1"), "bar", SeriesType.Bar, xField, yField, xAxis, yAxis);

            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }

        private static IGraphModel CreateMultiSeriesGraphModel()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;
            var xAxis = new AxisModel(new AxisId("x-axis"), AxisOrientation.X, AxisSide.Bottom, unit, "m", new NumericFormatter("fmt-x", registry, "X", "F1"));
            var yAxis = new AxisModel(new AxisId("y-axis"), AxisOrientation.Y, AxisSide.Left, unit, "m", new NumericFormatter("fmt-y", registry, "Y", "F1"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d, 3d });
            var yField1 = new TestFieldDefinition("Y1", "y1", unit, new[] { 10d, 20d, 30d, 40d });
            var yField2 = new TestFieldDefinition("Y2", "y2", unit, new[] { 5d, 15d, 25d, 35d });
            var lineSeries = new GraphSeriesModel(new SeriesId("s1"), "s1", SeriesType.Line, xField, yField1, xAxis, yAxis);
            var scatterSeries = new GraphSeriesModel(new SeriesId("s2"), "s2", SeriesType.Scatter, xField, yField2, xAxis, yAxis);

            return new GraphModel(new[] { xAxis, yAxis }, new[] { lineSeries, scatterSeries });
        }

        private sealed class TestEngineeringGraphControl : EngineeringGraphControl
        {
            public void RaisePaint(Graphics graphics)
            {
                OnPaint(new PaintEventArgs(graphics, new Rectangle(0, 0, Width, Height)));
            }

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
