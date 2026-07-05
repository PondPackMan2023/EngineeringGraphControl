using System;
using System.Windows;
using Graphing.TestHarness.WPF.Core.ViewModels;

namespace Graphing.TestHarness.WPF.Pie;

public partial class MainWindow : Window
{
    public MainWindow(PieHarnessViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;
    }
}
