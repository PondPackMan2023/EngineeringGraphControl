using Dev.Core.Services.Mode;
using Dev.Core.Toolbar;
using Graphing.TestHarness.WPF.Core.ViewModels;

namespace Graphing.TestHarness.WPF.Core.Navigation;

public sealed class ModeBFeatureMode : IFeatureMode
{
    public const string ModeIdValue = "Harness.ModeB";

    private readonly ModeBMainContentViewModel _viewModel;

    public ModeBFeatureMode(ModeBMainContentViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    public string ModeId => ModeIdValue;

    public ToolbarId? PrimaryToolbarId => null;

    public void OnEnter()
    {
        _viewModel.Activate();
    }

    public void OnExit()
    {
    }

    public Task<bool> TryApplyAsync()
    {
        return Task.FromResult(true);
    }

    public void Cancel()
    {
    }
}
