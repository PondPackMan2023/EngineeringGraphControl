using Graphing.Core.Pie.Presentation;
using Graphing.TestScenarios.Scenarios;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

public sealed class PieGraphScenarioProvider : IPieGraphScenarioProvider
{
    private static readonly IReadOnlyList<PieScenarioOption> Scenarios =
    [
        new(PieScenarioId.BasicPie, "Basic Pie"),
        new(PieScenarioId.SpendingByCategory, "Spending By Category"),
        new(PieScenarioId.SingleSlice, "Single Slice"),
        new(PieScenarioId.ManySlices, "Many Slices"),
        new(PieScenarioId.PaletteRepeat, "Palette Repeat"),
        new(PieScenarioId.ZeroValueSlice, "Zero Value Slice"),
        new(PieScenarioId.AllZeroSlices, "All Zero Slices"),
        new(PieScenarioId.LegendHidden, "Legend Hidden")
    ];

    public IReadOnlyList<PieScenarioOption> GetAvailableScenarios()
    {
        return Scenarios;
    }

    public PieScenarioResult BuildScenario(PieScenarioId scenarioId)
    {
        return scenarioId switch
        {
            PieScenarioId.BasicPie => new PieScenarioResult(
                PieScenarioDefinitions.BuildBasicPie(),
                new PieGraphPresentationOptions(legendVisible: true)),
            PieScenarioId.SpendingByCategory => new PieScenarioResult(
                PieScenarioDefinitions.BuildSpendingByCategory(),
                new PieGraphPresentationOptions(legendVisible: true)),
            PieScenarioId.SingleSlice => new PieScenarioResult(
                PieScenarioDefinitions.BuildSingleSlice(),
                new PieGraphPresentationOptions(legendVisible: true)),
            PieScenarioId.ManySlices => new PieScenarioResult(
                PieScenarioDefinitions.BuildManySlices(),
                new PieGraphPresentationOptions(legendVisible: true)),
            PieScenarioId.PaletteRepeat => new PieScenarioResult(
                PieScenarioDefinitions.BuildPaletteRepeat(),
                new PieGraphPresentationOptions(legendVisible: true)),
            PieScenarioId.ZeroValueSlice => new PieScenarioResult(
                PieScenarioDefinitions.BuildZeroValueSlice(),
                new PieGraphPresentationOptions(legendVisible: true)),
            PieScenarioId.AllZeroSlices => new PieScenarioResult(
                PieScenarioDefinitions.BuildAllZeroSlices(),
                new PieGraphPresentationOptions(legendVisible: true)),
            PieScenarioId.LegendHidden => new PieScenarioResult(
                PieScenarioDefinitions.BuildLegendHidden(),
                new PieGraphPresentationOptions(legendVisible: false)),
            _ => new PieScenarioResult(
                PieScenarioDefinitions.BuildBasicPie(),
                new PieGraphPresentationOptions(legendVisible: true))
        };
    }
}
