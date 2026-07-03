#nullable enable

using System;
using System.Linq;
using Dev.Core.Services;
using Dev.Core.Toolbar;
using Graphing.TestHarness.WPF.Core.Navigation;
using Graphing.TestHarness.WPF.Core.ViewModels;
using NUnit.Framework;

namespace Graphing.Core.Tests;

[TestFixture]
public class WpfHarnessToolbarViewModelTests
{
    [Test]
    public void Constructor_RegistersToolbarDefinition()
    {
        var toolbarRegistry = new ToolbarRegistryService(TestContext.CurrentContext.WorkDirectory);
        var navigationService = new FakeNavigationService();

        var viewModel = new ToolbarViewModel(toolbarRegistry, navigationService);

        viewModel.RebuildForMode(ModeHostState.ModeA);

        var definition = toolbarRegistry.ToolbarDefinitions.SingleOrDefault(d => d.Id == viewModel.PrimaryToolbarId);
        Assert.That(definition, Is.Not.Null);
        Assert.That(definition!.ItemIds.Select(id => id.Value), Is.EqualTo(new[] { "Harness.Toolbar.ModeA" }));
        Assert.That(toolbarRegistry.IsVisible(viewModel.PrimaryToolbarId), Is.True);
        Assert.That(toolbarRegistry.IsItemVisible(viewModel.PrimaryToolbarId, new ToolbarItemId("Harness.Toolbar.ModeA")), Is.True);
    }

    [Test]
    public void RebuildForMode_CreatesModeAOnlyToolbar_AndEnablesReturnOnlyFromModeB()
    {
        var toolbarRegistry = new ToolbarRegistryService(TestContext.CurrentContext.WorkDirectory);
        var navigationService = new FakeNavigationService();
        var viewModel = new ToolbarViewModel(toolbarRegistry, navigationService);

        viewModel.RebuildForMode(ModeHostState.ModeA);
        var firstItem = viewModel.PrimaryToolbarItems.Single();

        Assert.That(firstItem.SemanticMetadata.Text.Label, Is.EqualTo("ModeA"));
        Assert.That(firstItem.IsEnabled, Is.False);
        Assert.That(viewModel.RebuildCount, Is.EqualTo(1));

        viewModel.RebuildForMode(ModeHostState.ModeB);
        var secondItem = viewModel.PrimaryToolbarItems.Single();

        Assert.That(secondItem.SemanticMetadata.Text.Label, Is.EqualTo("ModeA"));
        Assert.That(secondItem.IsEnabled, Is.True);
        Assert.That(secondItem, Is.Not.SameAs(firstItem));
        Assert.That(viewModel.RebuildCount, Is.EqualTo(2));
    }

    private sealed class FakeNavigationService : ITestHarnessNavigationService
    {
        public ModeHostState CurrentState => ModeHostState.ModeA;

        public event EventHandler<ModeNavigationChangedEventArgs>? NavigationChanged;

        public void NavigateToModeA()
        {
            NavigationChanged?.Invoke(this, new ModeNavigationChangedEventArgs(ModeHostState.ModeB, ModeHostState.ModeA));
        }

        public void NavigateToModeB()
        {
            NavigationChanged?.Invoke(this, new ModeNavigationChangedEventArgs(ModeHostState.ModeA, ModeHostState.ModeB));
        }
    }
}