using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Graphing.Controls;
using Graphing.Controls.Interaction;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering.Geometry;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;
using PresentationAxisSide = Graphing.Controls.Presentation.AxisSide;

namespace Graphing.Tests
{
    [TestFixture]
    public class EngineeringGraphControlAxisInteractionTests
    {
        [Test]
        public void MouseDown_OnAxis_RaisesAxisMouseDownWithDescriptor()
        {
            using (var control = CreateControlWithStackedAxes())
            {
                var captured = (AxisInteractionMouseEventArgs)null;
                control.AxisMouseDown += (_, args) => captured = args;

                var region = control.ActivePresentation.Layout.AxisHitRegions.First(r => r.AxisId == "y-left-1");
                var point = ToClientPoint(control, Mid(region.BottomLeft.X, region.TopRight.X), Mid(region.BottomLeft.Y, region.TopRight.Y));

                control.RaiseMouseDown(MouseButtons.Left, point);

                Assert.That(captured, Is.Not.Null);
                Assert.That(captured.Button, Is.EqualTo(MouseButtons.Left));
                Assert.That(captured.Descriptor, Is.Not.Null);
                Assert.That(captured.Descriptor.AxisId, Is.EqualTo("y-left-1"));
                Assert.That(captured.Descriptor.NumericFormatter, Is.Not.Null);
            }
        }

        [Test]
        public void MouseUp_RightButtonOnAxis_RaisesMouseUpAndContextRequested()
        {
            using (var control = CreateControlWithStackedAxes())
            {
                AxisInteractionMouseEventArgs mouseUp = null;
                AxisInteractionMouseEventArgs context = null;
                control.AxisMouseUp += (_, args) => mouseUp = args;
                control.AxisContextRequested += (_, args) => context = args;

                var region = control.ActivePresentation.Layout.AxisHitRegions.First(r => r.AxisId == "y-right");
                var point = ToClientPoint(control, Mid(region.BottomLeft.X, region.TopRight.X), Mid(region.BottomLeft.Y, region.TopRight.Y));

                control.RaiseMouseUp(MouseButtons.Right, point);

                Assert.That(mouseUp, Is.Not.Null);
                Assert.That(context, Is.Not.Null);
                Assert.That(mouseUp.Button, Is.EqualTo(MouseButtons.Right));
                Assert.That(context.Button, Is.EqualTo(MouseButtons.Right));
                Assert.That(mouseUp.Descriptor.AxisId, Is.EqualTo("y-right"));
                Assert.That(context.Descriptor.AxisId, Is.EqualTo("y-right"));
            }
        }

        [Test]
        public void MouseEvents_OutsideAxisRegions_DoNotRaiseAxisEvents()
        {
            using (var control = CreateControlWithStackedAxes())
            {
                var downCount = 0;
                var upCount = 0;
                var contextCount = 0;
                control.AxisMouseDown += (_, __) => downCount++;
                control.AxisMouseUp += (_, __) => upCount++;
                control.AxisContextRequested += (_, __) => contextCount++;

                var outsidePoint = ToClientPoint(control, 0.5d, 0.5d);
                control.RaiseMouseDown(MouseButtons.Left, outsidePoint);
                control.RaiseMouseUp(MouseButtons.Right, outsidePoint);

                Assert.That(downCount, Is.EqualTo(0));
                Assert.That(upCount, Is.EqualTo(0));
                Assert.That(contextCount, Is.EqualTo(0));
            }
        }

        [Test]
        public void MouseDown_OnStackedLeftAxes_RaisesCorrectDescriptorPerAxis()
        {
            using (var control = CreateControlWithStackedAxes())
            {
                var capturedIds = new List<string>();
                control.AxisMouseDown += (_, args) => capturedIds.Add(args.Descriptor.AxisId);

                var regions = control.ActivePresentation.Layout.AxisHitRegions
                    .Where(r => r.Side == PresentationAxisSide.Left)
                    .OrderByDescending(r => r.BottomLeft.Y)
                    .ToArray();

                Assert.That(regions.Length, Is.GreaterThanOrEqualTo(2));

                for (var i = 0; i < 2; i++)
                {
                    var region = regions[i];
                    var point = ToClientPoint(control, Mid(region.BottomLeft.X, region.TopRight.X), Mid(region.BottomLeft.Y, region.TopRight.Y));
                    control.RaiseMouseDown(MouseButtons.Left, point);
                }

                Assert.That(capturedIds.Count, Is.EqualTo(2));
                Assert.That(capturedIds[0], Is.EqualTo(regions[0].AxisId));
                Assert.That(capturedIds[1], Is.EqualTo(regions[1].AxisId));
                Assert.That(capturedIds[0], Is.Not.EqualTo(capturedIds[1]));
            }
        }

