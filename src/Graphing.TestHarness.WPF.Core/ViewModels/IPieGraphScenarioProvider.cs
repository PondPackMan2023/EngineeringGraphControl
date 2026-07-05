namespace Graphing.TestHarness.WPF.Core.ViewModels;

public interface IPieGraphScenarioProvider
{
    IReadOnlyList<PieScenarioOption> GetAvailableScenarios();

    PieScenarioResult BuildScenario(PieScenarioId scenarioId);
}
