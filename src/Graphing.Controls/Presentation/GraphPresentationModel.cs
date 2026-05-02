using System;
using System.Collections.Generic;
using System.Drawing;
using Graphing.Controls.Rendering.Geometry;
using Graphing.Controls.Snapshot;

namespace Graphing.Controls.Presentation
{
    public sealed partial class GraphPresentationModel
    {
        private const int MaxLeftAxisCount = 6;
        private const double AxisStackGap = 0.06;
        private const double SideBandSiblingGap = 0.0025;
        private const double EdgePaddingThickness = 0.012;
        private const double LegendBoundaryEpsilon = 1e-6;
        private const double DefaultAxisLineThickness = 0.0015;
        private const double AxisHitInflationFactor = 0.5;
        private const double HorizontalAxisMinimumInteractionHalfThickness = 0.010;
        private const double VerticalAxisMinimumInteractionHalfThickness = 0.003;
        // Minimum interaction half-height for horizontal-axis affordance regions.
        // Defined in normalized abstract geometry space and isolated as policy for future configurability.
        private const double HorizontalAxisInteractionAffordanceHalfHeight = 0.0425;
        private const double VerticalAxisAffordanceHalfThickness = 0.0425;
        private const double VerticalAxisEndpointInsetAutoFactor = 0.60;
        private const double HorizontalAxisEndpointInsetAutoFactor = 0.85;
        private const double TitleHeight = 0.06;
        private const double SubtitleHeight = 0.04;
        private const double TitleSubtitleGap = 0.01;
        // Axis protected-band sizing estimates in normalized space.
        private const double AxisBandMinimum = 0.02;
        private const double AxisTickMarkExtentEstimate = 0.005;
        private const double AxisTickMarkVerticalExtentEstimate = 0.015;
        private const double AxisTickLabelOffsetEstimate = 0.0045;
        private const double AxisTickLabelCharWidthEstimate = 0.007;
        private const double AxisTickLabelHeightEstimate = 0.022;
        private const double AxisTitleOffsetEstimate = 0.005;
        private const double AxisVerticalTitleThicknessEstimate = 0.022;
        private const double AxisHorizontalTitleHeightEstimate = 0.028;
        private const double AxisBandOuterPadding = 0.0035;
        private const double MinPlotAreaWidth = 0.10;
        private const double MinPlotAreaHeight = 0.10;
        // Legend sizing — content-driven; these are structural constraints, not hard-coded sizes.
        private const double LegendOuterPaddingX = 0.006;
        private const double LegendOuterPaddingY = 0.006;
        private const double LegendInnerPaddingX = 0.010;
        private const double LegendInnerPaddingY = 0.004;
        private const double LegendEntryGap = 0.008;
        private const double LegendEntryPaddingX = 0.005;
        private const double LegendGlyphWidth = 0.030;
        private const double LegendGlyphHeight = 0.010;
        // Estimated width per character in normalized [0,1] X space, calibrated for Arial 8pt
        // at ~700px control width (1 char ≈ 5.5px → 5.5/700 ≈ 0.008).
        private const double LegendCharWidthEstimate = 0.008;
        // Estimated height of one entry row in normalized [0,1] Y space.
        // Arial 8pt ≈ 11px; at ~600px height with ~1.6× line spacing ≈ 0.029.
        private const double LegendEntryHeightEstimate = 0.030;
        // Absolute fallback minimums — only used when content estimate is smaller.
        private const double LegendMinBandWidth = 0.05;
        private const double LegendMinBandHeight = 0.04;

        internal static Color ResolveSeriesColor(
            ISeriesSnapshot seriesSnapshot,
            int visibleSeriesIndex,
            GraphPresentationOptions options)
        {
            var resolvedPaletteColor = GraphPresentationOptions.GetDefaultSeriesColor(visibleSeriesIndex);

            if (seriesSnapshot == null || seriesSnapshot.SeriesId == null || options == null || options.SeriesStyles == null)
            {
                return resolvedPaletteColor;
            }

            SeriesStyle seriesStyle;
            if (options.SeriesStyles.TryGetValue(seriesSnapshot.SeriesId, out seriesStyle)
                && seriesStyle != null)
            {
                return seriesStyle.Color;
            }

            return resolvedPaletteColor;
        }

        private readonly IReadOnlyList<SeriesPresentationGeometry> _series;
        private readonly IReadOnlyList<AxisPresentationGeometry> _axes;
        private readonly GraphLayoutModel _layout;
        private readonly GraphSemanticModel _semantics;

