namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Factory abstraction for creating graph snapshot builders.
    /// </summary>
    public interface IGraphSnapshotBuilderProvider
    {
        /// <summary>
        /// Creates a new graph snapshot builder instance.
        /// </summary>
        /// <returns>A graph snapshot builder.</returns>
        IGraphSnapshotBuilder CreateGraphSnapshotBuilder();
    }
}
