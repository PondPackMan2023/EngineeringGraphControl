namespace Graphing.TestHarness.WPF.Core.ViewModels;

public sealed class PieScenarioOption
{
    public PieScenarioOption(PieScenarioId id, string displayName)
    {
        Id = id;
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
    }

    public PieScenarioId Id { get; }

    public string DisplayName { get; }
}
