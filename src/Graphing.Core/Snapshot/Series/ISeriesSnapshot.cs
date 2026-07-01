using System.Collections.Generic;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Read-only interface for consuming a series snapshot.
    /// </summary>
    public interface ISeriesSnapshot
    {
        /// <summary>
        /// Stable identity of the series.
        /// </summary>
        SeriesId SeriesId { get; }

        /// <summary>
        /// Display label for the series.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// The series type used to render this series.
        /// </summary>
        SeriesType SeriesType { get; }

        /// <summary>
        /// Presentation intent for line rendering.
        /// </summary>
        LineRenderMode LineRenderMode { get; }

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
