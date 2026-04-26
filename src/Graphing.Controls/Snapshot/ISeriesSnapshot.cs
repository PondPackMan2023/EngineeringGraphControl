using System.Collections.Generic;
using Graphing.Controls.Models;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Read-only interface for consuming a series snapshot.
    /// </summary>
    public interface ISeriesSnapshot
    {
        /// <summary>
        /// Opaque series identity copied from the graph series model.
        /// </summary>
        object Identifier { get; }

        /// <summary>
        /// Unique identifier for the series.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Display label for the series.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// The chart type used to render this series.
        /// </summary>
        ChartType ChartType { get; }

        /// <summary>
        /// X field snapshot used by this series.
        /// </summary>
        IFieldSnapshot XField { get; }

        /// <summary>
        /// Y field snapshot used by this series.
        /// </summary>
        IFieldSnapshot YField { get; }

        /// <summary>
        /// Axis identity used for the X field.
        /// </summary>
        string XAxisId { get; }

        /// <summary>
        /// Axis identity used for the Y field.
        /// </summary>
        string YAxisId { get; }

        /// <summary>
        /// Read-only collection containing the X and Y field snapshots.
        /// </summary>
        IReadOnlyList<IFieldSnapshot> Fields { get; }
    }
}
