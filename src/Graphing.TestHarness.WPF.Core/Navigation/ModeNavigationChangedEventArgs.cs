namespace Graphing.TestHarness.WPF.Core.Navigation;

public sealed class ModeNavigationChangedEventArgs : EventArgs
{
    public ModeNavigationChangedEventArgs(ModeHostState previous, ModeHostState current)
    {
        Previous = previous;
        Current = current;
    }

    public ModeHostState Previous { get; }

    public ModeHostState Current { get; }
}
