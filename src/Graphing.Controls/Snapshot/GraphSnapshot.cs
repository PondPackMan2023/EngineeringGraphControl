using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Immutable snapshot of an entire graph at a point in time.
    /// Contains a collection of series snapshots representing the complete graph state.
    /// </summary>
    internal sealed class GraphSnapshot : IGraphSnapshot
    {
        private readonly IReadOnlyList<SeriesSnapshot> _series;
        private readonly IReadOnlyList<AxisSnapshot> _axes;

        /// <summary>
        /// Read-only collection of series snapshots contained in this graph.
        /// </summary>
        public IReadOnlyList<ISeriesSnapshot> Series
        {
            get { return _series; }
        }

        /// <summary>
        /// Read-only collection of derived axis snapshots contained in this graph.
        /// </summary>
        public IReadOnlyList<IAxisSnapshot> Axes
        {
            get { return _axes; }
        }

        /// <summary>
        /// Creates an immutable snapshot of a graph.
        /// </summary>
        /// <param name="series">Collection of series snapshots to include in the graph snapshot.</param>
        /// <param name="axes">Collection of derived axis snapshots to include in the graph snapshot.</param>
        public GraphSnapshot(IEnumerable<SeriesSnapshot> series, IEnumerable<AxisSnapshot> axes)
        {
            _series = new ReadOnlyCollection<SeriesSnapshot>(
                new List<SeriesSnapshot>(series ?? Array.Empty<SeriesSnapshot>())
            );
            _axes = new ReadOnlyCollection<AxisSnapshot>(
                new List<AxisSnapshot>(axes ?? Array.Empty<AxisSnapshot>())
            );
        }
    }
}
