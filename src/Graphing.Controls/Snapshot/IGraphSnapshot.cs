using System.Collections.Generic;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Read-only interface for consuming a graph snapshot.
    /// </summary>
    public interface IGraphSnapshot
    {
        /// <summary>
        /// Read-only collection of series snapshots contained in this graph.
        /// </summary>
        IReadOnlyList<ISeriesSnapshot> Series { get; }

        /// <summary>
        /// Read-only collection of derived axis snapshots contained in this graph.
        /// </summary>
        IReadOnlyList<IAxisSnapshot> Axes { get; }
    }
}
