using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Graphing.Controls.Models;
using UnitRegistry;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Immutable snapshot of a graph axis.
    /// Represents the static properties of an axis as captured at a point in time.
    /// </summary>
    internal sealed class AxisSnapshot : IAxisSnapshot
    {
        private readonly string _axisId;
        private readonly AxisOrientation _orientation;
        private readonly AxisSide _side;
        private readonly string _formatterName;
        private readonly IAxisLabelValueConverter _labelValueConverter;
        private readonly Unit _unit;
        private readonly string _displayUnitLabel;
        private readonly AxisScaleType _scaleType;
        private readonly bool _isAutoRange;
        private readonly IReadOnlyList<IFieldSnapshot> _fields;
        private readonly double? _minimumValue;
        private readonly double? _maximumValue;
        private readonly double? _increment;
        private readonly bool _isAutoIncrement;
        private readonly int _majorTickStride;
        private readonly string _title;

        /// <summary>
        /// Stable identity of the axis.
        /// </summary>
        public string AxisId
        {
            get { return _axisId; }
        }

        public AxisOrientation Orientation
        {
            get { return _orientation; }
        }

        public AxisSide Side
        {
            get { return _side; }
        }

        /// <summary>
        /// Formatter identity used to derive this axis.
        /// </summary>
        public string FormatterName
        {
            get { return _formatterName; }
        }

        /// <summary>
        /// Optional converter that maps numeric axis coordinates to semantic label values.
        /// </summary>
        public IAxisLabelValueConverter LabelValueConverter
        {
            get { return _labelValueConverter; }
        }

        /// <summary>
        /// Unit used for axis labeling.
        /// </summary>
        public Unit Unit
        {
            get { return _unit; }
        }

        public AxisScaleType ScaleType
        {
            get { return _scaleType; }
        }

        public bool IsAutoRange
        {
            get { return _isAutoRange; }
        }

        /// <summary>
        /// Optional display unit label copied from contributing fields.
        /// </summary>
        public string DisplayUnitLabel
        {
            get { return _displayUnitLabel; }
        }

        /// <summary>
        /// Contributing field snapshots for this axis.
        /// </summary>
        public IReadOnlyList<IFieldSnapshot> Fields
        {
            get { return _fields; }
        }

        /// <summary>
        /// Minimum numeric value across contributing fields, if present.
        /// </summary>
        public double? MinimumValue
        {
            get { return _minimumValue; }
        }

        /// <summary>
        /// Maximum numeric value across contributing fields, if present.
        /// </summary>
        public double? MaximumValue
        {
            get { return _maximumValue; }
        }

        /// <summary>
        /// Tick increment to use when realizing axis ticks.
        /// </summary>
        public double? Increment
        {
            get { return _increment; }
        }

        /// <summary>
        /// Indicates whether increment was auto-calculated.
        /// </summary>
        public bool IsAutoIncrement
        {
            get { return _isAutoIncrement; }
        }

        /// <summary>
        /// Number of minor tick intervals between consecutive major tick labels.
        /// </summary>
        public int MajorTickStride
        {
            get { return _majorTickStride; }
        }

        /// <summary>
        /// Display title for the axis, combining contributing field label with the axis display unit.
        /// </summary>
        public string Title
        {
            get { return _title; }
        }

        /// <summary>
        /// Creates an immutable snapshot of an axis.
        /// </summary>
        /// <param name="axisId">Stable identity of the axis.</param>
        /// <param name="orientation">Axis orientation.</param>
        /// <param name="side">Axis side placement.</param>
        /// <param name="formatterName">Formatter identity used to derive this axis.</param>
        /// <param name="labelValueConverter">Optional converter for axis label semantic value projection.</param>
        /// <param name="unit">Axis unit.</param>
        /// <param name="displayUnitLabel">Optional display unit label.</param>
        /// <param name="scaleType">Axis scale type.</param>
        /// <param name="isAutoRange">Whether axis bounds are automatic.</param>
        /// <param name="fields">Contributing field snapshots.</param>
        /// <param name="minimumValue">Minimum numeric value across contributing fields.</param>
        /// <param name="maximumValue">Maximum numeric value across contributing fields.</param>
        /// <param name="increment">Tick increment used to realize axis ticks.</param>
        /// <param name="isAutoIncrement">Whether increment was auto-calculated.</param>
        /// <param name="title">Display title for the axis derived from formatter and unit labels.</param>
        public AxisSnapshot(
            string axisId,
            AxisOrientation orientation,
            AxisSide side,
            string formatterName,
            IAxisLabelValueConverter labelValueConverter,
            Unit unit,
            string displayUnitLabel,
            AxisScaleType scaleType,
            bool isAutoRange,
            IEnumerable<IFieldSnapshot> fields,
            double? minimumValue,
            double? maximumValue,
            double? increment,
            bool isAutoIncrement,
            int majorTickStride,
            string title)
        {
            _axisId = axisId;
            _orientation = orientation;
            _side = side;
            _formatterName = formatterName;
            _labelValueConverter = labelValueConverter;
            _unit = unit;
            _displayUnitLabel = displayUnitLabel;
            _scaleType = scaleType;
            _isAutoRange = isAutoRange;
            _fields = new ReadOnlyCollection<IFieldSnapshot>(
                new List<IFieldSnapshot>(fields ?? Array.Empty<IFieldSnapshot>())
            );
            _minimumValue = minimumValue;
            _maximumValue = maximumValue;
            _increment = increment;
            _isAutoIncrement = isAutoIncrement;
            _majorTickStride = majorTickStride > 0 ? majorTickStride : 1;
            _title = title ?? string.Empty;
        }
    }
}
