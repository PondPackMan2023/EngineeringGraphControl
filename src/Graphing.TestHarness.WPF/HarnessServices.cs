using System;
using System.IO;
using Dev.Core.Services;
using Dev.Wpf.Services;
using Graphing.TestHarness.WPF.Core;
using Graphing.TestHarness.WPF.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Graphing.TestHarness.WPF;

public static class HarnessServices
{
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        return services.BuildServiceProvider();
    }

    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IToolbarRegistryService>(_ => new ToolbarRegistryService(Path.GetTempPath()));
        services.AddSingleton<IGraphScenarioProvider, ScenarioProvider>();
        services.AddGraphingTestHarnessCore();
        services.AddSingleton<MainWindow>();

        return services;
    }
}