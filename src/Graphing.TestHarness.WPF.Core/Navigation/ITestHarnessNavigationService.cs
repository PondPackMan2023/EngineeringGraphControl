namespace Graphing.TestHarness.WPF.Core.Navigation;

public interface ITestHarnessNavigationService
{
    ModeHostState CurrentState { get; }

    event EventHandler<ModeNavigationChangedEventArgs>? NavigationChanged;

    void NavigateToModeA();

    void NavigateToModeB();
}
