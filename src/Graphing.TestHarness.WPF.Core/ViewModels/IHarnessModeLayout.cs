namespace Graphing.TestHarness.WPF.Core.ViewModels;

/// <summary>
/// Represents the mode-owned layout contract. Each active mode provides its own
/// sidebar content and main content. The shell projects these independently into
/// SidebarRegion and MainContentRegion.
/// </summary>
public interface IHarnessModeLayout
{
    /// <summary>
    /// The view model to project into the SidebarRegion, or null if the active
    /// mode does not own a sidebar.
    /// </summary>
    IHarnessSidebarViewModel? Sidebar { get; }

    /// <summary>
    /// The view model to project into the MainContentRegion.
    /// </summary>
    IHarnessMainContentViewModel MainContent { get; }
}
