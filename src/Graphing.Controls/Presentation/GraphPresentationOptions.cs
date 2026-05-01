using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Graphing.Controls.Models;
using Graphing.Controls.Snapshot;
using UnitRegistry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Holds series-specific presentation overrides.
    /// </summary>
    public sealed class SeriesOverrides
    {
        public bool HasLabelOverride { get; set; }
        public string Label { get; set; }
        public bool HasColorOverride { get; set; }
        public Color Color { get; set; }
    }

    /// <summary>
    /// Holds axis-specific presentation overrides.
    /// </summary>
    public sealed class AxisOverrides
    {
        public bool HasTitleOverride { get; set; }
        public string Title { get; set; }
        public bool HasFixedRange { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public bool HasFixedIncrement { get; set; }
        public double Increment { get; set; }
        public bool EnforceMinimumZero { get; set; }
    }
    /// <summary>
    /// Semantic presentation options consumed while building presentation geometry.
    /// </summary>
    public sealed class GraphPresentationOptions
    {
        private static readonly ISet<Dimension> DefaultDenseNumericYAxisExcludedDimensionsSet =
            new HashSet<Dimension>
            {
                Dimensions.Percent,
                Dimensions.Time
            };

        private readonly HashSet<SeriesId> _hiddenSeriesIds;
        private readonly HashSet<AxisId> _hiddenAxisIds;
        private readonly HashSet<AxisId> _hiddenAxisGridLineIds;
        private readonly IReadOnlyList<AnnotationSemantic> _annotations;
        private readonly IReadOnlyDictionary<SeriesId, SeriesOverrides> _seriesOverrides;
        private readonly IReadOnlyDictionary<AxisId, AxisOverrides> _axisOverrides;
        private readonly bool _enableDenseNumericYAxisTicks;
        private readonly ISet<Dimension> _denseNumericYAxisExcludedDimensions;

        public GraphPresentationOptions(
            IEnumerable<SeriesId> hiddenSeriesIds = null,
            IEnumerable<AxisId> hiddenAxisIds = null,
            string graphTitle = null,
            string graphSubtitle = null,
            IEnumerable<AnnotationSemantic> annotations = null,
            bool showGraphBorder = true,
            LegendPlacement legendPlacement = LegendPlacement.Bottom,
            bool resizeChart = true,
            AxisEndpointInsetMode axisEndpointInsetMode = AxisEndpointInsetMode.Auto,
            double axisEndpointInsetFixedValue = 0.01,
            IEnumerable<AxisId> hiddenAxisGridLineIds = null,
            IDictionary<SeriesId, SeriesOverrides> seriesOverrides = null,
            IDictionary<AxisId, AxisOverrides> axisOverrides = null,
            bool enableDenseNumericYAxisTicks = true,
            ISet<Dimension> denseNumericYAxisExcludedDimensions = null)
        {
            _hiddenSeriesIds = hiddenSeriesIds != null
                ? [.. hiddenSeriesIds]
                : new HashSet<SeriesId>();

            _hiddenAxisIds = hiddenAxisIds != null
                ? [.. hiddenAxisIds]
                : new HashSet<AxisId>();

            _hiddenAxisGridLineIds = hiddenAxisGridLineIds != null
                ? [.. hiddenAxisGridLineIds]
                : new HashSet<AxisId>();

            _seriesOverrides = seriesOverrides != null
                ? new ReadOnlyDictionary<SeriesId, SeriesOverrides>(seriesOverrides)
                : new ReadOnlyDictionary<SeriesId, SeriesOverrides>(new Dictionary<SeriesId, SeriesOverrides>());

            _axisOverrides = axisOverrides != null
                ? new ReadOnlyDictionary<AxisId, AxisOverrides>(axisOverrides)
                : new ReadOnlyDictionary<AxisId, AxisOverrides>(new Dictionary<AxisId, AxisOverrides>());

            _enableDenseNumericYAxisTicks = enableDenseNumericYAxisTicks;
            _denseNumericYAxisExcludedDimensions = denseNumericYAxisExcludedDimensions != null
                ? new HashSet<Dimension>(denseNumericYAxisExcludedDimensions)
                : new HashSet<Dimension>(DefaultDenseNumericYAxisExcludedDimensionsSet);

            GraphTitle = graphTitle;
            GraphSubtitle = graphSubtitle;
            HiddenAxisIds = new ReadOnlyCollection<AxisId>([.. _hiddenAxisIds]);
            HiddenAxisGridLineIds = new ReadOnlyCollection<AxisId>([.. _hiddenAxisGridLineIds]);
            HiddenSeriesIds = new ReadOnlyCollection<SeriesId>([.. _hiddenSeriesIds]);
            _annotations = new ReadOnlyCollection<AnnotationSemantic>(
                [.. annotations ?? []]);
            ShowGraphBorder = showGraphBorder;
            LegendPlacement = legendPlacement;
            ResizeChart = resizeChart;
            AxisEndpointInsetMode = axisEndpointInsetMode;
            AxisEndpointInsetFixedValue = axisEndpointInsetFixedValue;
        }

        public string GraphTitle { get; }

        public string GraphSubtitle { get; }

        public bool ShowGraphBorder { get; }

        public LegendPlacement LegendPlacement { get; }

        public bool ResizeChart { get; }

        public AxisEndpointInsetMode AxisEndpointInsetMode { get; }

        public double AxisEndpointInsetFixedValue { get; }

        public IReadOnlyList<AnnotationSemantic> Annotations
        {
            get { return _annotations; }
        }

        public IReadOnlyCollection<AxisId> HiddenAxisIds { get; }

        public IReadOnlyCollection<AxisId> HiddenAxisGridLineIds { get; }

        public IReadOnlyCollection<SeriesId> HiddenSeriesIds { get; }

        public IReadOnlyDictionary<SeriesId, SeriesOverrides> SeriesOverrides
        {
            get { return _seriesOverrides; }
        }

        public IReadOnlyDictionary<AxisId, AxisOverrides> AxisOverrides
        {
            get { return _axisOverrides; }
        }

        public bool EnableDenseNumericYAxisTicks
        {
            get { return _enableDenseNumericYAxisTicks; }
        }

        public ISet<Dimension> DenseNumericYAxisExcludedDimensions
        {
            get { return _denseNumericYAxisExcludedDimensions; }
        }

        internal static ISet<Dimension> CreateDefaultDenseNumericYAxisExcludedDimensions()
        {
            return new HashSet<Dimension>(DefaultDenseNumericYAxisExcludedDimensionsSet);
        }

        public bool IsSeriesVisible(ISeriesSnapshot series)
        {
            if (series == null)
            {
                return false;
            }

            if (series.SeriesId != null && _hiddenSeriesIds.Contains(series.SeriesId))
            {
                return false;
            }

            return true;
        }

        public bool IsAxisVisible(IAxisSnapshot axis)
        {
            if (axis == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(axis.AxisId) && _hiddenAxisIds.Contains(new AxisId(axis.AxisId)))
            {
                return false;
            }

            return true;
        }

        public bool IsAxisGridLinesVisible(string axisId)
        {
            if (string.IsNullOrWhiteSpace(axisId))
            {
                return true;
            }

            return !_hiddenAxisGridLineIds.Contains(new AxisId(axisId));
        }
    }
}
