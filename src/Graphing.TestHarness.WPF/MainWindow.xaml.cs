using System.Windows;
using Graphing.TestHarness.WPF.Core.ViewModels;

namespace Graphing.TestHarness.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(new ScenarioProvider());
        }
    }
}
