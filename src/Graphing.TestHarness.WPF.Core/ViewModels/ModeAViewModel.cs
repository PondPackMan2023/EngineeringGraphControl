using Graphing.TestHarness.WPF.Core.Navigation;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

public sealed class ModeAViewModel
{
    public ModeAViewModel(ITestHarnessNavigationService navigationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);

        NavigateToModeBCommand = new RelayCommand(navigationService.NavigateToModeB);
    }

    public RelayCommand NavigateToModeBCommand { get; }
}
