using Graphing.Controls.Models;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

public interface IGraphScenarioProvider
{
    IGraphModel? BuildScenario(GraphScenarioId scenarioId);
}
