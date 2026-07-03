#nullable enable

using System.Collections.Generic;
using Dev.Core.Services.Mode;
using Graphing.Controls.Models;
using Graphing.TestHarness.WPF.Core.Navigation;
using Graphing.TestHarness.WPF.Core.ViewModels;
using NUnit.Framework;

namespace Graphing.Core.Tests;

[TestFixture]
public class WpfHarnessNavigationServiceTests
{
    [Test]
    public void NavigateToModeB_ThenModeA_RaisesOrderedNavigationEvents()
    {
        var modeService = new ModeService();
        var modeBViewModel = new ModeBMainContentViewModel(new TrackingScenarioProvider());
        var featureMode = new ModeBFeatureMode(modeBViewModel);
        var navigationService = new TestHarnessNavigationService(modeService, featureMode);
        var transitions = new List<(ModeHostState Previous, ModeHostState Current)>();

        navigationService.NavigationChanged += (_, e) => transitions.Add((e.Previous, e.Current));

        navigationService.NavigateToModeB();
        navigationService.NavigateToModeA();

        Assert.That(transitions, Is.EqualTo(new[]
        {
            (ModeHostState.ModeA, ModeHostState.ModeB),
            (ModeHostState.ModeB, ModeHostState.ModeA),
        }));
        Assert.That(modeService.ActiveFeatureMode, Is.Null);
        Assert.That(navigationService.CurrentState, Is.EqualTo(ModeHostState.ModeA));
    }

    [Test]
    public void NavigateToModeB_WhenAlreadyInModeB_DoesNotRaiseDuplicateEvent()
    {
        var modeService = new ModeService();
        var modeBViewModel = new ModeBMainContentViewModel(new TrackingScenarioProvider());
        var featureMode = new ModeBFeatureMode(modeBViewModel);
        var navigationService = new TestHarnessNavigationService(modeService, featureMode);
        var eventCount = 0;

        navigationService.NavigationChanged += (_, _) => eventCount++;

        navigationService.NavigateToModeB();
        navigationService.NavigateToModeB();

        Assert.That(eventCount, Is.EqualTo(1));
        Assert.That(modeService.ActiveFeatureMode, Is.Not.Null);
        Assert.That(navigationService.CurrentState, Is.EqualTo(ModeHostState.ModeB));
    }

    private sealed class TrackingScenarioProvider : IGraphScenarioProvider
    {
        public IGraphModel? BuildScenario(GraphScenarioId scenarioId)
        {
            return null;
        }
    }
}
