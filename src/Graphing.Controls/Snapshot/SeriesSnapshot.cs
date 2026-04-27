using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Graphing.Controls.Models;

namespace Graphing.Controls.Snapshot
{
    /// <summary>
    /// Immutable snapshot of a single data series within a graph.
    /// Contains series metadata and a collection of field snapshots.
    /// </summary>
    internal sealed class SeriesSnapshot : ISeriesSnapshot
    {
        private readonly SeriesId _seriesId;
        private readonly string _label;
        private readonly ChartType _chartType;
        private readonly string _xAxisId;
        private readonly string _yAxisId;
        private readonly IFieldSnapshot _xField;
        private readonly IFieldSnapshot _yField;
        private readonly IReadOnlyList<FieldSnapshot> _fields;

        /// <summary>
        /// Stable identity of the series.
        /// </summary>
        public SeriesId SeriesId
        {
            get { return _seriesId; }
        }

        /// <summary>
        /// Display label for the series.
        /// </summary>
        public string Label
        {
            get { return _label; }
        }

        /// <summary>
        /// The chart type used to render this series (e.g., Line, Bar, etc.).
        /// </summary>
        public ChartType ChartType
        {
            get { return _chartType; }
        }

        public IFieldSnapshot XField
        {
            get { return _xField; }
        }

        public IFieldSnapshot YField
        {
            get { return _yField; }
        }

        public string XAxisId
        {
            get { return _xAxisId; }
        }

        public string YAxisId
        {
            get { return _yAxisId; }
        }

        /// <summary>
        /// Read-only collection of field snapshots in this series.
        /// </summary>
        public IReadOnlyList<IFieldSnapshot> Fields
        {
            get { return _fields; }
        }

        /// <summary>
        /// Creates an immutable snapshot of a series.
        /// </summary>
        /// <param name="seriesId">Stable identity for the series.</param>
        /// <param name="label">Display label for the series.</param>
        /// <param name="chartType">The chart type for rendering.</param>
        /// <param name="xAxisId">Axis identity for the X field.</param>
        /// <param name="yAxisId">Axis identity for the Y field.</param>
        /// <param name="xField">X field snapshot.</param>
        /// <param name="yField">Y field snapshot.</param>
        public SeriesSnapshot(
            SeriesId seriesId,
            string label,
            ChartType chartType,
            string xAxisId,
            string yAxisId,
            FieldSnapshot xField,
            FieldSnapshot yField)
        {
            _seriesId = seriesId;
            _label = label;
            _chartType = chartType;
            _xAxisId = xAxisId;
            _yAxisId = yAxisId;
            _xField = xField;
            _yField = yField;
            _fields = new ReadOnlyCollection<FieldSnapshot>(
                new List<FieldSnapshot>(new[] { xField, yField })
            );
        }
    }
}
