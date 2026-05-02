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
    /// Holds persistent per-series presentation style.
    /// </summary>
    public sealed class SeriesStyle
    {
        public bool HasLabelOverride { get; set; }
        public string Label { get; set; }
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
        private static readonly Color[] DefaultSeriesColorPalette =
        {
            Color.FromArgb(0, 0, 255),
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(0, 128, 0),
            Color.FromArgb(255, 0, 255),
            Color.FromArgb(0, 255, 255),
            Color.FromArgb(128, 0, 0),
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(128, 128, 0),
            Color.FromArgb(128, 0, 128),
            Color.FromArgb(0, 128, 128),
            Color.FromArgb(255, 215, 0),
            Color.FromArgb(64, 224, 208),
            Color.FromArgb(160, 32, 240),
            Color.FromArgb(154, 205, 50),
            Color.FromArgb(255, 192, 203),
            Color.FromArgb(255, 165, 0),
        };

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
        private readonly IReadOnlyList<SeriesId> _seriesOrder;
        private readonly IReadOnlyDictionary<SeriesId, SeriesStyle> _seriesStyles;
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
            IEnumerable<SeriesId> seriesOrder = null,
            IDictionary<SeriesId, SeriesStyle> seriesStyles = null,
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

            _seriesOrder = new ReadOnlyCollection<SeriesId>(
                [.. seriesOrder ?? []]);

            _seriesStyles = seriesStyles != null
                ? new ReadOnlyDictionary<SeriesId, SeriesStyle>(seriesStyles)
                : new ReadOnlyDictionary<SeriesId, SeriesStyle>(new Dictionary<SeriesId, SeriesStyle>());

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

        public IReadOnlyList<SeriesId> SeriesOrder
        {
            get { return _seriesOrder; }
        }

        public IReadOnlyDictionary<SeriesId, SeriesStyle> SeriesStyles
        {
            get { return _seriesStyles; }
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

        public static Color GetDefaultSeriesColor(int index)
        {
            return DefaultSeriesColorPalette[index % DefaultSeriesColorPalette.Length];
        }

        public static IReadOnlyList<IGraphSeriesModel> ResolveSeriesOrder(
            IReadOnlyList<IGraphSeriesModel> graphSeries,
            IReadOnlyList<SeriesId> seriesOrder)
        {
            var orderedSeries = new List<IGraphSeriesModel>();
            if (graphSeries == null || graphSeries.Count == 0)
            {
                return orderedSeries;
            }

            var remainingSeriesById = new Dictionary<SeriesId, IGraphSeriesModel>();
            for (var index = 0; index < graphSeries.Count; index++)
            {
                var series = graphSeries[index];
                if (series != null && series.SeriesId != null && !remainingSeriesById.ContainsKey(series.SeriesId))
                {
                    remainingSeriesById.Add(series.SeriesId, series);
                }
            }

            if (seriesOrder != null)
            {
                for (var index = 0; index < seriesOrder.Count; index++)
                {
                    var seriesId = seriesOrder[index];
                    if (seriesId == null)
                    {
                        continue;
                    }

                    if (remainingSeriesById.TryGetValue(seriesId, out var orderedMatch))
                    {
                        orderedSeries.Add(orderedMatch);
                        remainingSeriesById.Remove(seriesId);
                    }
                }
            }

            for (var index = 0; index < graphSeries.Count; index++)
            {
                var series = graphSeries[index];
                if (series == null || series.SeriesId == null)
                {
                    continue;
                }

                if (remainingSeriesById.ContainsKey(series.SeriesId))
                {
                    orderedSeries.Add(series);
                    remainingSeriesById.Remove(series.SeriesId);
                }
            }

            return orderedSeries;
        }

        public static GraphPresentationOptions EnsureSeriesStyles(
            IGraphModel graphModel,
            GraphPresentationOptions options)
        {
            options = options ?? new GraphPresentationOptions();

            var graphSeries = graphModel != null ? graphModel.Series : null;
            if (graphSeries == null || graphSeries.Count == 0)
            {
                return options;
            }

            var orderedSeries = ResolveSeriesOrder(graphSeries, options.SeriesOrder);
            var seriesStyles = new Dictionary<SeriesId, SeriesStyle>();
            var nextPaletteIndex = 0;

            if (options.SeriesStyles != null)
            {
                foreach (var existingStyle in options.SeriesStyles)
                {
                    if (existingStyle.Key == null || existingStyle.Value == null)
                    {
                        continue;
                    }

                    seriesStyles[existingStyle.Key] = new SeriesStyle
                    {
                        HasLabelOverride = existingStyle.Value.HasLabelOverride,
                        Label = existingStyle.Value.Label,
                        Color = existingStyle.Value.Color
                    };
                    nextPaletteIndex++;
                }
            }

            for (var index = 0; index < orderedSeries.Count; index++)
            {
                var series = orderedSeries[index];
                if (series == null || series.SeriesId == null || seriesStyles.ContainsKey(series.SeriesId))
                {
                    continue;
                }

                seriesStyles[series.SeriesId] = new SeriesStyle
                {
                    HasLabelOverride = false,
                    Label = null,
                    Color = GetDefaultSeriesColor(nextPaletteIndex)
                };
                nextPaletteIndex++;
            }

            return new GraphPresentationOptions(
                hiddenSeriesIds: options.HiddenSeriesIds,
                hiddenAxisIds: options.HiddenAxisIds,
                graphTitle: options.GraphTitle,
                graphSubtitle: options.GraphSubtitle,
                annotations: options.Annotations,
                showGraphBorder: options.ShowGraphBorder,
                legendPlacement: options.LegendPlacement,
                resizeChart: options.ResizeChart,
                axisEndpointInsetMode: options.AxisEndpointInsetMode,
                axisEndpointInsetFixedValue: options.AxisEndpointInsetFixedValue,
                hiddenAxisGridLineIds: options.HiddenAxisGridLineIds,
                seriesOrder: options.SeriesOrder,
                seriesStyles: seriesStyles,
                axisOverrides: CloneAxisOverrides(options.AxisOverrides),
                enableDenseNumericYAxisTicks: options.EnableDenseNumericYAxisTicks,
                denseNumericYAxisExcludedDimensions: options.DenseNumericYAxisExcludedDimensions != null
                    ? new HashSet<Dimension>(options.DenseNumericYAxisExcludedDimensions)
                    : null);
        }

        private static IDictionary<AxisId, AxisOverrides> CloneAxisOverrides(
            IReadOnlyDictionary<AxisId, AxisOverrides> axisOverrides)
        {
            if (axisOverrides == null)
            {
                return null;
            }

            var clone = new Dictionary<AxisId, AxisOverrides>();
            foreach (var axisOverride in axisOverrides)
            {
                clone[axisOverride.Key] = axisOverride.Value;
            }

            return clone;
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
