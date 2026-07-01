using Graphing.Controls.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IGraphScenarioProvider _scenarioProvider;
    private IGraphModel? _graphModel;
    private bool _zoomEnabled;
    private int _zoomExtentsRequestVersion;

    public MainViewModel(IGraphScenarioProvider scenarioProvider)
    {
        _scenarioProvider = scenarioProvider;
        LoadScenarioACommand = new RelayCommand(LoadScenarioA);
        LoadScenarioBCommand = new RelayCommand(() => LoadScenario(GraphScenarioId.B));
        LoadScenarioCCommand = new RelayCommand(() => LoadScenario(GraphScenarioId.C));
        LoadScenarioDCommand = new RelayCommand(() => LoadScenario(GraphScenarioId.D));
        ZoomExtentsCommand = new RelayCommand(RequestZoomExtents);

        LoadScenarioA();
    }

    public MainViewModel()
        : this(new NullGraphScenarioProvider())
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IGraphModel? GraphModel
    {
        get => _graphModel;
        private set
        {
            if (ReferenceEquals(_graphModel, value))
            {
                return;
            }

            _graphModel = value;
            OnPropertyChanged();
        }
    }

    public bool ZoomEnabled
    {
        get => _zoomEnabled;
        set
        {
            if (_zoomEnabled == value)
            {
                return;
            }

            _zoomEnabled = value;
            OnPropertyChanged();
        }
    }

    public int ZoomExtentsRequestVersion
    {
        get => _zoomExtentsRequestVersion;
        private set
        {
            if (_zoomExtentsRequestVersion == value)
            {
                return;
            }

            _zoomExtentsRequestVersion = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadScenarioACommand { get; }

    public ICommand LoadScenarioBCommand { get; }

    public ICommand LoadScenarioCCommand { get; }

    public ICommand LoadScenarioDCommand { get; }

    public ICommand ZoomExtentsCommand { get; }

    private void LoadScenarioA()
    {
        LoadScenario(GraphScenarioId.A);
    }

    private void LoadScenario(GraphScenarioId scenarioId)
    {
        GraphModel = _scenarioProvider.BuildScenario(scenarioId);
    }

    private void RequestZoomExtents()
    {
        ZoomExtentsRequestVersion++;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class NullGraphScenarioProvider : IGraphScenarioProvider
    {
        public IGraphModel? BuildScenario(GraphScenarioId scenarioId)
        {
            return null;
        }
    }
}
