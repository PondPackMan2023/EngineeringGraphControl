#nullable enable

using System;
using Graphing.Controls.Models;
using Dev.Core.Services.Mode;
using Dev.Core.Services;
using Graphing.TestHarness.WPF.Core;
using Graphing.TestHarness.WPF.Core.Navigation;
using Graphing.TestHarness.WPF.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Graphing.Core.Tests;

[TestFixture]
public class WpfHarnessCoreServiceRegistrationTests
{
    [Test]
    public void AddGraphingTestHarnessCore_ResolvesModeDrivenMainViewModel_AndNavigatesToModeB()
    {
        var scenarioProvider = new TrackingScenarioProvider();
        var services = new ServiceCollection();
        var toolbarPath = TestContext.CurrentContext.WorkDirectory;

        services.AddSingleton<IThemeService>(new FakeThemeService());
        services.AddSingleton<IToolbarRegistryService>(_ => new ToolbarRegistryService(toolbarPath));
        services.AddSingleton<IGraphScenarioProvider>(scenarioProvider);
        services.AddGraphingTestHarnessCore();

        using var serviceProvider = services.BuildServiceProvider();

        var modeService = serviceProvider.GetRequiredService<IModeService>();
        var navigationService = serviceProvider.GetRequiredService<ITestHarnessNavigationService>();
        var toolbar = serviceProvider.GetRequiredService<ToolbarViewModel>();
        var first = serviceProvider.GetRequiredService<MainViewModel>();
        var second = serviceProvider.GetRequiredService<MainViewModel>();

        Assert.That(first, Is.SameAs(second));
        Assert.That(first.Toolbar, Is.SameAs(toolbar));
        Assert.That(modeService.ActiveFeatureMode, Is.Null);
        Assert.That(navigationService.CurrentState, Is.EqualTo(ModeHostState.ModeA));
        Assert.That(first.CurrentLayout, Is.TypeOf<ModeALayout>());
        Assert.That(first.SidebarViewModel, Is.TypeOf<ModeASidebarViewModel>());
        Assert.That(first.MainContentViewModel, Is.TypeOf<ModeAMainContentViewModel>());
        Assert.That(scenarioProvider.LastScenarioId, Is.Null);

        navigationService.NavigateToModeB();

        Assert.That(modeService.ActiveFeatureMode, Is.Not.Null);
        Assert.That(modeService.ActiveFeatureMode!.ModeId, Is.EqualTo(ModeBFeatureMode.ModeIdValue));
        Assert.That(navigationService.CurrentState, Is.EqualTo(ModeHostState.ModeB));
        Assert.That(first.CurrentLayout, Is.TypeOf<ModeBLayout>());
        Assert.That(first.SidebarViewModel, Is.Null);
        Assert.That(first.MainContentViewModel, Is.TypeOf<ModeBMainContentViewModel>());
        Assert.That(first.ToolbarRebuildVersion, Is.EqualTo(1));
        Assert.That(scenarioProvider.LastScenarioId, Is.EqualTo(GraphScenarioId.A));
    }

    private sealed class FakeThemeService : IThemeService
    {
        public string CurrentTheme { get; private set; } = "Light";

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        public void ApplyTheme(string theme)
        {
            CurrentTheme = string.Equals(theme, "Dark", StringComparison.Ordinal) ? "Dark" : "Light";
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(CurrentTheme));
        }
    }

    private sealed class TrackingScenarioProvider : IGraphScenarioProvider
    {
        public GraphScenarioId? LastScenarioId { get; private set; }

        public IGraphModel? BuildScenario(GraphScenarioId scenarioId)
        {
            LastScenarioId = scenarioId;
            return null;
        }
    }
}