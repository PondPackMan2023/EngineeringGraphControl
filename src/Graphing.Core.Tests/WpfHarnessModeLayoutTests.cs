#nullable enable

using System;
using System.Collections.Generic;
using Dev.Core.Services;
using Dev.Core.Services.Mode;
using Graphing.Controls.Models;
using Graphing.TestHarness.WPF.Core.Navigation;
using Graphing.TestHarness.WPF.Core.ViewModels;
using NUnit.Framework;

namespace Graphing.Core.Tests;

[TestFixture]
public class WpfHarnessModeLayoutTests
{
    // -------------------------------------------------------------------------
    // IHarnessModeLayout contracts
    // -------------------------------------------------------------------------

    [Test]
    public void ModeALayout_Sidebar_IsNonNull()
    {
        var (layout, _) = BuildModeALayout();

        Assert.That(layout.Sidebar, Is.Not.Null);
    }

    [Test]
    public void ModeALayout_Sidebar_IsTypeOf_ModeASidebarViewModel()
    {
        var (layout, _) = BuildModeALayout();

        Assert.That(layout.Sidebar, Is.TypeOf<ModeASidebarViewModel>());
    }

    [Test]
    public void ModeALayout_MainContent_IsTypeOf_ModeAMainContentViewModel()
    {
        var (layout, _) = BuildModeALayout();

        Assert.That(layout.MainContent, Is.TypeOf<ModeAMainContentViewModel>());
    }

    [Test]
    public void ModeBLayout_Sidebar_IsNull()
    {
        var layout = BuildModeBLayout(new TrackingScenarioProvider());

        Assert.That(layout.Sidebar, Is.Null);
    }

    [Test]
    public void ModeBLayout_MainContent_IsTypeOf_ModeBMainContentViewModel()
    {
        var layout = BuildModeBLayout(new TrackingScenarioProvider());

        Assert.That(layout.MainContent, Is.TypeOf<ModeBMainContentViewModel>());
    }

    // -------------------------------------------------------------------------
    // MainViewModel projection
    // -------------------------------------------------------------------------

    [Test]
    public void MainViewModel_InitialState_ProjectsModeALayout()
    {
        var vm = BuildMainViewModel(new TrackingScenarioProvider(), out _);

        Assert.That(vm.CurrentLayout, Is.TypeOf<ModeALayout>());
        Assert.That(vm.SidebarViewModel, Is.TypeOf<ModeASidebarViewModel>());
        Assert.That(vm.MainContentViewModel, Is.TypeOf<ModeAMainContentViewModel>());
    }

    [Test]
    public void MainViewModel_AfterNavigateToModeB_ProjectsModeBLayout()
    {
        var vm = BuildMainViewModel(new TrackingScenarioProvider(), out var navigationService);

        navigationService.NavigateToModeB();

        Assert.That(vm.CurrentLayout, Is.TypeOf<ModeBLayout>());
        Assert.That(vm.SidebarViewModel, Is.Null);
        Assert.That(vm.MainContentViewModel, Is.TypeOf<ModeBMainContentViewModel>());
    }

    [Test]
    public void MainViewModel_SidebarViewModel_IsNull_WhenInModeB()
    {
        var vm = BuildMainViewModel(new TrackingScenarioProvider(), out var navigationService);

        navigationService.NavigateToModeB();

        Assert.That(vm.SidebarViewModel, Is.Null, "SidebarRegion should project nothing for ModeB.");
    }

    [Test]
    public void MainViewModel_SidebarViewModel_IsRestored_AfterReturnToModeA()
    {
        var vm = BuildMainViewModel(new TrackingScenarioProvider(), out var navigationService);

        navigationService.NavigateToModeB();
        navigationService.NavigateToModeA();

        Assert.That(vm.SidebarViewModel, Is.Not.Null);
        Assert.That(vm.SidebarViewModel, Is.TypeOf<ModeASidebarViewModel>());
    }

    [Test]
    public void MainViewModel_RaisesPropertyChanged_ForSidebarViewModel_OnNavigation()
    {
        var vm = BuildMainViewModel(new TrackingScenarioProvider(), out var navigationService);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        navigationService.NavigateToModeB();

        Assert.That(changed, Does.Contain(nameof(MainViewModel.SidebarViewModel)));
        Assert.That(changed, Does.Contain(nameof(MainViewModel.MainContentViewModel)));
    }

    [Test]
    public void MainViewModel_CurrentLayout_RetainsSameInstance_BetweenTransitions()
    {
        var vm = BuildMainViewModel(new TrackingScenarioProvider(), out var navigationService);

        var firstModeALayout = vm.CurrentLayout;

        navigationService.NavigateToModeB();
        navigationService.NavigateToModeA();

        Assert.That(vm.CurrentLayout, Is.SameAs(firstModeALayout),
            "Layout instances should be stable singletons; same ModeA layout returned on re-entry.");
    }

    // -------------------------------------------------------------------------
    // Toolbar rebuild during navigation
    // -------------------------------------------------------------------------

    [Test]
    public void MainViewModel_ToolbarRebuildVersion_IncreasesOnEachNavigation()
    {
        var vm = BuildMainViewModel(new TrackingScenarioProvider(), out var navigationService);

        Assert.That(vm.ToolbarRebuildVersion, Is.EqualTo(0));

        navigationService.NavigateToModeB();
        Assert.That(vm.ToolbarRebuildVersion, Is.EqualTo(1));

        navigationService.NavigateToModeA();
        Assert.That(vm.ToolbarRebuildVersion, Is.EqualTo(2));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static (ModeALayout layout, ITestHarnessNavigationService navigationService) BuildModeALayout()
    {
        var modeService = new ModeService();
        var navigationService = new TestHarnessNavigationService(
            modeService,
            new ModeBFeatureMode(new ModeBMainContentViewModel(new TrackingScenarioProvider())));
        var sidebar = new ModeASidebarViewModel(navigationService);
        var mainContent = new ModeAMainContentViewModel();
        return (new ModeALayout(sidebar, mainContent), navigationService);
    }

    private static ModeBLayout BuildModeBLayout(IGraphScenarioProvider scenarioProvider)
    {
        var mainContent = new ModeBMainContentViewModel(scenarioProvider);
        return new ModeBLayout(mainContent);
    }

    private static MainViewModel BuildMainViewModel(
        IGraphScenarioProvider scenarioProvider,
        out ITestHarnessNavigationService navigationService)
    {
        var modeService = new ModeService();
        var modeBMainContent = new ModeBMainContentViewModel(scenarioProvider);
        var featureMode = new ModeBFeatureMode(modeBMainContent);
        var nav = new TestHarnessNavigationService(modeService, featureMode);
        navigationService = nav;

        var toolbarRegistry = new Dev.Core.Services.ToolbarRegistryService(
            System.IO.Path.GetTempPath());
        var toolbar = new ToolbarViewModel(toolbarRegistry, nav);

        var sidebarVm = new ModeASidebarViewModel(nav);
        var modeAMainContent = new ModeAMainContentViewModel();
        var modeALayout = new ModeALayout(sidebarVm, modeAMainContent);
        var modeBLayout = new ModeBLayout(modeBMainContent);

        return new MainViewModel(modeALayout, modeBLayout, nav, toolbar);
    }

    private sealed class TrackingScenarioProvider : IGraphScenarioProvider
    {
        public IGraphModel? BuildScenario(GraphScenarioId scenarioId) => null;
    }
}
