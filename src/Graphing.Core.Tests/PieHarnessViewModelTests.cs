using System.Linq;
using Graphing.TestHarness.WPF.Core.ViewModels;
using NUnit.Framework;

namespace Graphing.Core.Tests
{
    [TestFixture]
    public class PieHarnessViewModelTests
    {
        [Test]
        public void Provider_ExposesRequiredScenarios()
        {
            var provider = new PieGraphScenarioProvider();

            var scenarios = provider.GetAvailableScenarios();

            Assert.That(scenarios.Count, Is.EqualTo(8));
            Assert.That(scenarios.Select(s => s.Id), Is.EquivalentTo(new[]
            {
                PieScenarioId.BasicPie,
                PieScenarioId.SpendingByCategory,
                PieScenarioId.SingleSlice,
                PieScenarioId.ManySlices,
                PieScenarioId.PaletteRepeat,
                PieScenarioId.ZeroValueSlice,
                PieScenarioId.AllZeroSlices,
                PieScenarioId.LegendHidden
            }));
        }

        [Test]
        public void ApplySelectedScenario_AssignsModelAndPresentationOptions()
        {
            var viewModel = new PieHarnessViewModel(new PieGraphScenarioProvider())
            {
                SelectedPieScenario = new PieScenarioOption(PieScenarioId.SpendingByCategory, "Spending By Category")
            };

            viewModel.ApplySelectedScenario();

            Assert.That(viewModel.CurrentPieGraphModel, Is.Not.Null);
            Assert.That(viewModel.CurrentPieGraphModel.Slices.Count, Is.GreaterThan(0));
            Assert.That(viewModel.CurrentPieGraphPresentationOptions, Is.Not.Null);
            Assert.That(viewModel.CurrentPieGraphPresentationOptions.LegendVisible, Is.True);
        }

        [Test]
        public void ApplySelectedScenario_LegendHiddenScenario_DisablesLegend()
        {
            var viewModel = new PieHarnessViewModel(new PieGraphScenarioProvider())
            {
                SelectedPieScenario = new PieScenarioOption(PieScenarioId.LegendHidden, "Legend Hidden"),
            };

            viewModel.ApplySelectedScenario();

            Assert.That(viewModel.CurrentPieGraphModel, Is.Not.Null);
            Assert.That(viewModel.CurrentPieGraphPresentationOptions, Is.Not.Null);
            Assert.That(viewModel.CurrentPieGraphPresentationOptions.LegendVisible, Is.True);
        }
    }
}
