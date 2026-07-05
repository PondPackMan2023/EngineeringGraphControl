using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Graphing.Core.Pie.Models;
using Graphing.Core.Pie.Presentation;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

public sealed class PieHarnessViewModel : INotifyPropertyChanged
{
    private PieScenarioOption? _selectedPieScenario;
    private IPieGraphModel? _currentPieGraphModel;
    private PieGraphPresentationOptions? _currentPieGraphPresentationOptions;
    private bool _useShortLegend;
    private bool _showLegendBorder;
    private PieSliceInteractionContext? _lastDoubleClickContext;
    private string? _lastDoubleClickLabel;

    public PieHarnessViewModel(IPieGraphScenarioProvider scenarioProvider)
    {
        ScenarioProvider = scenarioProvider ?? throw new ArgumentNullException(nameof(scenarioProvider));

        AvailablePieScenarios = ScenarioProvider.GetAvailableScenarios();
        ApplyScenarioCommand = new RelayCommand(ApplySelectedScenario, () => SelectedPieScenario is not null);
        PieSliceDoubleClickCommand = new RelayCommand<PieSliceInteractionContext>(OnPieSliceDoubleClick);

        if (AvailablePieScenarios.Count > 0)
        {
            SelectedPieScenario = AvailablePieScenarios[0];
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IPieGraphScenarioProvider ScenarioProvider { get; }

    public IReadOnlyList<PieScenarioOption> AvailablePieScenarios { get; }

    public PieScenarioOption? SelectedPieScenario
    {
        get => _selectedPieScenario;
        set
        {
            if (ReferenceEquals(_selectedPieScenario, value))
            {
                return;
            }

            _selectedPieScenario = value;
            OnPropertyChanged();
            ApplyScenarioCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand ApplyScenarioCommand { get; }

    public ICommand PieSliceDoubleClickCommand { get; }

    public PieSliceInteractionContext? LastDoubleClickContext
    {
        get => _lastDoubleClickContext;
        private set
        {
            if (ReferenceEquals(_lastDoubleClickContext, value))
            {
                return;
            }

            _lastDoubleClickContext = value;
            OnPropertyChanged();
        }
    }

    public string? LastDoubleClickLabel
    {
        get => _lastDoubleClickLabel;
        private set
        {
            if (_lastDoubleClickLabel == value)
            {
                return;
            }

            _lastDoubleClickLabel = value;
            OnPropertyChanged();
        }
    }

    public IPieGraphModel? CurrentPieGraphModel
    {
        get => _currentPieGraphModel;
        private set
        {
            if (ReferenceEquals(_currentPieGraphModel, value))
            {
                return;
            }

            _currentPieGraphModel = value;
            OnPropertyChanged();
        }
    }

    public PieGraphPresentationOptions? CurrentPieGraphPresentationOptions
    {
        get => _currentPieGraphPresentationOptions;
        private set
        {
            if (ReferenceEquals(_currentPieGraphPresentationOptions, value))
            {
                return;
            }

            _currentPieGraphPresentationOptions = value;
            OnPropertyChanged();
        }
    }

    public bool UseShortLegend
    {
        get => _useShortLegend;
        set
        {
            if (_useShortLegend == value)
            {
                return;
            }

            _useShortLegend = value;
            OnPropertyChanged();
            UpdatePresentationOptions();
        }
    }

    public bool ShowLegendBorder
    {
        get => _showLegendBorder;
        set
        {
            if (_showLegendBorder == value)
            {
                return;
            }

            _showLegendBorder = value;
            OnPropertyChanged();
            UpdatePresentationOptions();
        }
    }

    public void ApplySelectedScenario()
    {
        if (SelectedPieScenario is null)
        {
            return;
        }

        var scenario = ScenarioProvider.BuildScenario(SelectedPieScenario.Id);
        CurrentPieGraphModel = scenario.PieGraphModel;
        _useShortLegend = false;
        UpdatePresentationOptions();
    }

    private void OnPieSliceDoubleClick(PieSliceInteractionContext? context)
    {
        if (context == null)
        {
            return;
        }

        LastDoubleClickContext = context;
        LastDoubleClickLabel = $"Clicked: {context.Label} (${context.FormattedValue} - {context.Percentage:F1}%)";
    }

    private void UpdatePresentationOptions()
    {
        if (CurrentPieGraphModel is null)
        {
            CurrentPieGraphPresentationOptions = null;
            return;
        }

        CurrentPieGraphPresentationOptions = new PieGraphPresentationOptions(
            legendVisible: true,
            useShortLegend: _useShortLegend,
            showLegendBorder: _showLegendBorder);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
