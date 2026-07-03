using Graphing.TestHarness.WPF.Core.ViewModels;
using Graphing.TestHarness.WPF.Core.Navigation;
using Dev.Core.Services.Mode;
using Microsoft.Extensions.DependencyInjection;

namespace Graphing.TestHarness.WPF.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphingTestHarnessCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IModeService, ModeService>();
        services.AddSingleton<ModeBFeatureMode>();
        services.AddSingleton<ITestHarnessNavigationService, TestHarnessNavigationService>();

        // ModeA layout
        services.AddSingleton<ModeASidebarViewModel>();
        services.AddSingleton<ModeAMainContentViewModel>();
        services.AddSingleton<ModeALayout>();

        // ModeB layout
        services.AddSingleton<ModeBMainContentViewModel>();
        services.AddSingleton<ModeBLayout>();

        services.AddSingleton<ToolbarViewModel>();
        services.AddSingleton<MainViewModel>();

        return services;
    }
}