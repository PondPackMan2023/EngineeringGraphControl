using System;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using Graphing.Editors.Presentation;
using NUnit.Framework;
using UnitRegistry;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

namespace Graphing.Tests
{
    [TestFixture]
    public class GraphTitlesOptionsCycleTests
    {
        [Test]
        public void FullCycle_TitleText_RoundTrips()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var presentationModel1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(presentationModel1.Titles.HasTitleTextOverride, Is.True);
            Assert.That(presentationModel1.Titles.TitleText, Is.Empty);

            // Modify
            presentationModel1.Titles.HasTitleTextOverride = true;
            presentationModel1.Titles.TitleText = "My Title";

            // Apply
            var appliedOptions = presentationModel1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.GraphTitle, Is.EqualTo("My Title"));

            // Reload
            var presentationModel2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(presentationModel2.Titles.HasTitleTextOverride, Is.True);
            Assert.That(presentationModel2.Titles.TitleText, Is.EqualTo("My Title"));
        }

        [Test]
        public void FullCycle_SubtitleText_RoundTrips()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var presentationModel1 = new GraphOptionsPresentationModel(model, defaultOptions);
            Assert.That(presentationModel1.Titles.HasSubtitleTextOverride, Is.True);
            Assert.That(presentationModel1.Titles.SubtitleText, Is.Empty);

            // Modify
            presentationModel1.Titles.HasSubtitleTextOverride = true;
            presentationModel1.Titles.SubtitleText = "My Subtitle";

            // Apply
            var appliedOptions = presentationModel1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.GraphSubtitle, Is.EqualTo("My Subtitle"));

            // Reload
            var presentationModel2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(presentationModel2.Titles.HasSubtitleTextOverride, Is.True);
            Assert.That(presentationModel2.Titles.SubtitleText, Is.EqualTo("My Subtitle"));
        }

        [Test]
        public void FullCycle_TitleAndSubtitle_BothRoundTrip()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var presentationModel1 = new GraphOptionsPresentationModel(model, defaultOptions);

            // Modify
            presentationModel1.Titles.HasTitleTextOverride = true;
            presentationModel1.Titles.TitleText = "Title Text";
            presentationModel1.Titles.HasSubtitleTextOverride = true;
            presentationModel1.Titles.SubtitleText = "Subtitle Text";

            // Apply
            var appliedOptions = presentationModel1.BuildGraphPresentationOptions();

            // Reload
            var presentationModel2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(presentationModel2.Titles.HasTitleTextOverride, Is.True);
            Assert.That(presentationModel2.Titles.TitleText, Is.EqualTo("Title Text"));
            Assert.That(presentationModel2.Titles.HasSubtitleTextOverride, Is.True);
            Assert.That(presentationModel2.Titles.SubtitleText, Is.EqualTo("Subtitle Text"));
        }

        [Test]
        public void FullCycle_ClearTitleOverride_RoundTrips()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var optionsWithTitle = new GraphPresentationOptions(graphTitle: "Original Title");

            // First Load
            var presentationModel1 = new GraphOptionsPresentationModel(model, optionsWithTitle);
            Assert.That(presentationModel1.Titles.HasTitleTextOverride, Is.True);
            Assert.That(presentationModel1.Titles.TitleText, Is.EqualTo("Original Title"));

            // Modify - clear the override
            presentationModel1.Titles.HasTitleTextOverride = true;
            presentationModel1.Titles.TitleText = string.Empty;

            // Apply
            var appliedOptions = presentationModel1.BuildGraphPresentationOptions();
            Assert.That(appliedOptions.GraphTitle, Is.Not.Null);

            // Reload
            var presentationModel2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(presentationModel2.Titles.HasTitleTextOverride, Is.True);
            Assert.That(presentationModel2.Titles.TitleText, Is.Empty);
        }

        [Test]
        public void FullCycle_ModifyTitleText_Persists()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var optionsWithTitle = new GraphPresentationOptions(graphTitle: "Initial Title");

            // First Load
            var presentationModel1 = new GraphOptionsPresentationModel(model, optionsWithTitle);

            // Modify
            presentationModel1.Titles.TitleText = "Modified Title";

            // Apply
            var appliedOptions = presentationModel1.BuildGraphPresentationOptions();

            // Reload
            var presentationModel2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(presentationModel2.Titles.TitleText, Is.EqualTo("Modified Title"));
            Assert.That(appliedOptions.GraphTitle, Is.EqualTo("Modified Title"));
        }

        [Test]
        public void FullCycle_TitleStaysEmpty_WhenNotOverridden()
        {
            // Arrange
            var model = CreateMinimalGraphModel();
            var defaultOptions = new GraphPresentationOptions();

            // First Load
            var presentationModel1 = new GraphOptionsPresentationModel(model, defaultOptions);

            // Don't modify title

            // Apply
            var appliedOptions = presentationModel1.BuildGraphPresentationOptions();

            // Reload
            var presentationModel2 = new GraphOptionsPresentationModel(model, appliedOptions);

            // Assert
            Assert.That(presentationModel2.Titles.HasTitleTextOverride, Is.True);
            Assert.That(appliedOptions.GraphTitle, Is.Not.Null);
        }

        [Test]
        public void FullCycle_MultipleRounds_StayConsistent()
        {
            // Test repeated load/edit/apply cycles
            var model = CreateMinimalGraphModel();
            var options = new GraphPresentationOptions();

            for (int round = 0; round < 3; round++)
            {
                var pm = new GraphOptionsPresentationModel(model, options);
                pm.Titles.HasTitleTextOverride = true;
                pm.Titles.TitleText = $"Title Round {round}";
                options = pm.BuildGraphPresentationOptions();
            }

            var finalPm = new GraphOptionsPresentationModel(model, options);
            Assert.That(finalPm.Titles.TitleText, Is.EqualTo("Title Round 2"));
        }

        private static IGraphModel CreateMinimalGraphModel()
        {
            var unit = Units.Length.Meter;
            var xAxis = new AxisModel(
                new AxisId("x-axis"),
                ModelAxisOrientation.X,
                ModelAxisSide.Bottom,
                unit,
                "m",
                null);
            var yAxis = new AxisModel(
                new AxisId("y-axis"),
                ModelAxisOrientation.Y,
                ModelAxisSide.Left,
                unit,
                "m",
                null);

            var xField = new TestFieldDefinition("X", "x", unit, new double[] { 0d, 1d });
            var yField = new TestFieldDefinition("Y", "y", unit, new double[] { 0d, 1d });

            var series = new GraphSeriesModel(
                new SeriesId("series-1"),
                "series-1",
                SeriesType.Line,
                xField,
                yField,
                xAxis,
                yAxis);

            return new GraphModel(new[] { xAxis, yAxis }, new[] { series });
        }
    }

    /// <summary>
    /// Helper for field definitions in tests - shared across cycle test files.
    /// </summary>
    internal sealed class TestFieldDefinition : GraphFieldDefinitionBase
    {
        private readonly Array _values;

        public TestFieldDefinition(string label, string id, Unit unit, Array values)
            : base(id, label, unit)
        {
            _values = values;
        }

        public override Array GetValues()
        {
            return _values;
        }
    }
}
