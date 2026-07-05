using System.Windows;
using Dev.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Graphing.TestHarness.WPF.Pie;

public partial class App : Application
{
    private ServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _serviceProvider = HarnessServices.BuildServiceProvider();
        var themeService = _serviceProvider.GetRequiredService<IThemeService>();
        themeService.ApplyTheme("System");

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();

        base.OnExit(e);
    }
}
