using System.ComponentModel;
using System.Runtime.CompilerServices;
using Graphing.TestHarness.WPF.Core.Navigation;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ModeALayout _modeALayout;
    private readonly ModeBLayout _modeBLayout;
    private IHarnessModeLayout _currentLayout;
    private int _toolbarRebuildVersion;

    public MainViewModel(
        ModeALayout modeALayout,
        ModeBLayout modeBLayout,
        ITestHarnessNavigationService navigationService,
        ToolbarViewModel toolbar)
    {
        _modeALayout = modeALayout ?? throw new ArgumentNullException(nameof(modeALayout));
        _modeBLayout = modeBLayout ?? throw new ArgumentNullException(nameof(modeBLayout));
        NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        Toolbar = toolbar ?? throw new ArgumentNullException(nameof(toolbar));

        _currentLayout = _modeALayout;
        Toolbar.RebuildForMode(NavigationService.CurrentState);

        NavigationService.NavigationChanged += OnNavigationChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The mode-owned layout currently active. The shell projects
    /// Sidebar and MainContent from this layout into their respective regions.
    /// </summary>
    public IHarnessModeLayout CurrentLayout
    {
        get => _currentLayout;
        private set
        {
            if (ReferenceEquals(_currentLayout, value))
            {
                return;
            }

            _currentLayout = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SidebarViewModel));
            OnPropertyChanged(nameof(MainContentViewModel));
        }
    }

    /// <summary>
    /// The view model projected into the SidebarRegion. Null when the active
    /// mode does not own a sidebar, causing the region to collapse.
    /// </summary>
    public IHarnessSidebarViewModel? SidebarViewModel => _currentLayout.Sidebar;

    /// <summary>
    /// The view model projected into the MainContentRegion.
    /// </summary>
    public IHarnessMainContentViewModel MainContentViewModel => _currentLayout.MainContent;

    public int ToolbarRebuildVersion
    {
        get => _toolbarRebuildVersion;
        private set
        {
            if (_toolbarRebuildVersion == value)
            {
                return;
            }

            _toolbarRebuildVersion = value;
            OnPropertyChanged();
        }
    }

    public ToolbarViewModel Toolbar { get; }

    public ITestHarnessNavigationService NavigationService { get; }

    private void OnNavigationChanged(object? sender, ModeNavigationChangedEventArgs e)
    {
        CurrentLayout = e.Current == ModeHostState.ModeA
            ? _modeALayout
            : _modeBLayout;

        Toolbar.RebuildForMode(e.Current);
        ToolbarRebuildVersion++;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
