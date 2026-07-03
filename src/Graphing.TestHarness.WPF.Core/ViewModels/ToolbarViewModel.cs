using System.Collections.ObjectModel;
using System.Linq;
using Dev.Core.Toolbar;
using Dev.Core.Services;
using Graphing.TestHarness.WPF.Core.Navigation;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

public sealed class ToolbarViewModel
{
    private static readonly ToolbarId HarnessToolbarId = new("Harness.Primary");
    private static readonly ToolbarItemId ModeAItemId = new("Harness.Toolbar.ModeA");

    private readonly IToolbarRegistryService _toolbarRegistry;
    private readonly ITestHarnessNavigationService _navigationService;

    public ToolbarViewModel(IToolbarRegistryService toolbarRegistry, ITestHarnessNavigationService navigationService)
    {
        _toolbarRegistry = toolbarRegistry ?? throw new ArgumentNullException(nameof(toolbarRegistry));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        PrimaryToolbarItems = [];
        NavigateToModeACommand = new RelayCommand(_navigationService.NavigateToModeA);

        RegisterToolbarDefinition();
    }

    public ToolbarId PrimaryToolbarId => HarnessToolbarId;

    public IToolbarRegistryService ToolbarRegistry => _toolbarRegistry;

    public ObservableCollection<ToolbarItem> PrimaryToolbarItems { get; }

    public RelayCommand NavigateToModeACommand { get; }

    public int RebuildCount { get; private set; }

    public void RebuildForMode(ModeHostState activeMode)
    {
        PrimaryToolbarItems.Clear();
        PrimaryToolbarItems.Add(CreateModeAButton(activeMode));
        _toolbarRegistry.SetItemVisibility(HarnessToolbarId, ModeAItemId, true);
        RebuildCount++;
    }

    private void RegisterToolbarDefinition()
    {
        var existingDefinition = _toolbarRegistry.ToolbarDefinitions.FirstOrDefault(definition => definition.Id == HarnessToolbarId);
        if (existingDefinition is null)
        {
            _toolbarRegistry.RegisterDefinition(
                new ToolbarDefinition(
                    id: HarnessToolbarId,
                    displayName: "Harness",
                    canHide: true,
                    defaultVisible: true,
                    itemIds: [ModeAItemId]));
        }

        _toolbarRegistry.SetItemVisibility(HarnessToolbarId, ModeAItemId, true);
    }

    private ToolbarItem CreateModeAButton(ModeHostState activeMode)
    {
        return new ToolbarItem(
            id: ModeAItemId,
            kind: ToolbarItemKind.Button,
            semanticMetadata: new ToolbarItemSemanticMetadata(
                new ToolbarItemText("ModeA", "Navigate to ModeA")),
            displayIntent: ToolbarItemDisplayIntent.TextOnly,
            order: 10,
            command: NavigateToModeACommand,
            includeInMenuBar: false,
            logicalGroup: "Mode")
        {
            IsEnabled = activeMode != ModeHostState.ModeA
        };
    }
}