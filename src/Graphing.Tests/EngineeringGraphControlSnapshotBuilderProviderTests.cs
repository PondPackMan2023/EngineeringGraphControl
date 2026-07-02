using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Graphing.Tests
{
    [TestFixture]
    public class EngineeringGraphControlSnapshotBuilderProviderTests
    {
        [Test]
        public void SetGraphSource_WhenProviderSupplied_UsesProviderBuilder()
        {
            using (var control = new EngineeringGraphControl())
            {
                _ = control.Handle;
                control.Size = new Size(640, 480);

                var model = CreateSimpleGraphModel();
                var options = new GraphPresentationOptions();
                var spyBuilder = new SpyGraphSnapshotBuilder();
                var provider = new SpyGraphSnapshotBuilderProvider(spyBuilder);

                control.SetGraphSource(model, options, provider);

                Assert.That(provider.CreateCallCount, Is.EqualTo(1));
                Assert.That(spyBuilder.BuildCallCount, Is.EqualTo(1));
                Assert.That(spyBuilder.LastGraphModel, Is.SameAs(model));
                Assert.That(spyBuilder.LastOptions, Is.Not.Null);
                Assert.That(control.ActiveSnapshot, Is.Not.Null);
            }
        }

        private static IGraphModel CreateSimpleGraphModel()
        {
            var unit = Units.Length.Meter;
            var registry = UnitsRegistry.Default;

            var xAxis = new AxisModel(new AxisId("x-axis"), ModelAxisOrientation.X, ModelAxisSide.Bottom, unit, "m", new NumericFormatter("fmt-x", registry, "X", "F2"));
            var yAxis = new AxisModel(new AxisId("y-axis"), ModelAxisOrientation.Y, ModelAxisSide.Left, unit, "m", new NumericFormatter("fmt-y", registry, "Y", "F2"));

            var xField = new TestFieldDefinition("X", "x", unit, new[] { 0d, 1d, 2d, 3d });
            var yField = new TestFieldDefinition("Y", "y", unit, new[] { 10d, 20d, 30d, 40d });
            var series = new GraphSeriesModel(new SeriesId("s1"), "s1", SeriesType.Line, xField, yField, xAxis, yAxis);

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

        private sealed class SpyGraphSnapshotBuilderProvider : IGraphSnapshotBuilderProvider
        {
            private readonly IGraphSnapshotBuilder _builder;

            public SpyGraphSnapshotBuilderProvider(IGraphSnapshotBuilder builder)
            {
                _builder = builder;
            }

            public int CreateCallCount { get; private set; }

            public IGraphSnapshotBuilder CreateGraphSnapshotBuilder()
            {
                CreateCallCount++;
                return _builder;
            }
        }

        private sealed class SpyGraphSnapshotBuilder : IGraphSnapshotBuilder
        {
            private readonly IGraphSnapshotBuilder _inner = new GraphSnapshotBuilder();

            public int BuildCallCount { get; private set; }

            public IGraphModel LastGraphModel { get; private set; }

            public GraphPresentationOptions LastOptions { get; private set; }

            public IGraphSnapshot Build(IGraphModel graphModel, GraphPresentationOptions options = null)
            {
                BuildCallCount++;
                LastGraphModel = graphModel;
                LastOptions = options;
                return _inner.Build(graphModel, options);
            }
        }
    }
}
