using Graphing.Controls.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Tests
{

    [TestFixture]
    public class GraphModelsApiAlignmentTests
    {
        [Test]
        public void GraphModel_ExposesOnlyAxesAndSeriesOwnership()
        {
            var propertyNames = typeof(IGraphModel)
                .GetProperties()
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToArray();

            Assert.That(propertyNames, Is.EqualTo(new[] { "Axes", "Series" }));
        }

        [Test]
        public void GraphFieldDefinition_HasNoAxisOrSeriesProperties()
        {
            var propertyNames = typeof(IGraphFieldDefinition)
                .GetProperties()
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToArray();

            Assert.That(propertyNames, Is.EqualTo(new[] { "Label", "Name", "Unit" }));

            var methodNames = typeof(IGraphFieldDefinition)
                .GetMethods()
                .Where(m => !m.IsSpecialName)
                .Select(m => m.Name)
                .OrderBy(n => n)
                .ToArray();

            Assert.That(methodNames, Is.EqualTo(new[] { "GetValues" }));
        }

        [Test]
        public void SeriesModel_RequiresExplicitXYFieldsAndAxes()
        {
            var propertyNames = typeof(IGraphSeriesModel)
                .GetProperties()
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToArray();

            Assert.That(
                propertyNames,
                Is.EqualTo(new[] { "ChartType", "Id", "Identifier", "Label", "XAxis", "XField", "YAxis", "YField" }));
        }

        [Test]
        public void AxisModel_UsesOrientationSideIdentityAndFormatter()
        {
            var propertyNames = typeof(IAxisModel)
                .GetProperties()
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToArray();

            Assert.That(
                propertyNames,
                Is.EqualTo(new[]
                {
                "Id", "IsAutoRange", "MaximumValue", "MinimumValue", "NumericFormatter", "Orientation", "ScaleType", "Side", "Unit", "UnitLabel"
                }));
        }

        [Test]
        public void ModelFixture_CanRepresentUnitNoneAndUnitlessSemantics_Explicitly()
        {
            var noneDimension = new Dimension("none");
            var unitlessDimension = new Dimension("unitless");
            var unitNone = new Unit("none", noneDimension, 1.0);
            var unitUnitless = new Unit("unitless", unitlessDimension, 1.0);

            var registry = new UnitRegistry.UnitRegistry();
            registry.RegisterBaseUnit(unitNone);
            registry.RegisterBaseUnit(unitUnitless);
            registry.Freeze();

            var formatter = new NumericFormatter("unitless-formatter", registry, "F2");

            var noneField = new TestFieldDefinition("Point Count", "pointCount", unitNone, new[] { 1d, 2d, 3d });
            var unitlessField = new TestFieldDefinition("Efficiency", "efficiency", unitUnitless, new[] { 0.12, 0.34, 0.56 });

            var xAxis = new TestAxisModel("x", AxisOrientation.X, AxisSide.Bottom, unitNone, "", null, AxisScaleType.Linear, true, null, null);
            var yAxis = new TestAxisModel("y", AxisOrientation.Y, AxisSide.Left, unitUnitless, "ratio", formatter, AxisScaleType.Linear, true, null, null);

            var series = new TestSeriesModel(1, "series", ChartType.Line, noneField, unitlessField, xAxis, yAxis);
            var graph = new TestGraphModel(new[] { xAxis, yAxis }, new[] { series });

            Assert.That(graph.Axes.Count, Is.EqualTo(2));
            Assert.That(graph.Series.Count, Is.EqualTo(1));
            Assert.That(graph.Series[0].XField.Unit, Is.EqualTo(unitNone));
            Assert.That(graph.Series[0].YField.Unit, Is.EqualTo(unitUnitless));
            Assert.That(graph.Series[0].YAxis.NumericFormatter, Is.SameAs(formatter));
            Assert.That(graph.Series[0].XAxis.NumericFormatter, Is.Null);
        }

        private sealed class TestGraphModel : IGraphModel
        {
            public TestGraphModel(IReadOnlyList<IAxisModel> axes, IReadOnlyList<IGraphSeriesModel> series)
            {
                Axes = axes;
                Series = series;
            }

            public IReadOnlyList<IAxisModel> Axes { get; }

            public IReadOnlyList<IGraphSeriesModel> Series { get; }
        }

        private sealed class TestSeriesModel : IGraphSeriesModel
        {
            public TestSeriesModel(
                int id,
                string label,
                ChartType chartType,
                IGraphFieldDefinition xField,
                IGraphFieldDefinition yField,
                IAxisModel xAxis,
                IAxisModel yAxis)
            {
                Identifier = "series-" + id;
                Id = id;
                Label = label;
                ChartType = chartType;
                XField = xField;
                YField = yField;
                XAxis = xAxis;
                YAxis = yAxis;
            }

            public object Identifier { get; }

            public int Id { get; }

            public string Label { get; }

            public ChartType ChartType { get; }

            public IGraphFieldDefinition XField { get; }

            public IGraphFieldDefinition YField { get; }

            public IAxisModel XAxis { get; }

            public IAxisModel YAxis { get; }
        }

        private sealed class TestFieldDefinition : IGraphFieldDefinition
        {
            private readonly Array _values;

            public TestFieldDefinition(string label, string name, Unit unit, Array values)
            {
                Label = label;
                Name = name;
                Unit = unit;
                _values = values;
            }

            public string Label { get; }

            public string Name { get; }

            public Unit Unit { get; }

            public Array GetValues()
            {
                return _values;
            }
        }

        private sealed class TestAxisModel : IAxisModel
        {
            public TestAxisModel(
                string id,
                AxisOrientation orientation,
                AxisSide side,
                Unit unit,
                string unitLabel,
                NumericFormatter formatter,
                AxisScaleType scaleType,
                bool isAutoRange,
                double? minimumValue,
                double? maximumValue)
            {
                Id = id;
                Orientation = orientation;
                Side = side;
                Unit = unit;
                UnitLabel = unitLabel;
                NumericFormatter = formatter;
                ScaleType = scaleType;
                IsAutoRange = isAutoRange;
                MinimumValue = minimumValue;
                MaximumValue = maximumValue;
            }

            public string Id { get; }

            public AxisOrientation Orientation { get; }

            public AxisSide Side { get; }

            public Unit Unit { get; }

            public string UnitLabel { get; }

            public NumericFormatter NumericFormatter { get; }

            public AxisScaleType ScaleType { get; }

            public bool IsAutoRange { get; }

            public double? MinimumValue { get; }

            public double? MaximumValue { get; }
        }
    }
}
