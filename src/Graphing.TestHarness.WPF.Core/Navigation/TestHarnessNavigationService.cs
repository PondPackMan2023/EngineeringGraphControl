using Dev.Core.Services.Mode;

namespace Graphing.TestHarness.WPF.Core.Navigation;

public sealed class TestHarnessNavigationService : ITestHarnessNavigationService
{
    private readonly IModeService _modeService;
    private readonly ModeBFeatureMode _modeBFeatureMode;
    private ModeHostState _currentState = ModeHostState.ModeA;

    public TestHarnessNavigationService(IModeService modeService, ModeBFeatureMode modeBFeatureMode)
    {
        _modeService = modeService ?? throw new ArgumentNullException(nameof(modeService));
        _modeBFeatureMode = modeBFeatureMode ?? throw new ArgumentNullException(nameof(modeBFeatureMode));

        _modeService.FeatureModeChanged += OnFeatureModeChanged;
    }

    public ModeHostState CurrentState => _currentState;

    public event EventHandler<ModeNavigationChangedEventArgs>? NavigationChanged;

    public void NavigateToModeA()
    {
        _modeService.ExitFeatureMode();
    }

    public void NavigateToModeB()
    {
        if (_modeService.ActiveFeatureMode is not null)
        {
            if (string.Equals(_modeService.ActiveFeatureMode.ModeId, ModeBFeatureMode.ModeIdValue, StringComparison.Ordinal))
            {
                return;
            }

            _modeService.ExitFeatureMode();
        }

        _modeService.EnterFeatureMode(_modeBFeatureMode);
    }

    private void OnFeatureModeChanged(object? sender, FeatureModeChangedEventArgs e)
    {
        var previous = _currentState;
        _currentState = e.IsEntering
            ? ModeHostState.ModeB
            : ModeHostState.ModeA;

        if (previous == _currentState)
        {
            return;
        }

        NavigationChanged?.Invoke(this, new ModeNavigationChangedEventArgs(previous, _currentState));
    }
}
