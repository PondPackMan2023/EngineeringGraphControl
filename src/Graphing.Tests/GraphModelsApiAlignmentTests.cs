using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
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
                Is.EqualTo(new[] { "Label", "SeriesId", "SeriesType", "XAxis", "XField", "YAxis", "YField" }));
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
                    "Formatter", "Id", "IsAutoRange", "MaximumValue", "MinimumValue", "Orientation", "ScaleType", "Side", "Unit", "UnitLabel"
                }));
        }

        [Test]
        public void ModelFixture_CanRepresentUnitNoneAndUnitlessSemantics_Explicitly()
        {
            var noneDimension = new Dimension("none");
            var unitlessDimension = new Dimension("unitless");
            var unitNone = new Unit("none", noneDimension, 1.0, "");
            var unitUnitless = new Unit("unitless", unitlessDimension, 1.0, "");

            var registry = new UnitsRegistry();
            registry.RegisterBaseUnit(unitNone);
            registry.RegisterBaseUnit(unitUnitless);
            registry.Freeze();

            var formatter = new NumericFormatter("unitless-formatter", registry, " ", "F2");

            var noneField = new TestFieldDefinition("Point Count", "pointCount", unitNone, new[] { 1d, 2d, 3d });
            var unitlessField = new TestFieldDefinition("Efficiency", "efficiency", unitUnitless, new[] { 0.12, 0.34, 0.56 });

            var xAxis = new TestAxisModel(new AxisId("x"), AxisOrientation.X, AxisSide.Bottom, unitNone, "", null, AxisScaleType.Linear, true, null, null);
            var yAxis = new TestAxisModel(new AxisId("y"), AxisOrientation.Y, AxisSide.Left, unitUnitless, "ratio", formatter, AxisScaleType.Linear, true, null, null);

            var series = new TestSeriesModel(1, "series", SeriesType.Line, noneField, unitlessField, xAxis, yAxis);
            var graph = new TestGraphModel(new[] { xAxis, yAxis }, new[] { series });

            Assert.That(graph.Axes.Count, Is.EqualTo(2));
            Assert.That(graph.Series.Count, Is.EqualTo(1));
            Assert.That(graph.Series[0].XField.Unit, Is.EqualTo(unitNone));
            Assert.That(graph.Series[0].YField.Unit, Is.EqualTo(unitUnitless));
            Assert.That(graph.Series[0].YAxis.Formatter, Is.SameAs(formatter));
            Assert.That(graph.Series[0].XAxis.Formatter, Is.Null);
        }

        [Test]
        public void ChangeAxisFormat_ReplacesFormatter_AndPreservesUnit_ForBothAxes()
        {
            var registry = UnitsRegistry.Default;
            var unit = Units.Length.Meter;
            var xAxisId = new AxisId("x");
            var yAxisId = new AxisId("y");
            var xFormatter = new NumericFormatter("x-fmt", registry, " ", "F1");
            var yFormatter = new NumericFormatter("y-fmt", registry, " ", "F2");

            var xAxis = new AxisModel(xAxisId, AxisOrientation.X, AxisSide.Bottom, unit, "m", null);
            var yAxis = new AxisModel(yAxisId, AxisOrientation.Y, AxisSide.Left, unit, "m", null);
            var graph = new GraphModel(new[] { xAxis, yAxis }, new IGraphSeriesModel[0]);

            var withXFormat = graph.ChangeAxisFormat(xAxisId, xFormatter);
            var withYFormat = withXFormat.ChangeAxisFormat(yAxisId, yFormatter);

            var resultingXAxis = withYFormat.Axes.Single(a => a.Id.Equals(xAxisId));
            var resultingYAxis = withYFormat.Axes.Single(a => a.Id.Equals(yAxisId));

            Assert.That(resultingXAxis.Formatter, Is.SameAs(xFormatter));
            Assert.That(resultingYAxis.Formatter, Is.SameAs(yFormatter));
            Assert.That(resultingXAxis.Unit, Is.SameAs(unit));
            Assert.That(resultingYAxis.Unit, Is.SameAs(unit));
        }

        [Test]
        public void ChangeAxisUnitAndFormat_UpdatesBothAtomically_WithoutIntermediateStateExposure()
        {
            var registry = UnitsRegistry.Default;
            var timeDimension = new Dimension("time");
            var hoursUnit = new Unit("hr", timeDimension, 3600.0, "hr");
            var secondsUnit = new Unit("s", timeDimension, 1.0, "s");
            var formatter = new NumericFormatter("seconds-fmt", registry, "Time", "F3");

            var xAxisId = new AxisId("x");
            var xAxis = new AxisModel(xAxisId, AxisOrientation.X, AxisSide.Bottom, hoursUnit, "hr", null);
            var yAxis = new AxisModel(new AxisId("y"), AxisOrientation.Y, AxisSide.Left, hoursUnit, "hr", null);
            var graph = new GraphModel(new[] { xAxis, yAxis }, new IGraphSeriesModel[0]);

            var updatedGraph = graph.ChangeAxisUnitAndFormat(xAxisId, secondsUnit, formatter);

            var originalXAxis = graph.Axes.Single(a => a.Id.Equals(xAxisId));
            var updatedXAxis = updatedGraph.Axes.Single(a => a.Id.Equals(xAxisId));

            Assert.That(originalXAxis.Unit, Is.SameAs(hoursUnit));
            Assert.That(originalXAxis.Formatter, Is.Null);
            Assert.That(updatedXAxis.Unit, Is.SameAs(secondsUnit));
            Assert.That(updatedXAxis.Formatter, Is.SameAs(formatter));
            Assert.That(updatedXAxis.UnitLabel, Is.EqualTo("s"));
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

            public IGraphModel ChangeAxisUnit(AxisId axisId, Unit unit)
            {
                var changes = new Dictionary<AxisId, Unit>();
                changes[axisId] = unit;
                return ChangeAxisUnits(changes);
            }

            public IGraphModel ChangeAxisFormat(AxisId axisId, IValueFormatter formatter)
            {
                var updatedAxes = new List<IAxisModel>(Axes.Count);

                for (var axisIndex = 0; axisIndex < Axes.Count; axisIndex++)
                {
                    var axis = Axes[axisIndex];
                    if (axis == null)
                    {
                        updatedAxes.Add(null);
                        continue;
                    }

                    if (!axis.Id.Equals(axisId))
                    {
                        updatedAxes.Add(axis);
                        continue;
                    }

                    updatedAxes.Add(
                        new TestAxisModel(
                            axis.Id,
                            axis.Orientation,
                            axis.Side,
                            axis.Unit,
                            axis.UnitLabel,
                            formatter,
                            axis.ScaleType,
                            axis.IsAutoRange,
                            axis.MinimumValue,
                            axis.MaximumValue));
                }

                return new TestGraphModel(updatedAxes, Series);
            }

            public IGraphModel ChangeAxisUnitAndFormat(AxisId axisId, Unit unit, IValueFormatter formatter)
            {
                var updatedAxes = new List<IAxisModel>(Axes.Count);

                for (var axisIndex = 0; axisIndex < Axes.Count; axisIndex++)
                {
                    var axis = Axes[axisIndex];
                    if (axis == null)
                    {
                        updatedAxes.Add(null);
                        continue;
                    }

                    if (!axis.Id.Equals(axisId))
                    {
                        updatedAxes.Add(axis);
                        continue;
                    }

                    var newUnitLabel = unit != null && unit.Id != null ? unit.Id.Value : null;

                    updatedAxes.Add(
                        new TestAxisModel(
                            axis.Id,
                            axis.Orientation,
                            axis.Side,
                            unit,
                            newUnitLabel,
                            formatter,
                            axis.ScaleType,
                            axis.IsAutoRange,
                            axis.MinimumValue,
                            axis.MaximumValue));
                }

                return new TestGraphModel(updatedAxes, Series);
            }

            public IGraphModel ChangeAxisUnits(IReadOnlyDictionary<AxisId, Unit> unitChanges)
            {
                var updatedAxes = new List<IAxisModel>(Axes.Count);

                for (var axisIndex = 0; axisIndex < Axes.Count; axisIndex++)
                {
                    var axis = Axes[axisIndex];
                    if (axis == null)
                    {
                        updatedAxes.Add(null);
                        continue;
                    }

                    var replacementUnit = default(Unit);
                    var hasReplacement = unitChanges != null
                        && unitChanges.TryGetValue(axis.Id, out replacementUnit);

                    if (!hasReplacement)
                    {
                        updatedAxes.Add(axis);
                        continue;
                    }

                    updatedAxes.Add(
                        new TestAxisModel(
                            axis.Id,
                            axis.Orientation,
                            axis.Side,
                            replacementUnit,
                            axis.UnitLabel,
                                axis.Formatter,
                            axis.ScaleType,
                            axis.IsAutoRange,
                            axis.MinimumValue,
                            axis.MaximumValue));
                }

                return new TestGraphModel(updatedAxes, Series);
            }
        }

        private sealed class TestSeriesModel : IGraphSeriesModel
        {
            public TestSeriesModel(
                int id,
                string label,
                SeriesType seriesType,
                IGraphFieldDefinition xField,
                IGraphFieldDefinition yField,
                IAxisModel xAxis,
                IAxisModel yAxis)
            {
                SeriesId = new SeriesId($"{id}");
                Label = label;
                SeriesType = seriesType;
                XField = xField;
                YField = yField;
                XAxis = xAxis;
                YAxis = yAxis;
            }

            public SeriesId SeriesId { get; }

            public string Label { get; }

            public SeriesType SeriesType { get; }

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
                AxisId id,
                AxisOrientation orientation,
                AxisSide side,
                Unit unit,
                string unitLabel,
                   IValueFormatter formatter,
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
                   Formatter = formatter;
                ScaleType = scaleType;
                IsAutoRange = isAutoRange;
                MinimumValue = minimumValue;
                MaximumValue = maximumValue;
            }

            public AxisId Id { get; }

            public AxisOrientation Orientation { get; }

            public AxisSide Side { get; }

            public Unit Unit { get; }

            public string UnitLabel { get; }

            public IValueFormatter Formatter { get; }

            public AxisScaleType ScaleType { get; }

            public bool IsAutoRange { get; }

            public double? MinimumValue { get; }

            public double? MaximumValue { get; }

            public IAxisModel ChangeUnit(Unit newUnit)
            {
                return new TestAxisModel(
                    Id,
                    Orientation,
                    Side,
                    newUnit,
                    UnitLabel,
                       Formatter,
                    ScaleType,
                    IsAutoRange,
                    MinimumValue,
                    MaximumValue);
            }

            public IAxisModel ChangeFormat(IValueFormatter newFormatter)
            {
                return new TestAxisModel(
                    Id,
                    Orientation,
                    Side,
                    Unit,
                    UnitLabel,
                    newFormatter,
                    ScaleType,
                    IsAutoRange,
                    MinimumValue,
                    MaximumValue);
            }
        }
    }
}
