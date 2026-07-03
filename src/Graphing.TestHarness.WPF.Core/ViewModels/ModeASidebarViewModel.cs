using Graphing.TestHarness.WPF.Core.Navigation;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

/// <summary>
/// Sidebar view model for ModeA. Owns the navigation action to ModeB,
/// which lives in the SidebarRegion rather than the main content area.
/// </summary>
public sealed class ModeASidebarViewModel : IHarnessSidebarViewModel
{
    public ModeASidebarViewModel(ITestHarnessNavigationService navigationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);

        NavigateToModeBCommand = new RelayCommand(navigationService.NavigateToModeB);
    }

    public RelayCommand NavigateToModeBCommand { get; }
}
