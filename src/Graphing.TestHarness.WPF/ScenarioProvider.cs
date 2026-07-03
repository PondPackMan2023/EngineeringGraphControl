#nullable enable

using Graphing.Controls.Models;
using Graphing.TestHarness.WPF.Core.ViewModels;
using Graphing.TestScenarios.Scenarios;

namespace Graphing.TestHarness.WPF;

internal sealed class ScenarioProvider : IGraphScenarioProvider
{
    public IGraphModel? BuildScenario(GraphScenarioId scenarioId)
    {
        return scenarioId switch
        {
            GraphScenarioId.A => ScenarioDefinitions.BuildScenarioA(),
            GraphScenarioId.B => ScenarioDefinitions.BuildScenarioB(),
            GraphScenarioId.C => ScenarioDefinitions.BuildScenarioC(),
            GraphScenarioId.D => ScenarioDefinitions.BuildScenarioD(),
            GraphScenarioId.E => ScenarioDefinitions.BuildScenarioE(),
            _ => ScenarioDefinitions.BuildScenarioA()
        };
    }
}
