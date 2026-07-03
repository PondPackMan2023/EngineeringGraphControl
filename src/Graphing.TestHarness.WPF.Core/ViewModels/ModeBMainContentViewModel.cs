using System.ComponentModel;
using System.Runtime.CompilerServices;
using Graphing.Controls.Models;

namespace Graphing.TestHarness.WPF.Core.ViewModels;

/// <summary>
/// Main content view model for ModeB. Hosts EngineeringGraphControl.
/// The graph model is populated lazily via Activate(), which is called
/// by ModeBFeatureMode.OnEnter() to ensure the control enters the visual
/// tree before data is provided.
/// </summary>
public sealed class ModeBMainContentViewModel : IHarnessMainContentViewModel, INotifyPropertyChanged
{
    private readonly IGraphScenarioProvider _scenarioProvider;
    private IGraphModel? _graphModel;

    public ModeBMainContentViewModel(IGraphScenarioProvider scenarioProvider)
    {
        _scenarioProvider = scenarioProvider ?? throw new ArgumentNullException(nameof(scenarioProvider));
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

    public bool ZoomEnabled { get; set; } = true;

    public void Activate()
    {
        GraphModel = _scenarioProvider.BuildScenario(GraphScenarioId.A);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
