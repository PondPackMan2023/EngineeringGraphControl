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