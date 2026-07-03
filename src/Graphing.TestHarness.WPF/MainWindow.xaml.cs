using System;
using System.Windows;
using Graphing.TestHarness.WPF.Core.ViewModels;

namespace Graphing.TestHarness.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel mainViewModel)
        {
            ArgumentNullException.ThrowIfNull(mainViewModel);

            InitializeComponent();
            DataContext = mainViewModel;
        }
    }
}
