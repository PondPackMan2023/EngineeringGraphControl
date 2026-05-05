using System;
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

        private sealed class TestEngineeringGraphControl : EngineeringGraphControl
        {
            public void RaisePaint(Graphics graphics)
            {
                OnPaint(new PaintEventArgs(graphics, new Rectangle(0, 0, Width, Height)));
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
