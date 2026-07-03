namespace Graphing.TestHarness.WPF.Core.ViewModels;

/// <summary>
/// Mode-owned layout for ModeB. Provides main content only; the sidebar
/// is absent (null). Mirrors the Balance Forecast mode in the MPL shell model.
/// </summary>
public sealed class ModeBLayout : IHarnessModeLayout
{
    public ModeBLayout(ModeBMainContentViewModel mainContent)
    {
        ArgumentNullException.ThrowIfNull(mainContent);

        MainContent = mainContent;
    }

    /// <inheritdoc/>
    /// <remarks>ModeB does not own a sidebar. The SidebarRegion collapses.</remarks>
    public IHarnessSidebarViewModel? Sidebar => null;

    public IHarnessMainContentViewModel MainContent { get; }
}
