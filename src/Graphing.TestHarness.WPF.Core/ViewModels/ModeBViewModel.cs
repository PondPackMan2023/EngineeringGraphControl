using Graphing.Controls.Models;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

public sealed class ModeBViewModel
{
    private readonly IGraphScenarioProvider _scenarioProvider;

    public ModeBViewModel(IGraphScenarioProvider scenarioProvider)
    {
        _scenarioProvider = scenarioProvider ?? throw new ArgumentNullException(nameof(scenarioProvider));
    }

    public IGraphModel? GraphModel { get; private set; }

    public bool ZoomEnabled { get; set; } = true;

    public int ZoomExtentsRequestVersion { get; private set; }

    public void Activate()
    {
        GraphModel = _scenarioProvider.BuildScenario(GraphScenarioId.A);
    }

    public void RequestZoomExtents()
    {
        ZoomExtentsRequestVersion++;
    }
}