        public GraphPresentationModel(
            IGraphSnapshot snapshot,
            GraphPresentationOptions options = null,
            IGraphLayoutMeasurementInput measurementInput = null)
        {
            options = options ?? new GraphPresentationOptions();
            measurementInput = measurementInput ?? new DefaultLayoutMeasurementInput();
            var seriesContexts = BuildSeriesGeometry(snapshot, options);
            _axes = BuildAxisGeometry(snapshot, seriesContexts, options);
            var initialSeriesList = BuildSeriesList(seriesContexts);
            _layout = BuildLayoutGeometry(_axes, initialSeriesList, options, measurementInput);
            BindSeriesAxisEntries(seriesContexts, _layout.Axes);
            _series = initialSeriesList;
            _semantics = BuildSemanticModel(snapshot, seriesContexts, _axes, options);
        }

        public IReadOnlyList<SeriesPresentationGeometry> Series
        {
            get { return _series; }
        }

        public IReadOnlyList<AxisPresentationGeometry> Axes
        {
            get { return _axes; }
        }

        public GraphLayoutModel Layout
        {
            get { return _layout; }
        }

        public GraphSemanticModel Semantics
        {
            get { return _semantics; }
        }

        /// <summary>
        /// Resolves the axis layout entry hit by an abstract normalized point, or null when no axis is hit.
        /// Uses only Presentation Model hit regions and stable layout order.
        /// </summary>
        public AxisLayoutEntry ResolveHitAxis(GeometryPoint3D point)
        {
            var axisId = _layout.ResolveHitAxisId(point);
            if (string.IsNullOrWhiteSpace(axisId))
            {
                return null;
            }

            for (var i = 0; i < _layout.Axes.Count; i++)
            {
                var entry = _layout.Axes[i];
                if (entry != null && entry.Axis != null && string.Equals(entry.Axis.AxisId, axisId, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the axis identifier hit by an abstract normalized point, or null when no axis is hit.
        /// </summary>
        public string ResolveHitAxisId(GeometryPoint3D point)
        {
            return _layout.ResolveHitAxisId(point);
        }

        /// <summary>
        /// Convenience overload for normalized abstract X/Y coordinates.
        /// </summary>
        public AxisLayoutEntry ResolveHitAxis(double x, double y)
        {
            return ResolveHitAxis(new GeometryPoint3D(x, y, 0d));
        }

        /// <summary>
        /// Convenience overload for normalized abstract X/Y coordinates.
        /// </summary>
        public string ResolveHitAxisId(double x, double y)
        {
            return ResolveHitAxisId(new GeometryPoint3D(x, y, 0d));
        }

        /// <summary>
        /// Resolves an interaction-safe descriptor for an axis hit at the abstract normalized point.
        /// Returns null when no axis hit region contains the point.
        /// </summary>
        public AxisInteractionDescriptor ResolveAxisInteraction(GeometryPoint3D point)
        {
            var hitRegion = _layout.ResolveHitAxisRegion(point);
            if (hitRegion == null)
            {
                return null;
            }

            AxisLayoutEntry axisEntry = null;
            for (var i = 0; i < _layout.Axes.Count; i++)
            {
                var entry = _layout.Axes[i];
                if (entry != null
                    && entry.Axis != null
                    && string.Equals(entry.Axis.AxisId, hitRegion.AxisId, StringComparison.Ordinal))
                {
                    axisEntry = entry;
                    break;
                }
            }

            if (axisEntry == null)
            {
                return null;
            }

            var normalizedPosition = ComputeNormalizedPositionAlongAxis(hitRegion, point);
            return new AxisInteractionDescriptor(
                axisEntry.Axis.AxisId,
                axisEntry.Axis.Orientation,
                axisEntry.Side,
                axisEntry.SideIndex,
                normalizedPosition,
                   axisEntry.Axis.Formatter,
                axisEntry.Axis.DisplayUnit);
        }

        /// <summary>
        /// Convenience overload for normalized abstract X/Y coordinates.
        /// </summary>
        public AxisInteractionDescriptor ResolveAxisInteraction(double x, double y)
        {
            return ResolveAxisInteraction(new GeometryPoint3D(x, y, 0d));
        }

        private static double ComputeNormalizedPositionAlongAxis(
            AxisHitRegionGeometry hitRegion,
            GeometryPoint3D point)
        {
            if (hitRegion.Orientation == AxisOrientation.Horizontal)
            {
                return NormalizeAlongSpan(point.X, hitRegion.BottomLeft.X, hitRegion.TopRight.X);
            }

            return NormalizeAlongSpan(point.Y, hitRegion.BottomLeft.Y, hitRegion.TopRight.Y);
        }

        private static double NormalizeAlongSpan(double value, double start, double end)
        {
            var span = end - start;
            if (span <= 0d)
            {
                return 0d;
            }

            var normalized = (value - start) / span;
            if (normalized < 0d)
            {
                return 0d;
            }

            if (normalized > 1d)
            {
                return 1d;
            }

            return normalized;
        }

        private sealed class DefaultLayoutMeasurementInput : IGraphLayoutMeasurementInput
        {
            public double MeasureAxisTickThickness(AxisSide side, IReadOnlyList<AxisTickPresentation> ticks)
            {
                var maxTickLabelLength = 0;
                for (var i = 0; i < ticks.Count; i++)
                {
                    var len = (ticks[i].Label ?? string.Empty).Length;
                    if (len > maxTickLabelLength)
                    {
                        maxTickLabelLength = len;
                    }
                }

                if (side == AxisSide.Left || side == AxisSide.Right)
                {
                    return AxisTickLabelOffsetEstimate + (maxTickLabelLength * AxisTickLabelCharWidthEstimate);
                }

                return maxTickLabelLength > 0 ? AxisTickLabelOffsetEstimate + AxisTickLabelHeightEstimate : 0d;
            }

            public double MeasureAxisTitleThickness(AxisSide side, string title)
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return 0d;
                }

                return side == AxisSide.Left || side == AxisSide.Right
                    ? AxisTitleOffsetEstimate + AxisVerticalTitleThicknessEstimate
                    : AxisTitleOffsetEstimate + AxisHorizontalTitleHeightEstimate;
            }

            public double MeasureAxisEndpointLabelExtent(AxisSide side, IReadOnlyList<AxisTickPresentation> ticks)
            {
                var maxTickLabelLength = 0;
                for (var i = 0; i < ticks.Count; i++)
                {
                    var len = (ticks[i].Label ?? string.Empty).Length;
                    if (len > maxTickLabelLength)
                    {
                        maxTickLabelLength = len;
                    }
                }

                if (side == AxisSide.Left || side == AxisSide.Right)
                {
                    return AxisTickLabelHeightEstimate;
                }

                return maxTickLabelLength * AxisTickLabelCharWidthEstimate;
            }

            public LegendMeasurementAdvice MeasureLegend(
                LegendPlacement placement,
                IReadOnlyList<SeriesPresentationGeometry> series,
                double availablePrimarySpan)
            {
                if (series == null || series.Count == 0)
                {
                    return new LegendMeasurementAdvice(0d, 0d, 0d, 0d, 0, 0);
                }

                var itemWidth = 0d;
                for (var i = 0; i < series.Count; i++)
                {
                    var width = ComputeLegendEntryWidth(series[i]);
                    if (width > itemWidth)
                    {
                        itemWidth = width;
                    }
                }

                if (placement == LegendPlacement.Left || placement == LegendPlacement.Right)
                {
                    var contentHeight = Math.Max(0d, availablePrimarySpan - (2 * LegendOuterPaddingY) - (2 * LegendInnerPaddingY));
                    var rowsPerColumn = 1;
                    if (contentHeight > 0d)
                    {
                        rowsPerColumn = Math.Max(1, (int)Math.Floor((contentHeight + LegendEntryGap) / (LegendEntryHeightEstimate + LegendEntryGap)));
                    }

                    var columnCount = (int)Math.Ceiling(series.Count / (double)rowsPerColumn);
                    var requiredThickness = (2 * LegendOuterPaddingX)
                        + (2 * LegendInnerPaddingX)
                        + (columnCount * itemWidth)
                        + (columnCount > 1 ? (columnCount - 1) * LegendEntryGap : 0d);

                    return new LegendMeasurementAdvice(
                        requiredThickness,
                        itemWidth,
                        LegendEntryHeightEstimate,
                        availablePrimarySpan,
                        rowsPerColumn,
                        columnCount);
                }

                var contentWidth = Math.Max(0d, availablePrimarySpan - (2 * LegendOuterPaddingX) - (2 * LegendInnerPaddingX));

                var itemsPerRow = 1;
                if (contentWidth > 0d && itemWidth > 0d)
                {
                    itemsPerRow = Math.Max(1, (int)Math.Floor((contentWidth + LegendEntryGap) / (itemWidth + LegendEntryGap)));
                }

                var rowCount = (int)Math.Ceiling(series.Count / (double)itemsPerRow);
                var requiredHeight = (2 * LegendOuterPaddingY)
                    + (2 * LegendInnerPaddingY)
                    + (rowCount * LegendEntryHeightEstimate)
                    + (rowCount > 1 ? (rowCount - 1) * LegendEntryGap : 0d);

                return new LegendMeasurementAdvice(
                    requiredHeight,
                    itemWidth,
                    LegendEntryHeightEstimate,
                    availablePrimarySpan,
                    itemsPerRow,
                    rowCount);
            }

            public double MeasureTitleThickness(string text, bool isSubtitle)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return 0d;
                }

                return isSubtitle ? SubtitleHeight : TitleHeight;
            }
        }

        private sealed class SeriesGeometryContext
        {
            public SeriesGeometryContext(
                ISeriesSnapshot source,
                SeriesPresentationGeometry geometry)
            {
                Source = source;
                Geometry = geometry;
            }

            public ISeriesSnapshot Source { get; }
            public SeriesPresentationGeometry Geometry { get; }
        }
    }
}
