using Graphing.Controls.Models;
using Graphing.Controls.Presentation;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Builds immutable snapshots from graph models.
    /// </summary>
    public interface IGraphSnapshotBuilder
    {
        /// <summary>
        /// Builds a snapshot for the supplied model and presentation options.
        /// </summary>
        /// <param name="graphModel">Graph model to snapshot.</param>
        /// <param name="options">Optional presentation options used while building the snapshot.</param>
        /// <returns>The resulting graph snapshot.</returns>
        IGraphSnapshot Build(IGraphModel graphModel, GraphPresentationOptions options = null);
    }
}
