using System.Collections.Generic;
using Graphing.Controls.Models;
using UnitRegistry;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Read-only interface for consuming an axis snapshot.
    /// </summary>
    public interface IAxisSnapshot
    {
        /// <summary>
        /// Stable identity of the axis.
        /// </summary>
        string AxisId { get; }

        /// <summary>
        /// Orientation of the axis.
        /// </summary>
        AxisOrientation Orientation { get; }

        /// <summary>
        /// Side placement intent of the axis.
        /// </summary>
        AxisSide Side { get; }

        /// <summary>
        /// Formatter identity used to derive this axis.
        /// </summary>
        string FormatterName { get; }

        /// <summary>
        /// Unit used for axis labeling.
        /// </summary>
        Unit Unit { get; }

        /// <summary>
        /// Axis scale type.
        /// </summary>
        AxisScaleType ScaleType { get; }

        /// <summary>
        /// Indicates whether bounds are automatic.
        /// </summary>
        bool IsAutoRange { get; }

        /// <summary>
        /// Optional display unit label copied from contributing fields.
        /// </summary>
        string DisplayUnitLabel { get; }

        /// <summary>
        /// Contributing field snapshots for this axis.
        /// </summary>
        IReadOnlyList<IFieldSnapshot> Fields { get; }

        /// <summary>
        /// Minimum numeric value across contributing fields, if present.
        /// </summary>
        double? MinimumValue { get; }

        /// <summary>
        /// Maximum numeric value across contributing fields, if present.
        /// </summary>
        double? MaximumValue { get; }
    }
}