        [Test]
        public void MouseDown_OnAxisBorder_IsDeterministicAcrossRepeatedInvocations()
        {
            using (var control = CreateControlWithStackedAxes())
            {
                var region = control.ActivePresentation.Layout.AxisHitRegions.First(r => r.AxisId == "x-axis");
                var borderPoint = ToClientPoint(control, region.BottomLeft.X, Mid(region.BottomLeft.Y, region.TopRight.Y));

                var capturedIds = new List<string>();
                control.AxisMouseDown += (_, args) => capturedIds.Add(args.Descriptor.AxisId);

                for (var i = 0; i < 5; i++)
                {
                    control.RaiseMouseDown(MouseButtons.Left, borderPoint);
                }

                Assert.That(capturedIds.Count, Is.EqualTo(5));
                Assert.That(capturedIds.All(id => id == capturedIds[0]), Is.True);
            }
        }

        [Test]
        public void MouseMove_DoesNotEmitAxisEvents_InPhaseH4()
        {
            using (var control = CreateControlWithStackedAxes())
            {
                var downCount = 0;
                var upCount = 0;
                var contextCount = 0;
                control.AxisMouseDown += (_, __) => downCount++;
                control.AxisMouseUp += (_, __) => upCount++;
                control.AxisContextRequested += (_, __) => contextCount++;

                var region = control.ActivePresentation.Layout.AxisHitRegions.First(r => r.AxisId == "y-left-1");
                var point = ToClientPoint(control, Mid(region.BottomLeft.X, region.TopRight.X), Mid(region.BottomLeft.Y, region.TopRight.Y));

                control.RaiseMouseMove(point);

                Assert.That(downCount, Is.EqualTo(0));
                Assert.That(upCount, Is.EqualTo(0));
                Assert.That(contextCount, Is.EqualTo(0));
            }
        }

        private static TestEngineeringGraphControl CreateControlWithStackedAxes()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", new NumericFormatter("fmt-x", registry, "X", "F1"));
            var yLeft1 = new AxisModel(new AxisId("y-left-1"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", new NumericFormatter("fmt-y1", registry, "Y1", "F2"));
            var yLeft2 = new AxisModel(new AxisId("y-left-2"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", new NumericFormatter("fmt-y2", registry, "Y2", "F2"));
            var yRight = new AxisModel(new AxisId("y-right"), ModelAxisOrientation.Y, ModelAxisSide.Right, unit, "m", new NumericFormatter("fmt-yr", registry, "YR", "F2"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d });
            var s1 = new GraphSeriesModel(new SeriesId("1"), "s1", SeriesType.Line, xField, new TestFieldDefinition("Y1", "y1", unit, new[] { 1d, 2d, 3d }), xAxis, yLeft1);
            var s2 = new GraphSeriesModel(new SeriesId("2"), "s2", SeriesType.Line, xField, new TestFieldDefinition("Y2", "y2", unit, new[] { 10d, 20d, 30d }), xAxis, yLeft2);
            var s3 = new GraphSeriesModel(new SeriesId("3"), "s3", SeriesType.Line, xField, new TestFieldDefinition("Y3", "y3", unit, new[] { 100d, 200d, 300d }), xAxis, yRight);

            var model = new GraphModel(new[] { xAxis, yLeft1, yLeft2, yRight }, new[] { s1, s2, s3 });

            var control = new TestEngineeringGraphControl
            {
                Size = new Size(640, 480)
            };

            control.SetGraphSource(model, new GraphPresentationOptions());
            return control;
        }

        private static Point ToClientPoint(Control control, double normalizedX, double normalizedY)
        {
            var x = (int)Math.Round(normalizedX * control.ClientSize.Width, MidpointRounding.AwayFromZero);
            var y = (int)Math.Round((1d - normalizedY) * control.ClientSize.Height, MidpointRounding.AwayFromZero);

            if (x < 0) x = 0;
            if (x >= control.ClientSize.Width) x = control.ClientSize.Width - 1;
            if (y < 0) y = 0;
            if (y >= control.ClientSize.Height) y = control.ClientSize.Height - 1;

            return new Point(x, y);
        }

        private static double Mid(double a, double b)
        {
            return (a + b) * 0.5d;
        }

        private sealed class TestEngineeringGraphControl : EngineeringGraphControl
        {
            public void RaiseMouseDown(MouseButtons button, Point location)
            {
                OnMouseDown(new MouseEventArgs(button, 1, location.X, location.Y, 0));
            }

            public void RaiseMouseUp(MouseButtons button, Point location)
            {
                OnMouseUp(new MouseEventArgs(button, 1, location.X, location.Y, 0));
            }

            public void RaiseMouseMove(Point location)
            {
                OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, location.X, location.Y, 0));
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
