using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
