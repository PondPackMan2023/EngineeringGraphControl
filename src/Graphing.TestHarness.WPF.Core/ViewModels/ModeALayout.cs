namespace Graphing.TestHarness.WPF.Core.ViewModels;

/// <summary>
/// Mode-owned layout for ModeA. Provides both a sidebar and main content.
/// Mirrors the Register mode in the MPL shell model.
/// </summary>
public sealed class ModeALayout : IHarnessModeLayout
{
    public ModeALayout(ModeASidebarViewModel sidebar, ModeAMainContentViewModel mainContent)
    {
        ArgumentNullException.ThrowIfNull(sidebar);
        ArgumentNullException.ThrowIfNull(mainContent);

        Sidebar = sidebar;
        MainContent = mainContent;
    }

    public IHarnessSidebarViewModel? Sidebar { get; }

    public IHarnessMainContentViewModel MainContent { get; }
}
