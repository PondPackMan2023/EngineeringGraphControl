using System;
using System.Globalization;
using System.Linq;
using Graphing.Controls.Presentation;
using Graphing.Controls.Snapshot;
using Graphing.TestScenarios.Libraries;
using Graphing.TestScenarios.Scenarios;
using NUnit.Framework;

namespace Graphing.Tests
{
    [TestFixture]
    public sealed class SemanticAxisLabelingScenarioTests
    {
        [Test]
        public void DateOnlyFormatter_FormatsDateOnlyDeterministically()
        {
            var formatter = new DateOnlyFormatter("date-only", "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var result = formatter.Format((object)new DateOnly(2024, 1, 15), CultureInfo.InvariantCulture);

            Assert.That(formatter.ValueType, Is.EqualTo(typeof(DateOnly)));
            Assert.That(result, Is.EqualTo("2024-01-15"));
        }

        [Test]
        public void ScenarioE_BuildsSuccessfully()
        {
            var graph = ScenarioDefinitions.BuildScenarioE();

            Assert.That(graph, Is.Not.Null);
            Assert.That(graph.Axes.Count, Is.EqualTo(2));
            Assert.That(graph.Series.Count, Is.EqualTo(1));
        }

        [Test]
        public void ScenarioE_ProducesSemanticLabels_WhilePreservingNumericGeometry()
        {
            var graph = ScenarioDefinitions.BuildScenarioE();
            var dateConverter = (DateOnlyDayNumberAxisLabelValueConverter)graph.Axes.Single(a => a.Id.Value == "date-axis").LabelValueConverter;
            var decimalConverter = (DecimalAxisLabelValueConverter)graph.Axes.Single(a => a.Id.Value == "balance-axis").LabelValueConverter;

            var snapshot = new GraphSnapshotBuilder().Build(graph);
            var presentation = new GraphPresentationModel(snapshot);

            var xAxis = presentation.Axes.Single(a => a.AxisId == "date-axis");
            var yAxis = presentation.Axes.Single(a => a.AxisId == "balance-axis");

            Assert.That(xAxis.Ticks.Count, Is.GreaterThan(0));
            Assert.That(yAxis.Ticks.Count, Is.GreaterThan(0));

            Assert.That(dateConverter.ReceivedCoordinates.Count, Is.EqualTo(xAxis.Ticks.Count));
            Assert.That(decimalConverter.ReceivedCoordinates.Count, Is.EqualTo(yAxis.Ticks.Count));

            for (int i = 0; i < xAxis.Ticks.Count; i++)
            {
                var tick = xAxis.Ticks[i];
                Assert.That(dateConverter.ReceivedCoordinates[i], Is.EqualTo(tick.Value).Within(1e-12));
                Assert.That(DateOnly.TryParseExact(tick.Label, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _), Is.True);
                Assert.That(tick.Start.X, Is.EqualTo(tick.Value).Within(1e-12));
                Assert.That(tick.End.X, Is.EqualTo(tick.Value).Within(1e-12));
            }

            for (int i = 0; i < yAxis.Ticks.Count; i++)
            {
                var tick = yAxis.Ticks[i];
                Assert.That(decimalConverter.ReceivedCoordinates[i], Is.EqualTo(tick.Value).Within(1e-12));
                Assert.That(tick.Label, Does.StartWith("$"));
                Assert.That(tick.Start.Y, Is.EqualTo(tick.Value).Within(1e-12));
                Assert.That(tick.End.Y, Is.EqualTo(tick.Value).Within(1e-12));
            }
        }
    }
}
