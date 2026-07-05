using Graphing.Core.Pie.Models;
using Graphing.Core.Pie.Presentation;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

public sealed class PieScenarioResult
{
    public PieScenarioResult(IPieGraphModel pieGraphModel, PieGraphPresentationOptions pieGraphPresentationOptions)
    {
        PieGraphModel = pieGraphModel ?? throw new ArgumentNullException(nameof(pieGraphModel));
        PieGraphPresentationOptions = pieGraphPresentationOptions ?? throw new ArgumentNullException(nameof(pieGraphPresentationOptions));
    }

    public IPieGraphModel PieGraphModel { get; }

    public PieGraphPresentationOptions PieGraphPresentationOptions { get; }
}
