using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Rendering.Geometry;
using Graphing.Controls.Snapshot;
using UnitRegistry.Formatting;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

namespace Graphing.Controls.Presentation
{
    public sealed class GraphPresentationModel
    {
        private const int MaxLeftAxisCount = 6;
        private const double AxisStackGap = 0.025;
        private const double SideBandSiblingGap = 0.0025;
        private const double EdgePaddingThickness = 0.012;
        private const double LegendBoundaryEpsilon = 1e-6;
            private const double VerticalAxisEndpointInsetAutoFactor = 0.60;
        private const double HorizontalAxisEndpointInsetAutoFactor = 0.85;
        private const double TitleHeight = 0.06;
        private const double SubtitleHeight = 0.04;
        private const double TitleSubtitleGap = 0.01;
        // Axis protected-band sizing estimates in normalized space.
        private const double AxisBandMinimum = 0.02;
        private const double AxisTickMarkExtentEstimate = 0.008;
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

        // Tableau 10 palette — perceptually distinct, professionally appropriate.
        private static readonly Color[] SeriesColorPalette =
        {
            Color.FromArgb(0x1F, 0x77, 0xB4),  // steel blue
            Color.FromArgb(0xFF, 0x7F, 0x0E),  // orange
            Color.FromArgb(0x2C, 0xA0, 0x2C),  // green
            Color.FromArgb(0xD6, 0x27, 0x28),  // red
            Color.FromArgb(0x94, 0x67, 0xBD),  // purple
            Color.FromArgb(0x8C, 0x56, 0x4B),  // brown
            Color.FromArgb(0xE3, 0x77, 0xC2),  // pink
            Color.FromArgb(0x7F, 0x7F, 0x7F),  // gray
            Color.FromArgb(0xBC, 0xBD, 0x22),  // yellow-green
            Color.FromArgb(0x17, 0xBE, 0xCF),  // teal
        };

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

        private static IReadOnlyList<SeriesGeometryContext> BuildSeriesGeometry(
            IGraphSnapshot snapshot,
            GraphPresentationOptions options)
        {
            var result = new List<SeriesGeometryContext>();
            var seriesSnapshots = snapshot != null ? snapshot.Series : null;

            if (seriesSnapshots == null)
            {
                return new ReadOnlyCollection<SeriesGeometryContext>(result);
            }

            for (var seriesIndex = 0; seriesIndex < seriesSnapshots.Count; seriesIndex++)
            {
                var seriesSnapshot = seriesSnapshots[seriesIndex];
                if (seriesSnapshot == null)
                {
                    continue;
                }

                if (!options.IsSeriesVisible(seriesSnapshot))
                {
                    continue;
                }

                var points = BuildPoints(seriesSnapshot.XField, seriesSnapshot.YField);
                var paletteColor = SeriesColorPalette[result.Count % SeriesColorPalette.Length];
                var geometry = new SeriesPresentationGeometry(
                    seriesSnapshot.SeriesId,
                    seriesSnapshot.Label,
                    seriesSnapshot.SeriesType,
                    ResolveConnectivity(seriesSnapshot.SeriesType),
                    points,
                    paletteColor);

                result.Add(new SeriesGeometryContext(seriesSnapshot, geometry));
            }

            return new ReadOnlyCollection<SeriesGeometryContext>(result);
        }

        private static IReadOnlyList<SeriesPresentationGeometry> BuildSeriesList(IReadOnlyList<SeriesGeometryContext> contexts)
        {
            var result = new List<SeriesPresentationGeometry>(contexts.Count);

            for (var index = 0; index < contexts.Count; index++)
            {
                result.Add(contexts[index].Geometry);
            }

            return new ReadOnlyCollection<SeriesPresentationGeometry>(result);
        }

        private static IReadOnlyList<AxisPresentationGeometry> BuildAxisGeometry(
            IGraphSnapshot snapshot,
            IReadOnlyList<SeriesGeometryContext> seriesContexts,
            GraphPresentationOptions options)
        {
            var result = new List<AxisPresentationGeometry>();
            var axisSnapshots = snapshot != null ? snapshot.Axes : null;

            if (axisSnapshots == null)
            {
                return new ReadOnlyCollection<AxisPresentationGeometry>(result);
            }

            for (var axisIndex = 0; axisIndex < axisSnapshots.Count; axisIndex++)
            {
                var axisSnapshot = axisSnapshots[axisIndex];
                if (axisSnapshot == null)
                {
                    continue;
                }

                var identity = BuildAxisIdentity(axisSnapshot);
                if (!options.IsAxisVisible(axisSnapshot))
                {
                    continue;
                }

                var orientation = ResolveAxisOrientation(axisSnapshot.Orientation);
                var side = ResolveAxisSide(axisSnapshot.Side);
                var title = axisSnapshot.Title;
                var formatter = ResolveAxisFormatter(axisSnapshot);
                var linePoints = BuildAxisLine(axisSnapshot.MinimumValue, axisSnapshot.MaximumValue, orientation);
                var ticks = BuildAxisTicks(axisSnapshot.MinimumValue, axisSnapshot.MaximumValue, formatter, axisSnapshot.Unit);

                result.Add(
                    new AxisPresentationGeometry(
                        identity,
                        axisSnapshot.AxisId,
                        side,
                        orientation,
                        title,
                        axisSnapshot.FormatterName,
                        axisSnapshot.DisplayUnitLabel,
                        axisSnapshot.MinimumValue,
                        axisSnapshot.MaximumValue,
                        linePoints,
                        ticks));
            }

            return new ReadOnlyCollection<AxisPresentationGeometry>(result);
        }

        private static IReadOnlyList<GeometryPoint3D> BuildPoints(IFieldSnapshot xField, IFieldSnapshot yField)
        {
            if (xField == null || yField == null)
            {
                return new ReadOnlyCollection<GeometryPoint3D>(new List<GeometryPoint3D>());
            }

            var xValues = xField.Values;
            var yValues = yField.Values;

            if (xValues == null || yValues == null)
            {
                return new ReadOnlyCollection<GeometryPoint3D>(new List<GeometryPoint3D>());
            }

            var pointCount = Math.Min(xValues.Length, yValues.Length);
            var points = new List<GeometryPoint3D>(pointCount);

            for (var index = 0; index < pointCount; index++)
            {
                var xVal = TryToDouble(xValues.GetValue(index));
                var yVal = TryToDouble(yValues.GetValue(index));

                if (double.IsNaN(xVal) || double.IsNaN(yVal))
                {
                    continue;
                }

                points.Add(new GeometryPoint3D(xVal, yVal, 0d));
            }

            return new ReadOnlyCollection<GeometryPoint3D>(points);
        }

        private static AxisOrientation ResolveAxisOrientation(ModelAxisOrientation orientation)
        {
            return orientation == ModelAxisOrientation.X
                ? AxisOrientation.Horizontal
                : AxisOrientation.Vertical;
        }

        private static AxisSide ResolveAxisSide(ModelAxisSide side)
        {
            switch (side)
            {
                case ModelAxisSide.Left:
                    return AxisSide.Left;

                case ModelAxisSide.Right:
                    return AxisSide.Right;

                case ModelAxisSide.Bottom:
                    return AxisSide.Bottom;

                case ModelAxisSide.Top:
                    return AxisSide.Top;

                default:
                    return AxisSide.Other;
            }
        }

        private static string BuildAxisIdentity(IAxisSnapshot axisSnapshot)
        {
            if (!string.IsNullOrWhiteSpace(axisSnapshot.AxisId))
            {
                return axisSnapshot.AxisId;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}",
                axisSnapshot.FormatterName ?? string.Empty,
                axisSnapshot.DisplayUnitLabel ?? string.Empty);
        }



        private static NumericFormatter ResolveAxisFormatter(IAxisSnapshot axisSnapshot)
        {
            var fields = axisSnapshot.Fields;
            if (fields == null)
            {
                return null;
            }

            for (var index = 0; index < fields.Count; index++)
            {
                if (fields[index] != null && fields[index].Formatter != null)
                {
                    return fields[index].Formatter;
                }
            }

            return null;
        }

        private static IReadOnlyList<GeometryPoint3D> BuildAxisLine(
            double? minimumValue,
            double? maximumValue,
            AxisOrientation orientation)
        {
            var points = new List<GeometryPoint3D>();

            if (!minimumValue.HasValue || !maximumValue.HasValue)
            {
                return new ReadOnlyCollection<GeometryPoint3D>(points);
            }

            if (orientation == AxisOrientation.Horizontal)
            {
                points.Add(new GeometryPoint3D(minimumValue.Value, 0d, 0d));
                points.Add(new GeometryPoint3D(maximumValue.Value, 0d, 0d));
            }
            else
            {
                points.Add(new GeometryPoint3D(0d, minimumValue.Value, 0d));
                points.Add(new GeometryPoint3D(0d, maximumValue.Value, 0d));
            }

            return new ReadOnlyCollection<GeometryPoint3D>(points);
        }

        private static IReadOnlyList<AxisTickPresentation> BuildAxisTicks(
            double? minimumValue,
            double? maximumValue,
            NumericFormatter formatter,
            UnitRegistry.Unit unit)
        {
            var ticks = new List<AxisTickPresentation>();

            if (!minimumValue.HasValue || !maximumValue.HasValue)
            {
                return new ReadOnlyCollection<AxisTickPresentation>(ticks);
            }

            var tickValues = BuildTickValues(minimumValue.Value, maximumValue.Value);

            for (var index = 0; index < tickValues.Count; index++)
            {
                var value = tickValues[index];
                ticks.Add(new AxisTickPresentation(value, FormatAxisLabel(formatter, value)));
            }

            return new ReadOnlyCollection<AxisTickPresentation>(ticks);
        }

        private static IReadOnlyList<double> BuildTickValues(double minimumValue, double maximumValue)
        {
            if (minimumValue == maximumValue)
            {
                return new ReadOnlyCollection<double>(new List<double> { minimumValue });
            }

            const int TickCount = 5;
            var ticks = new List<double>(TickCount);
            var increment = (maximumValue - minimumValue) / (TickCount - 1);

            for (var index = 0; index < TickCount; index++)
            {
                ticks.Add(minimumValue + (increment * index));
            }

            return new ReadOnlyCollection<double>(ticks);
        }

        private static string FormatAxisLabel(NumericFormatter formatter, double value)
        {
            if (formatter != null)
            {
                return formatter.Format(value);
            }

            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static double TryToDouble(object value)
        {
            if (value == null)
            {
                return double.NaN;
            }

            try
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return double.NaN;
            }
        }

        private static SeriesConnectivityIntent ResolveConnectivity(SeriesType seriesType)
        {
            switch (seriesType)
            {
                case SeriesType.Line:
                case SeriesType.Profile:
                case SeriesType.Contour:
                    return SeriesConnectivityIntent.Continuous;

                case SeriesType.Bar:
                    return SeriesConnectivityIntent.Step;

                case SeriesType.Scatter:
                case SeriesType.Shape:
                    return SeriesConnectivityIntent.Discrete;

                case SeriesType.Auto:
                default:
                    return SeriesConnectivityIntent.Unspecified;
            }
        }

        /// <summary>
        /// Computes the content-driven legend band width for Left/Right placements.
        /// Width accounts for the glyph, padding, and the longest series label text.
        /// </summary>
        private static double ComputeLegendBandWidth(IReadOnlyList<SeriesPresentationGeometry> series)
        {
            if (series == null || series.Count == 0)
            {
                return 0d;
            }

            var maxLabelLength = 0;
            for (var i = 0; i < series.Count; i++)
            {
                var len = (series[i].Label ?? string.Empty).Length;
                if (len > maxLabelLength)
                {
                    maxLabelLength = len;
                }
            }

            var estimatedTextWidth = maxLabelLength * LegendCharWidthEstimate;
            var rawWidth = (2 * LegendOuterPaddingX)
                + (2 * LegendInnerPaddingX)
                + LegendGlyphWidth
                + LegendEntryPaddingX
                + estimatedTextWidth;

            return Math.Max(LegendMinBandWidth, rawWidth);
        }

        /// <summary>
        /// Computes the content-driven legend band height for Top/Bottom placements.
        /// Height accounts for the number of entries, entry height, and padding.
        /// </summary>
        private static double ComputeLegendBandHeight(IReadOnlyList<SeriesPresentationGeometry> series)
        {
            if (series == null || series.Count == 0)
            {
                return 0d;
            }

            var entryCount = series.Count;
            var totalGap = entryCount > 1 ? (entryCount - 1) * LegendEntryGap : 0d;
            var rawHeight = (2 * LegendOuterPaddingY)
                + (2 * LegendInnerPaddingY)
                + (entryCount * LegendEntryHeightEstimate)
                + totalGap;

            return Math.Max(LegendMinBandHeight, rawHeight);
        }

        private static double ComputeLegendBandHeightForHorizontalFlow(
            IReadOnlyList<SeriesPresentationGeometry> series,
            double availableBandWidth)
        {
            if (series == null || series.Count == 0)
            {
                return 0d;
            }

            var contentWidth = availableBandWidth
                - (2 * LegendOuterPaddingX)
                - (2 * LegendInnerPaddingX);

            if (contentWidth <= 0d)
            {
                return LegendMinBandHeight;
            }

            var rowCount = 1;
            var currentRowWidth = 0d;
            var placedInCurrentRow = 0;

            for (var i = 0; i < series.Count; i++)
            {
                var entryWidth = ComputeLegendEntryWidth(series[i]);
                var requiredWidth = placedInCurrentRow > 0
                    ? currentRowWidth + LegendEntryGap + entryWidth
                    : entryWidth;

                if (placedInCurrentRow > 0 && requiredWidth > contentWidth)
                {
                    rowCount++;
                    currentRowWidth = entryWidth;
                    placedInCurrentRow = 1;
                }
                else
                {
                    currentRowWidth = requiredWidth;
                    placedInCurrentRow++;
                }
            }

            var totalGap = rowCount > 1 ? (rowCount - 1) * LegendEntryGap : 0d;
            var rawHeight = (2 * LegendOuterPaddingY)
                + (2 * LegendInnerPaddingY)
                + (rowCount * LegendEntryHeightEstimate)
                + totalGap;

            return Math.Max(LegendMinBandHeight, rawHeight);
        }

        private static double ComputeLegendEntryWidth(SeriesPresentationGeometry series)
        {
            var labelLength = (series != null && !string.IsNullOrWhiteSpace(series.Label)
                ? series.Label
                : series != null && series.SeriesId != null
                    ? series.SeriesId.ToString()
                    : string.Empty).Length;

            var textWidth = labelLength * LegendCharWidthEstimate;

            return (2 * LegendEntryPaddingX)
                + LegendGlyphWidth
                + LegendEntryPaddingX
                + textWidth;
        }

        private static double GetMaxLegendEntryWidth(IReadOnlyList<SeriesPresentationGeometry> series)
        {
            var max = 0d;
            if (series == null)
            {
                return max;
            }

            for (var i = 0; i < series.Count; i++)
            {
                var width = ComputeLegendEntryWidth(series[i]);
                if (width > max)
                {
                    max = width;
                }
            }

            return max;
        }

        private static double ComputeAxisBandExtent(IReadOnlyList<AxisPresentationGeometry> axes, AxisSide side)
        {
            var maxExtent = 0d;

            for (var i = 0; i < axes.Count; i++)
            {
                var axis = axes[i];
                if (axis.Side != side)
                {
                    continue;
                }

                var extent = EstimateAxisProtectedExtent(axis, side);
                if (extent > maxExtent)
                {
                    maxExtent = extent;
                }
            }

            return maxExtent;
        }

        private static double EstimateAxisProtectedExtent(AxisPresentationGeometry axis, AxisSide side)
        {
            var maxTickLabelLength = 0;
            var ticks = axis.Ticks;
            for (var i = 0; i < ticks.Count; i++)
            {
                var tickLength = (ticks[i].Label ?? string.Empty).Length;
                if (tickLength > maxTickLabelLength)
                {
                    maxTickLabelLength = tickLength;
                }
            }

            var hasTitle = !string.IsNullOrWhiteSpace(axis.Title);

            if (side == AxisSide.Left || side == AxisSide.Right)
            {
                var tickTextWidth = maxTickLabelLength * AxisTickLabelCharWidthEstimate;
                var titleThickness = hasTitle ? AxisVerticalTitleThicknessEstimate : 0d;
                var titleOffset = hasTitle ? AxisTitleOffsetEstimate : 0d;

                var width = AxisBandOuterPadding
                    + AxisTickMarkExtentEstimate
                    + AxisTickLabelOffsetEstimate
                    + tickTextWidth
                    + titleOffset
                    + titleThickness
                    + AxisBandOuterPadding;

                return Math.Max(AxisBandMinimum, width);
            }

            var tickBlock = maxTickLabelLength > 0
                ? AxisTickLabelOffsetEstimate + AxisTickLabelHeightEstimate
                : 0d;
            var horizontalTitleBlock = hasTitle
                ? AxisTitleOffsetEstimate + AxisHorizontalTitleHeightEstimate
                : 0d;
            var height = AxisBandOuterPadding
                + AxisTickMarkExtentEstimate
                + tickBlock
                + horizontalTitleBlock
                + AxisBandOuterPadding;

            return Math.Max(AxisBandMinimum, height);
        }

        /// <summary>
        /// Computes the final plot area after legend placement has reduced available space.
        /// 
        /// This is called ONCE at the end of layout computation to convert basePlotArea
        /// (which knows only about axis bands and titles) into finalPlotArea (which accounts
        /// for legend band placement in resizeChart mode).
        /// 
        /// In resizeChart mode, legends occupy space at control edges, shrinking the plot area.
        /// In overlay mode, legends sit inside the plot area without shrinking it.
        /// </summary>
        private static PlotAreaLayout ComputeFinalPlotArea(
            PlotAreaLayout basePlotArea,
            bool hasLegend,
            LegendPlacement legendPlacement,
            bool resizeChart,
            double legendBandWidth,
            double legendBandHeight)
        {
            if (!resizeChart || !hasLegend)
            {
                // In overlay mode or with no legend, plot area is unchanged.
                return basePlotArea;
            }

            var finalLeft = basePlotArea.BottomLeft.X;
            var finalRight = basePlotArea.TopRight.X;
            var finalBottom = basePlotArea.BottomLeft.Y;
            var finalTop = basePlotArea.TopRight.Y;

            // Legend band and axis band are independent spatial regions — they do not overlap.
            // The final plot inset is therefore axis-band + gap + legend-band (additive) on each side.
            // Band dimensions are passed in directly (legend geometry stores container coords, not band coords).
            switch (legendPlacement)
            {
                case LegendPlacement.Left:
                    // | legend [0, lw] | gap | axis band | plot |
                    finalLeft = finalLeft + legendBandWidth + SideBandSiblingGap;
                    break;

                case LegendPlacement.Right:
                    // | plot | axis band | gap | legend [1-lw, 1] |
                    finalRight = finalRight - legendBandWidth - SideBandSiblingGap;
                    break;

                case LegendPlacement.Bottom:
                    // | legend [0, lh] | gap | axis band | plot |  (in Y)
                    finalBottom = finalBottom + legendBandHeight + SideBandSiblingGap;
                    break;

                case LegendPlacement.Top:
                    // | plot | gap | legend [plotTop-lh, plotTop] | title zone | top axis |  (in Y)
                    finalTop = finalTop - legendBandHeight - SideBandSiblingGap;
                    break;
            }

            if (finalRight <= finalLeft)
            {
                finalRight = finalLeft;
            }

            if (finalTop <= finalBottom)
            {
                finalTop = finalBottom;
            }

            return new PlotAreaLayout(
                new GeometryPoint3D(finalLeft, finalBottom, 0d),
                new GeometryPoint3D(finalRight, finalTop, 0d));
        }

        private static GraphLayoutModel BuildLayoutGeometry(
            IReadOnlyList<AxisPresentationGeometry> axes,
            IReadOnlyList<SeriesPresentationGeometry> series,
            GraphPresentationOptions options,
            IGraphLayoutMeasurementInput measurementInput)
        {
            var entries = new List<AxisLayoutEntry>();
            var leftAxes = new List<AxisPresentationGeometry>();
            var rightAxes = new List<AxisPresentationGeometry>();
            var bottomAxes = new List<AxisPresentationGeometry>();
            var topAxes = new List<AxisPresentationGeometry>();
            var otherAxes = new List<AxisPresentationGeometry>();

            for (var index = 0; index < axes.Count; index++)
            {
                var axis = axes[index];

                switch (axis.Side)
                {
                    case AxisSide.Left:
                        leftAxes.Add(axis);
                        break;

                    case AxisSide.Right:
                        rightAxes.Add(axis);
                        break;

                    case AxisSide.Bottom:
                        bottomAxes.Add(axis);
                        break;

                    case AxisSide.Top:
                        topAxes.Add(axis);
                        break;

                    default:
                        otherAxes.Add(axis);
                        break;
                }
            }

            var leftCount = Math.Min(leftAxes.Count, MaxLeftAxisCount);
            var totalLeftGap = leftCount > 1 ? (leftCount - 1) * AxisStackGap : 0d;
            var leftSpanHeight = leftCount > 0 ? (1.0 - totalLeftGap) / leftCount : 0d;

            for (var leftIndex = 0; leftIndex < leftCount; leftIndex++)
            {
                var axis = leftAxes[leftIndex];
                var normalizedSpanStart = (leftCount - leftIndex - 1) * (leftSpanHeight + AxisStackGap);
                var normalizedSpanEnd = normalizedSpanStart + leftSpanHeight;
                var endpointInset = ComputeAxisEndpointInset(axis, options, measurementInput);

                entries.Add(new AxisLayoutEntry(axis, AxisSide.Left, leftIndex, normalizedSpanStart, normalizedSpanEnd, endpointInset));
            }

            for (var rightIndex = 0; rightIndex < rightAxes.Count; rightIndex++)
            {
                var axis = rightAxes[rightIndex];
                var endpointInset = ComputeAxisEndpointInset(axis, options, measurementInput);
                entries.Add(new AxisLayoutEntry(axis, AxisSide.Right, rightIndex, 0d, 1d, endpointInset));
            }

            for (var bottomIndex = 0; bottomIndex < bottomAxes.Count; bottomIndex++)
            {
                var axis = bottomAxes[bottomIndex];
                var endpointInset = ComputeAxisEndpointInset(axis, options, measurementInput);
                entries.Add(new AxisLayoutEntry(axis, AxisSide.Bottom, bottomIndex, 0d, 1d, endpointInset));
            }

            for (var topIndex = 0; topIndex < topAxes.Count; topIndex++)
            {
                var axis = topAxes[topIndex];
                var endpointInset = ComputeAxisEndpointInset(axis, options, measurementInput);
                entries.Add(new AxisLayoutEntry(axis, AxisSide.Top, topIndex, 0d, 1d, endpointInset));
            }

            for (var otherIndex = 0; otherIndex < otherAxes.Count; otherIndex++)
            {
                entries.Add(new AxisLayoutEntry(otherAxes[otherIndex], AxisSide.Other, 0, 0d, 1d, 0d));
            }

            var rightCount = rightAxes.Count;
            var bottomCount = bottomAxes.Count;
            var topCount = topAxes.Count;
            var leftEdgePadding = EdgePaddingThickness;
            var rightEdgePadding = EdgePaddingThickness;
            var bottomEdgePadding = EdgePaddingThickness;
            var topEdgePadding = EdgePaddingThickness;
            var hasLegend = series != null && series.Count > 0;
            var legendPlacement = options.LegendPlacement;
            var resizeChart = options.ResizeChart;
            var titleExists = !string.IsNullOrEmpty(options.GraphTitle);
            var subtitleExists = !string.IsNullOrEmpty(options.GraphSubtitle);
            var leftAxisMandatory = leftCount > 0 ? AxisBandOuterPadding + AxisTickMarkExtentEstimate + AxisBandOuterPadding : 0d;
            var rightAxisMandatory = rightCount > 0 ? AxisBandOuterPadding + AxisTickMarkExtentEstimate + AxisBandOuterPadding : 0d;
            var bottomAxisMandatory = bottomCount > 0 ? AxisBandOuterPadding + AxisTickMarkExtentEstimate + AxisBandOuterPadding : 0d;
            var topAxisMandatory = topCount > 0 ? AxisBandOuterPadding + AxisTickMarkExtentEstimate + AxisBandOuterPadding : 0d;

            var leftTickRequest = leftCount > 0
                ? measurementInput.MeasureAxisTickThickness(AxisSide.Left, GetRepresentativeTicks(leftAxes))
                : 0d;
            var rightTickRequest = rightCount > 0
                ? measurementInput.MeasureAxisTickThickness(AxisSide.Right, GetRepresentativeTicks(rightAxes))
                : 0d;
            var bottomTickRequest = bottomCount > 0
                ? measurementInput.MeasureAxisTickThickness(AxisSide.Bottom, GetRepresentativeTicks(bottomAxes))
                : 0d;
            var topTickRequest = topCount > 0
                ? measurementInput.MeasureAxisTickThickness(AxisSide.Top, GetRepresentativeTicks(topAxes))
                : 0d;

            var leftTitleRequest = leftCount > 0
                ? GetMaxAxisTitleThickness(leftAxes, AxisSide.Left, measurementInput)
                : 0d;
            var rightTitleRequest = rightCount > 0
                ? GetMaxAxisTitleThickness(rightAxes, AxisSide.Right, measurementInput)
                : 0d;
            var bottomTitleRequest = bottomCount > 0
                ? GetMaxAxisTitleThickness(bottomAxes, AxisSide.Bottom, measurementInput)
                : 0d;
            var topTitleRequest = topCount > 0
                ? GetMaxAxisTitleThickness(topAxes, AxisSide.Top, measurementInput)
                : 0d;

            var leftTickBandThickness = leftAxisMandatory;
            var rightTickBandThickness = rightAxisMandatory;
            var bottomTickBandThickness = bottomAxisMandatory;
            var topTickBandThickness = topAxisMandatory;
            var leftTitleBandThickness = 0d;
            var rightTitleBandThickness = 0d;
            var bottomTitleBandThickness = 0d;
            var topTitleBandThickness = 0d;

            GrantSide(ref leftTickBandThickness, leftTickRequest, true, rightTickBandThickness, topTickBandThickness, bottomTickBandThickness);
            GrantSide(ref rightTickBandThickness, rightTickRequest, true, leftTickBandThickness, topTickBandThickness, bottomTickBandThickness);
            GrantSide(ref bottomTickBandThickness, bottomTickRequest, false, topTickBandThickness, leftTickBandThickness, rightTickBandThickness);
            GrantSide(ref topTickBandThickness, topTickRequest, false, bottomTickBandThickness, leftTickBandThickness, rightTickBandThickness);

            var leftAxisBandWidth = leftTickBandThickness;
            var rightAxisBandWidth = rightTickBandThickness;
            var bottomAxisBandHeight = bottomTickBandThickness;
            var topAxisBandHeight = topTickBandThickness;

            var titleRequest = titleExists ? measurementInput.MeasureTitleThickness(options.GraphTitle, isSubtitle: false) : 0d;
            var subtitleRequest = subtitleExists
                ? measurementInput.MeasureTitleThickness(options.GraphSubtitle, isSubtitle: true)
                : 0d;
            var chromeToPlotGap = titleExists || subtitleExists ? TitleSubtitleGap : 0d;

            var leftLegendBandWidth = 0d;
            var rightLegendBandWidth = 0d;
            var bottomLegendBandHeight = 0d;
            var topLegendBandHeight = 0d;
            var titleBandHeight = 0d;
            var subtitleBandHeight = 0d;

            var topBottomAvailablePrimarySpan = Math.Max(0d, 1.0 - leftEdgePadding - rightEdgePadding - leftAxisBandWidth - rightAxisBandWidth);
            var leftRightAvailablePrimarySpan = Math.Max(
                0d,
                1.0
                - topEdgePadding
                - bottomEdgePadding
                - topAxisBandHeight
                - bottomAxisBandHeight
                - titleRequest
                - subtitleRequest);
            var legendAvailablePrimarySpan = legendPlacement == LegendPlacement.Left || legendPlacement == LegendPlacement.Right
                ? leftRightAvailablePrimarySpan
                : topBottomAvailablePrimarySpan;
            var legendAdvice = hasLegend
                ? measurementInput.MeasureLegend(legendPlacement, series, legendAvailablePrimarySpan)
                : null;
            var legendRequest = ComputeLegendThicknessRequest(legendPlacement, legendAdvice);

            if (resizeChart && hasLegend)
            {
                switch (legendPlacement)
                {
                    case LegendPlacement.Left:
                        GrantSide(ref leftLegendBandWidth, legendRequest, true, leftAxisBandWidth, topAxisBandHeight, bottomAxisBandHeight);
                        break;
                    case LegendPlacement.Right:
                        GrantSide(ref rightLegendBandWidth, legendRequest, true, rightAxisBandWidth, topAxisBandHeight, bottomAxisBandHeight);
                        break;
                    case LegendPlacement.Top:
                        GrantSide(ref topLegendBandHeight, legendRequest, false, topAxisBandHeight, leftAxisBandWidth, rightAxisBandWidth);
                        break;
                    case LegendPlacement.Bottom:
                        GrantSide(ref bottomLegendBandHeight, legendRequest, false, bottomAxisBandHeight, leftAxisBandWidth, rightAxisBandWidth);
                        break;
                }
            }

            GrantSide(ref leftTitleBandThickness, leftTitleRequest, true, leftTickBandThickness, rightAxisBandWidth, topAxisBandHeight, bottomAxisBandHeight);
            GrantSide(ref rightTitleBandThickness, rightTitleRequest, true, rightTickBandThickness, leftAxisBandWidth, topAxisBandHeight, bottomAxisBandHeight);
            GrantSide(ref bottomTitleBandThickness, bottomTitleRequest, false, bottomTickBandThickness, topAxisBandHeight, leftAxisBandWidth, rightAxisBandWidth);
            GrantSide(ref topTitleBandThickness, topTitleRequest, false, topTickBandThickness, bottomAxisBandHeight, leftAxisBandWidth, rightAxisBandWidth);

            leftAxisBandWidth = leftTickBandThickness + leftTitleBandThickness;
            rightAxisBandWidth = rightTickBandThickness + rightTitleBandThickness;
            bottomAxisBandHeight = bottomTickBandThickness + bottomTitleBandThickness;
            topAxisBandHeight = topTickBandThickness + topTitleBandThickness;

            GrantSide(ref subtitleBandHeight, subtitleRequest, false, topLegendBandHeight, leftAxisBandWidth, rightAxisBandWidth, topAxisBandHeight);
            GrantSide(ref titleBandHeight, titleRequest, false, subtitleBandHeight, leftAxisBandWidth, rightAxisBandWidth, topAxisBandHeight, topLegendBandHeight);

            var leftGap = leftLegendBandWidth > 0d && leftAxisBandWidth > 0d ? SideBandSiblingGap : 0d;
            var rightGap = rightLegendBandWidth > 0d && rightAxisBandWidth > 0d ? SideBandSiblingGap : 0d;
            var bottomGap = bottomLegendBandHeight > 0d && bottomAxisBandHeight > 0d ? SideBandSiblingGap : 0d;
            var topGap = topLegendBandHeight > 0d && (topAxisBandHeight > 0d || titleBandHeight > 0d || subtitleBandHeight > 0d)
                ? SideBandSiblingGap
                : 0d;

            var leftWithdraw = leftEdgePadding + leftLegendBandWidth + leftAxisBandWidth + leftGap;
            var rightWithdraw = rightEdgePadding + rightAxisBandWidth + rightLegendBandWidth + rightGap;
            var bottomWithdraw = bottomEdgePadding + bottomLegendBandHeight + bottomAxisBandHeight + bottomGap;
            var topWithdraw = topEdgePadding + topLegendBandHeight + titleBandHeight + subtitleBandHeight + topAxisBandHeight + topGap + chromeToPlotGap;

            var plotLeft = leftWithdraw;
            var plotRight = 1.0 - rightWithdraw;
            var plotBottom = bottomWithdraw;
            var plotTop = 1.0 - topWithdraw;

            if (plotRight - plotLeft < MinPlotAreaWidth)
            {
                plotRight = plotLeft + MinPlotAreaWidth;
            }

            if (plotTop - plotBottom < MinPlotAreaHeight)
            {
                plotTop = plotBottom + MinPlotAreaHeight;
            }

            plotLeft = Clamp01(plotLeft);
            plotRight = Clamp01(plotRight);
            plotBottom = Clamp01(plotBottom);
            plotTop = Clamp01(plotTop);

            var finalPlotArea = new PlotAreaLayout(
                new GeometryPoint3D(plotLeft, plotBottom, 0d),
                new GeometryPoint3D(plotRight, plotTop, 0d));

            var titleTopY = topLegendBandHeight > 0d
                ? 1.0 - topEdgePadding - topLegendBandHeight - SideBandSiblingGap
                : 1.0 - topEdgePadding;

            var legendBandWidthForBuild = (legendPlacement == LegendPlacement.Left)
                ? (resizeChart ? leftLegendBandWidth : legendRequest)
                : (legendPlacement == LegendPlacement.Right ? (resizeChart ? rightLegendBandWidth : legendRequest) : 0d);
            var legendBandHeightForBuild = (legendPlacement == LegendPlacement.Top)
                ? (resizeChart ? topLegendBandHeight : legendRequest)
                : (legendPlacement == LegendPlacement.Bottom ? (resizeChart ? bottomLegendBandHeight : legendRequest) : 0d);

            var legendGeometry = hasLegend
                ? BuildLegendGeometry(
                    series,
                    legendPlacement,
                    resizeChart,
                    finalPlotArea,
                    leftEdgePadding,
                    rightEdgePadding,
                    topEdgePadding,
                    bottomEdgePadding,
                    leftAxisBandWidth,
                    rightAxisBandWidth,
                    topAxisBandHeight,
                    bottomAxisBandHeight,
                    titleBandHeight + subtitleBandHeight,
                    SideBandSiblingGap,
                    legendBandWidthForBuild,
                    legendBandHeightForBuild,
                    legendAdvice)
                : null;

            // Title and subtitle are immutable chart chrome: anchored once from basePlotArea, never adjusted for legend.
            var titleGeometry = titleExists
                ? BuildTitleGeometry(options.GraphTitle, titleTopY, titleBandHeight, leftEdgePadding, rightEdgePadding)
                : null;

            var subtitleGeometry = subtitleExists
                ? BuildSubtitleGeometry(options.GraphSubtitle, titleGeometry, finalPlotArea, subtitleBandHeight, leftEdgePadding, rightEdgePadding)
                : null;

            var axisTitleBands = BuildAxisTitleBandsGeometry(
                entries,
                finalPlotArea,
                leftAxisBandWidth,
                rightAxisBandWidth,
                bottomAxisBandHeight,
                topAxisBandHeight,
                leftLegendBandWidth,
                rightLegendBandWidth,
                bottomLegendBandHeight,
                topLegendBandHeight,
                leftEdgePadding,
                rightEdgePadding,
                bottomEdgePadding,
                topEdgePadding,
                leftTitleBandThickness,
                leftTickBandThickness,
                rightTitleBandThickness,
                rightTickBandThickness,
                bottomTitleBandThickness,
                bottomTickBandThickness,
                topTitleBandThickness,
                topTickBandThickness);

            // Create grid lines geometry
            var gridLines = BuildGridLinesGeometry(entries, finalPlotArea, options);

            AssertLayoutInvariants(finalPlotArea, axisTitleBands, legendGeometry);

            var edgePaddingBands = BuildEdgePaddingBandsGeometry(
                leftEdgePadding,
                rightEdgePadding,
                bottomEdgePadding,
                topEdgePadding);

            return new GraphLayoutModel(
                finalPlotArea,
                entries,
                series,
                titleGeometry,
                subtitleGeometry,
                gridLines,
                legendGeometry,
                axisTitleBands,
                edgePaddingBands);
        }

        private static IReadOnlyList<AxisTickPresentation> GetRepresentativeTicks(IReadOnlyList<AxisPresentationGeometry> axes)
        {
            var result = new List<AxisTickPresentation>();
            for (var i = 0; i < axes.Count; i++)
            {
                var ticks = axes[i].Ticks;
                for (var tickIndex = 0; tickIndex < ticks.Count; tickIndex++)
                {
                    result.Add(ticks[tickIndex]);
                }
            }

            return new ReadOnlyCollection<AxisTickPresentation>(result);
        }

        private static double GetMaxAxisTitleThickness(
            IReadOnlyList<AxisPresentationGeometry> axes,
            AxisSide side,
            IGraphLayoutMeasurementInput measurementInput)
        {
            var max = 0d;
            for (var i = 0; i < axes.Count; i++)
            {
                var axis = axes[i];
                if (string.IsNullOrWhiteSpace(axis.Title))
                {
                    continue;
                }

                var current = measurementInput.MeasureAxisTitleThickness(side, axis.Title);
                if (current > max)
                {
                    max = current;
                }
            }

            return max;
        }

        private static double ComputeAxisEndpointInset(
            AxisPresentationGeometry axis,
            GraphPresentationOptions options,
            IGraphLayoutMeasurementInput measurementInput)
        {
            if (axis == null || options == null)
            {
                return 0d;
            }

            if (options.AxisEndpointInsetMode == AxisEndpointInsetMode.None)
            {
                return 0d;
            }

            if (options.AxisEndpointInsetMode == AxisEndpointInsetMode.Fixed)
            {
                return Math.Max(0d, options.AxisEndpointInsetFixedValue);
            }

            var representativeExtent = measurementInput.MeasureAxisEndpointLabelExtent(axis.Side, axis.Ticks);
            var factor = axis.Side == AxisSide.Left || axis.Side == AxisSide.Right
                ? VerticalAxisEndpointInsetAutoFactor
                : HorizontalAxisEndpointInsetAutoFactor;

            return Math.Max(0d, representativeExtent * factor);
        }

        private static void GrantSide(ref double target, double requested, bool horizontal, params double[] others)
        {
            if (requested <= 0d)
            {
                return;
            }

            var used = target;
            for (var i = 0; i < others.Length; i++)
            {
                used += others[i];
            }

            var capacity = horizontal
                ? (1.0 - MinPlotAreaWidth - used)
                : (1.0 - MinPlotAreaHeight - used);
            if (capacity <= 0d)
            {
                return;
            }

            target += Math.Min(requested, capacity);
        }

        private static double ComputeLegendThicknessRequest(
            LegendPlacement placement,
            LegendMeasurementAdvice legendAdvice)
        {
            if (legendAdvice == null)
            {
                return 0d;
            }

            var itemWidth = Math.Max(0d, legendAdvice.ItemWidth);
            var itemHeight = Math.Max(0d, legendAdvice.ItemHeight);

            if (placement == LegendPlacement.Left || placement == LegendPlacement.Right)
            {
                var columnCount = Math.Max(1, legendAdvice.SecondaryLineCount);
                if (itemWidth <= 0d)
                {
                    return Math.Max(0d, legendAdvice.RequiredThickness);
                }

                return (2 * LegendOuterPaddingX)
                    + (2 * LegendInnerPaddingX)
                    + (columnCount * itemWidth)
                    + (columnCount > 1 ? (columnCount - 1) * LegendEntryGap : 0d);
            }

            var rowCount = Math.Max(1, legendAdvice.RowCount);
            if (itemHeight <= 0d)
            {
                return Math.Max(0d, legendAdvice.RequiredThickness);
            }

            return (2 * LegendOuterPaddingY)
                + (2 * LegendInnerPaddingY)
                + (rowCount * itemHeight)
                + (rowCount > 1 ? (rowCount - 1) * LegendEntryGap : 0d);
        }

        private static double Clamp01(double value)
        {
            if (value < 0d)
            {
                return 0d;
            }

            if (value > 1d)
            {
                return 1d;
            }

            return value;
        }

        private static LegendPresentationGeometry BuildLegendGeometry(
            IReadOnlyList<SeriesPresentationGeometry> series,
            LegendPlacement placement,
            bool resizeChart,
            PlotAreaLayout plotArea,
            double leftEdgePadding,
            double rightEdgePadding,
            double topEdgePadding,
            double bottomEdgePadding,
            double leftAxisBandWidth,
            double rightAxisBandWidth,
            double topAxisBandHeight,
            double bottomAxisBandHeight,
            double titleSpaceReserved,
            double siblingGap,
            double legendBandWidth,
            double legendBandHeight,
            LegendMeasurementAdvice legendAdvice)
        {
            if (series == null || series.Count == 0)
            {
                return null;
            }

            double bandLeft;
            double bandRight;
            double bandBottom;
            double bandTop;

            switch (placement)
            {
                case LegendPlacement.Top:
                    if (resizeChart)
                    {
                        // ADR-0004/0005: top legend inserts at the control edge and grows inward.
                        bandTop = 1.0 - topEdgePadding;
                        bandBottom = bandTop - legendBandHeight;
                        bandLeft = leftEdgePadding + leftAxisBandWidth;
                        bandRight = 1.0 - rightEdgePadding - rightAxisBandWidth;
                    }
                    else
                    {
                        bandLeft = plotArea.BottomLeft.X;
                        bandRight = plotArea.TopRight.X;
                        bandTop = plotArea.TopRight.Y;
                        bandBottom = Math.Max(plotArea.BottomLeft.Y, bandTop - legendBandHeight);
                    }
                    break;

                case LegendPlacement.Left:
                    if (resizeChart)
                    {
                        // ADR-0004: Root at control edge and grow inward.
                        // Axis-title band is an inner exclusion boundary.
                        bandLeft = leftEdgePadding;
                        bandRight = bandLeft + legendBandWidth;
                        bandBottom = bottomEdgePadding + bottomAxisBandHeight;
                        bandTop = 1.0 - topEdgePadding - topAxisBandHeight - titleSpaceReserved;
                    }
                    else
                    {
                        bandLeft = plotArea.BottomLeft.X;
                        bandRight = Math.Min(plotArea.TopRight.X, bandLeft + legendBandWidth);
                        bandBottom = plotArea.BottomLeft.Y;
                        bandTop = plotArea.TopRight.Y;
                    }
                    break;

                case LegendPlacement.Right:
                    if (resizeChart)
                    {
                        // ADR-0004: Root at control edge and grow inward.
                        bandRight = 1.0 - rightEdgePadding;
                        bandLeft = bandRight - legendBandWidth;
                        bandBottom = bottomEdgePadding + bottomAxisBandHeight;
                        bandTop = 1.0 - topEdgePadding - topAxisBandHeight - titleSpaceReserved;
                    }
                    else
                    {
                        bandRight = plotArea.TopRight.X;
                        bandLeft = Math.Max(plotArea.BottomLeft.X, bandRight - legendBandWidth);
                        bandBottom = plotArea.BottomLeft.Y;
                        bandTop = plotArea.TopRight.Y;
                    }
                    break;

                case LegendPlacement.Bottom:
                default:
                    if (resizeChart)
                    {
                        // ADR-0004: Root at control edge and grow inward.
                        bandBottom = bottomEdgePadding;
                        bandTop = bandBottom + legendBandHeight;
                        bandLeft = leftEdgePadding + leftAxisBandWidth;
                        bandRight = 1.0 - rightEdgePadding - rightAxisBandWidth;
                    }
                    else
                    {
                        bandLeft = plotArea.BottomLeft.X;
                        bandRight = plotArea.TopRight.X;
                        bandBottom = plotArea.BottomLeft.Y;
                        bandTop = Math.Min(plotArea.TopRight.Y, bandBottom + legendBandHeight);
                    }
                    break;
            }

            if (bandTop <= bandBottom)
            {
                return null;
            }

            if (bandRight <= bandLeft)
            {
                return null;
            }

            // Localized numeric guard: avoid near-boundary touch/overlap from floating-point
            // accumulation without broadening any global spacing.
            if (resizeChart && placement == LegendPlacement.Left)
            {
                bandRight = Math.Max(bandLeft, bandRight - LegendBoundaryEpsilon);
            }

            if (resizeChart && placement == LegendPlacement.Right)
            {
                bandLeft = Math.Min(bandRight, bandLeft + LegendBoundaryEpsilon);
            }

            var containerLeft = bandLeft + LegendOuterPaddingX;
            var containerRight = bandRight - LegendOuterPaddingX;
            var containerBottom = bandBottom + LegendOuterPaddingY;
            var containerTop = bandTop - LegendOuterPaddingY;

            if (containerRight <= containerLeft)
            {
                containerLeft = bandLeft;
                containerRight = bandRight;
            }

            if (containerTop <= containerBottom)
            {
                containerBottom = bandBottom;
                containerTop = bandTop;
            }

            var contentLeft = containerLeft + LegendInnerPaddingX;
            var contentRight = containerRight - LegendInnerPaddingX;
            var contentBottom = containerBottom + LegendInnerPaddingY;
            var contentTop = containerTop - LegendInnerPaddingY;

            if (contentRight <= contentLeft)
            {
                contentLeft = containerLeft;
                contentRight = containerRight;
            }

            if (contentTop <= contentBottom)
            {
                contentBottom = containerBottom;
                contentTop = containerTop;
            }

            var entryCount = series.Count;
            var entries = new List<LegendEntryPresentationGeometry>(entryCount);

            if (placement == LegendPlacement.Top || placement == LegendPlacement.Bottom)
            {
                var contentWidth = Math.Max(0d, contentRight - contentLeft);
                var rowHeight = legendAdvice != null && legendAdvice.ItemHeight > 0d
                    ? legendAdvice.ItemHeight
                    : LegendEntryHeightEstimate;
                var rowGap = LegendEntryGap;
                var currentX = contentLeft;
                var currentRow = 0;
                var itemsInRow = 0;
                var itemsPerRow = legendAdvice != null && legendAdvice.ItemsPerRow > 0
                    ? legendAdvice.ItemsPerRow
                    : int.MaxValue;

                for (var index = 0; index < entryCount; index++)
                {
                    var seriesGeometry = series[index];
                    var entryWidth = legendAdvice != null && legendAdvice.ItemWidth > 0d
                        ? legendAdvice.ItemWidth
                        : ComputeLegendEntryWidth(seriesGeometry);
                    var requiredWidth = itemsInRow > 0
                        ? (currentX - contentLeft) + LegendEntryGap + entryWidth
                        : entryWidth;

                    if ((itemsInRow > 0 && requiredWidth > contentWidth) || itemsInRow >= itemsPerRow)
                    {
                        currentRow++;
                        currentX = contentLeft;
                        itemsInRow = 0;
                    }

                    var entryLeft = currentX;
                    var entryRight = Math.Min(contentRight, entryLeft + entryWidth);
                    var entryTop = contentTop - (currentRow * (rowHeight + rowGap));
                    var entryBottom = Math.Max(contentBottom, entryTop - rowHeight);

                    var glyphCenterY = entryBottom + ((entryTop - entryBottom) * 0.5);
                    var glyphBottom = glyphCenterY - (LegendGlyphHeight * 0.5);
                    var glyphTop = glyphCenterY + (LegendGlyphHeight * 0.5);
                    if (glyphBottom < entryBottom)
                    {
                        glyphBottom = entryBottom;
                    }

                    if (glyphTop > entryTop)
                    {
                        glyphTop = entryTop;
                    }

                    var glyphLeft = entryLeft + LegendEntryPaddingX;
                    var glyphRight = Math.Min(glyphLeft + LegendGlyphWidth, entryRight - LegendEntryPaddingX);
                    if (glyphRight <= glyphLeft)
                    {
                        glyphRight = glyphLeft;
                    }

                    var displayText = !string.IsNullOrWhiteSpace(seriesGeometry.Label)
                        ? seriesGeometry.Label
                        : seriesGeometry.SeriesId != null
                            ? seriesGeometry.SeriesId.ToString()
                            : string.Empty;

                    entries.Add(
                        new LegendEntryPresentationGeometry(
                            seriesGeometry.SeriesId,
                            displayText,
                            new GeometryPoint3D(entryLeft, entryBottom, 0d),
                            new GeometryPoint3D(entryRight, entryTop, 0d),
                            new GeometryPoint3D(glyphLeft, glyphBottom, 0d),
                            new GeometryPoint3D(glyphRight, glyphTop, 0d),
                            seriesGeometry.SeriesColor));

                    currentX = entryRight + LegendEntryGap;
                    itemsInRow++;
                }
            }
            else
            {
                var columnWidth = legendAdvice != null && legendAdvice.ItemWidth > 0d
                    ? legendAdvice.ItemWidth
                    : GetMaxLegendEntryWidth(series);
                var rowHeight = legendAdvice != null && legendAdvice.ItemHeight > 0d
                    ? legendAdvice.ItemHeight
                    : LegendEntryHeightEstimate;
                var rowsPerColumn = legendAdvice != null && legendAdvice.ItemsPerPrimarySpan > 0
                    ? legendAdvice.ItemsPerPrimarySpan
                    : int.MaxValue;

                var currentColumn = 0;
                var itemsInCurrentColumn = 0;
                var currentX = contentLeft;
                var currentY = contentTop;

                for (var index = 0; index < entryCount; index++)
                {
                    if (itemsInCurrentColumn > 0 && (itemsInCurrentColumn >= rowsPerColumn || currentY - rowHeight < contentBottom))
                    {
                        currentColumn++;
                        currentX = contentLeft + (currentColumn * (columnWidth + LegendEntryGap));
                        currentY = contentTop;
                        itemsInCurrentColumn = 0;
                    }

                    var entryLeft = currentX;
                    var entryRight = Math.Min(contentRight, entryLeft + columnWidth);
                    var entryTop = currentY;
                    var entryBottom = Math.Max(contentBottom, entryTop - rowHeight);

                    var glyphCenterY = entryBottom + ((entryTop - entryBottom) * 0.5);
                    var glyphBottom = glyphCenterY - (LegendGlyphHeight * 0.5);
                    var glyphTop = glyphCenterY + (LegendGlyphHeight * 0.5);
                    if (glyphBottom < entryBottom)
                    {
                        glyphBottom = entryBottom;
                    }

                    if (glyphTop > entryTop)
                    {
                        glyphTop = entryTop;
                    }

                    var glyphLeft = entryLeft + LegendEntryPaddingX;
                    var glyphRight = Math.Min(glyphLeft + LegendGlyphWidth, entryRight - LegendEntryPaddingX);
                    if (glyphRight <= glyphLeft)
                    {
                        glyphRight = glyphLeft;
                    }

                    var seriesGeometry = series[index];
                    var displayText = !string.IsNullOrWhiteSpace(seriesGeometry.Label)
                        ? seriesGeometry.Label
                        : seriesGeometry.SeriesId != null
                            ? seriesGeometry.SeriesId.ToString()
                            : string.Empty;

                    entries.Add(
                        new LegendEntryPresentationGeometry(
                            seriesGeometry.SeriesId,
                            displayText,
                            new GeometryPoint3D(entryLeft, entryBottom, 0d),
                            new GeometryPoint3D(entryRight, entryTop, 0d),
                            new GeometryPoint3D(glyphLeft, glyphBottom, 0d),
                            new GeometryPoint3D(glyphRight, glyphTop, 0d),
                            seriesGeometry.SeriesColor));

                    currentY = entryBottom - LegendEntryGap;
                    itemsInCurrentColumn++;
                }
            }

            // ── Placement-specific frame tightening ───────────────────────────────────────────────
            // Rules are per-placement. No unified algorithm.
            //
            // Left/Right: reduce frame height to used content height only; width stays full band
            //             width; frame is top-aligned. Do NOT tighten width.
            //
            // Top/Bottom: border drawn around actual content bounds; centered horizontally in the
            //             band; centered vertically only if content height < band height.
            double frameLeft, frameBottom, frameRight, frameTop;
            double frameContentLeft, frameContentBottom, frameContentRight, frameContentTop;

            if (placement == LegendPlacement.Left || placement == LegendPlacement.Right)
            {
                // Find the actual lowest Y among all placed entries.
                var actualContentBottom = contentTop;
                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i].BottomLeft.Y < actualContentBottom)
                    {
                        actualContentBottom = entries[i].BottomLeft.Y;
                    }
                }

                // Tight container bottom = last-entry bottom minus inner padding, clamped to the band bottom.
                var tightContainerBottom = Math.Max(bandBottom + LegendOuterPaddingY, actualContentBottom - LegendInnerPaddingY);

                frameLeft = containerLeft;
                frameRight = containerRight;
                frameTop = containerTop;
                frameBottom = tightContainerBottom;
                frameContentLeft = contentLeft;
                frameContentRight = contentRight;
                frameContentTop = contentTop;
                frameContentBottom = actualContentBottom;
            }
            else
            {
                // Top/Bottom: content-driven border, horizontally (and conditionally vertically) centered.
                if (entries.Count == 0)
                {
                    frameLeft = containerLeft;
                    frameRight = containerRight;
                    frameTop = containerTop;
                    frameBottom = containerBottom;
                    frameContentLeft = contentLeft;
                    frameContentRight = contentRight;
                    frameContentTop = contentTop;
                    frameContentBottom = contentBottom;
                }
                else
                {
                    // Compute actual entry bounding box (entries are placed left-to-right from contentLeft).
                    var actualLeft = contentRight;
                    var actualRight = contentLeft;
                    var actualTop = contentBottom;
                    var actualBottom = contentTop;
                    for (var i = 0; i < entries.Count; i++)
                    {
                        if (entries[i].BottomLeft.X < actualLeft) actualLeft = entries[i].BottomLeft.X;
                        if (entries[i].TopRight.X > actualRight) actualRight = entries[i].TopRight.X;
                        if (entries[i].TopRight.Y > actualTop) actualTop = entries[i].TopRight.Y;
                        if (entries[i].BottomLeft.Y < actualBottom) actualBottom = entries[i].BottomLeft.Y;
                    }

                    // Tight border = actual entry bounds expanded by inner padding.
                    var tightBorderLeft = actualLeft - LegendInnerPaddingX;
                    var tightBorderRight = actualRight + LegendInnerPaddingX;
                    var tightBorderBottom = actualBottom - LegendInnerPaddingY;
                    var tightBorderTop = actualTop + LegendInnerPaddingY;
                    var tightBorderWidth = tightBorderRight - tightBorderLeft;
                    var tightBorderHeight = tightBorderTop - tightBorderBottom;

                    // Horizontal centering within the band.
                    var bandCenterX = (bandLeft + bandRight) / 2.0;
                    var horizontalOffset = bandCenterX - (tightBorderLeft + tightBorderWidth / 2.0);

                    // Vertical centering only if the tight border fits within the band.
                    var bandHeight = bandTop - bandBottom;
                    var verticalOffset = 0.0;
                    if (tightBorderHeight < bandHeight)
                    {
                        var bandCenterY = (bandBottom + bandTop) / 2.0;
                        verticalOffset = bandCenterY - (tightBorderBottom + tightBorderHeight / 2.0);
                    }

                    // Shift entry positions by the centering offsets.
                    if (Math.Abs(horizontalOffset) > 1e-12 || Math.Abs(verticalOffset) > 1e-12)
                    {
                        var centeredEntries = new List<LegendEntryPresentationGeometry>(entries.Count);
                        for (var i = 0; i < entries.Count; i++)
                        {
                            var e = entries[i];
                            centeredEntries.Add(new LegendEntryPresentationGeometry(
                                e.SeriesId,
                                e.DisplayText,
                                new GeometryPoint3D(e.BottomLeft.X + horizontalOffset, e.BottomLeft.Y + verticalOffset, 0d),
                                new GeometryPoint3D(e.TopRight.X + horizontalOffset, e.TopRight.Y + verticalOffset, 0d),
                                new GeometryPoint3D(e.GlyphBottomLeft.X + horizontalOffset, e.GlyphBottomLeft.Y + verticalOffset, 0d),
                                new GeometryPoint3D(e.GlyphTopRight.X + horizontalOffset, e.GlyphTopRight.Y + verticalOffset, 0d),
                                e.SeriesColor));
                        }

                        entries = centeredEntries;
                    }

                    frameLeft = tightBorderLeft + horizontalOffset;
                    frameRight = tightBorderRight + horizontalOffset;
                    frameBottom = tightBorderBottom + verticalOffset;
                    frameTop = tightBorderTop + verticalOffset;
                    frameContentLeft = actualLeft + horizontalOffset;
                    frameContentRight = actualRight + horizontalOffset;
                    frameContentBottom = actualBottom + verticalOffset;
                    frameContentTop = actualTop + verticalOffset;
                }
            }

            return new LegendPresentationGeometry(
                new GeometryPoint3D(frameLeft, frameBottom, 0d),
                new GeometryPoint3D(frameRight, frameTop, 0d),
                new GeometryPoint3D(frameContentLeft, frameContentBottom, 0d),
                new GeometryPoint3D(frameContentRight, frameContentTop, 0d),
                new ReadOnlyCollection<LegendEntryPresentationGeometry>(entries),
                showBorder: true);
        }

        private static IReadOnlyList<AxisTitleBandGeometry> BuildAxisTitleBandsGeometry(
            IReadOnlyList<AxisLayoutEntry> axisEntries,
            PlotAreaLayout plotArea,
            double leftAxisBandWidth,
            double rightAxisBandWidth,
            double bottomAxisBandHeight,
            double topAxisBandHeight,
            double leftLegendBandWidth,
            double rightLegendBandWidth,
            double bottomLegendBandHeight,
            double topLegendBandHeight,
            double leftEdgePadding,
            double rightEdgePadding,
            double bottomEdgePadding,
            double topEdgePadding,
            double leftTitleBandThickness,
            double leftTickBandThickness,
            double rightTitleBandThickness,
            double rightTickBandThickness,
            double bottomTitleBandThickness,
            double bottomTickBandThickness,
            double topTitleBandThickness,
            double topTickBandThickness)
        {
            var bands = new List<AxisTitleBandGeometry>();
            var leftItems = new List<AxisTitlePresentationItem>();
            var rightItems = new List<AxisTitlePresentationItem>();
            var bottomItems = new List<AxisTitlePresentationItem>();
            var topItems = new List<AxisTitlePresentationItem>();

            var leftBandLeft = leftEdgePadding + leftLegendBandWidth;
            var leftBandRight = leftEdgePadding + leftLegendBandWidth + leftAxisBandWidth;
            var leftTitleRight = leftBandLeft + leftTitleBandThickness;
            var leftTickLeft = leftBandRight - leftTickBandThickness;

            var rightBandLeft = 1.0 - rightEdgePadding - rightLegendBandWidth - rightAxisBandWidth;
            var rightBandRight = 1.0 - rightEdgePadding - rightLegendBandWidth;
            var rightTickRight = rightBandLeft + rightTickBandThickness;
            var rightTitleLeft = rightBandRight - rightTitleBandThickness;

            var bottomBandBottom = bottomEdgePadding + bottomLegendBandHeight;
            var bottomBandTop = bottomEdgePadding + bottomLegendBandHeight + bottomAxisBandHeight;
            var bottomTitleTop = bottomBandBottom + bottomTitleBandThickness;
            var bottomTickBottom = bottomBandTop - bottomTickBandThickness;

            var topBandBottom = 1.0 - topEdgePadding - topLegendBandHeight - topAxisBandHeight;
            var topBandTop = 1.0 - topEdgePadding - topLegendBandHeight;
            var topTickTop = topBandBottom + topTickBandThickness;
            var topTitleBottom = topBandTop - topTitleBandThickness;

            for (var i = 0; i < axisEntries.Count; i++)
            {
                var entry = axisEntries[i];
                var axis = entry.Axis;
                if (string.IsNullOrWhiteSpace(axis.Title))
                {
                    continue;
                }

                switch (entry.Side)
                {
                    case AxisSide.Left:
                    {
                        var y0 = plotArea.BottomLeft.Y + ((plotArea.TopRight.Y - plotArea.BottomLeft.Y) * Clamp01(entry.NormalizedSpanStart));
                        var y1 = plotArea.BottomLeft.Y + ((plotArea.TopRight.Y - plotArea.BottomLeft.Y) * Clamp01(entry.NormalizedSpanEnd));
                        leftItems.Add(new AxisTitlePresentationItem(
                            axis.AxisId,
                            AxisSide.Left,
                            axis.Orientation,
                            axis.Title,
                            new GeometryPoint3D(leftBandLeft, y0, 0d),
                            new GeometryPoint3D(leftBandRight, y1, 0d),
                            new AxisBandRegionGeometry(
                                new GeometryPoint3D(leftBandLeft, y0, 0d),
                                new GeometryPoint3D(Math.Max(leftBandLeft, leftTitleRight), y1, 0d)),
                            new AxisBandRegionGeometry(
                                new GeometryPoint3D(Math.Min(leftBandRight, Math.Max(leftBandLeft, leftTickLeft)), y0, 0d),
                                new GeometryPoint3D(leftBandRight, y1, 0d))));
                        break;
                    }
                    case AxisSide.Right:
                        rightItems.Add(new AxisTitlePresentationItem(
                            axis.AxisId,
                            AxisSide.Right,
                            axis.Orientation,
                            axis.Title,
                            new GeometryPoint3D(rightBandLeft, plotArea.BottomLeft.Y, 0d),
                            new GeometryPoint3D(rightBandRight, plotArea.TopRight.Y, 0d),
                            new AxisBandRegionGeometry(
                                new GeometryPoint3D(Math.Min(rightBandRight, Math.Max(rightBandLeft, rightTitleLeft)), plotArea.BottomLeft.Y, 0d),
                                new GeometryPoint3D(rightBandRight, plotArea.TopRight.Y, 0d)),
                            new AxisBandRegionGeometry(
                                new GeometryPoint3D(rightBandLeft, plotArea.BottomLeft.Y, 0d),
                                new GeometryPoint3D(Math.Max(rightBandLeft, rightTickRight), plotArea.TopRight.Y, 0d))));
                        break;
                    case AxisSide.Bottom:
                        bottomItems.Add(new AxisTitlePresentationItem(
                            axis.AxisId,
                            AxisSide.Bottom,
                            axis.Orientation,
                            axis.Title,
                            new GeometryPoint3D(plotArea.BottomLeft.X, bottomBandBottom, 0d),
                            new GeometryPoint3D(plotArea.TopRight.X, bottomBandTop, 0d),
                            new AxisBandRegionGeometry(
                                new GeometryPoint3D(plotArea.BottomLeft.X, bottomBandBottom, 0d),
                                new GeometryPoint3D(plotArea.TopRight.X, Math.Max(bottomBandBottom, bottomTitleTop), 0d)),
                            new AxisBandRegionGeometry(
                                new GeometryPoint3D(plotArea.BottomLeft.X, Math.Min(bottomBandTop, Math.Max(bottomBandBottom, bottomTickBottom)), 0d),
                                new GeometryPoint3D(plotArea.TopRight.X, bottomBandTop, 0d))));
                        break;
                    case AxisSide.Top:
                        topItems.Add(new AxisTitlePresentationItem(
                            axis.AxisId,
                            AxisSide.Top,
                            axis.Orientation,
                            axis.Title,
                            new GeometryPoint3D(plotArea.BottomLeft.X, topBandBottom, 0d),
                            new GeometryPoint3D(plotArea.TopRight.X, topBandTop, 0d),
                            new AxisBandRegionGeometry(
                                new GeometryPoint3D(plotArea.BottomLeft.X, Math.Min(topBandTop, Math.Max(topBandBottom, topTitleBottom)), 0d),
                                new GeometryPoint3D(plotArea.TopRight.X, topBandTop, 0d)),
                            new AxisBandRegionGeometry(
                                new GeometryPoint3D(plotArea.BottomLeft.X, topBandBottom, 0d),
                                new GeometryPoint3D(plotArea.TopRight.X, Math.Max(topBandBottom, topTickTop), 0d))));
                        break;
                }
            }

            if (leftAxisBandWidth > 0d)
            {
                bands.Add(new AxisTitleBandGeometry(
                    AxisSide.Left,
                    new GeometryPoint3D(leftBandLeft, plotArea.BottomLeft.Y, 0d),
                    new GeometryPoint3D(leftBandRight, plotArea.TopRight.Y, 0d),
                    new AxisBandRegionGeometry(
                        new GeometryPoint3D(leftBandLeft, plotArea.BottomLeft.Y, 0d),
                        new GeometryPoint3D(Math.Max(leftBandLeft, leftTitleRight), plotArea.TopRight.Y, 0d)),
                    new AxisBandRegionGeometry(
                        new GeometryPoint3D(Math.Min(leftBandRight, Math.Max(leftBandLeft, leftTickLeft)), plotArea.BottomLeft.Y, 0d),
                        new GeometryPoint3D(leftBandRight, plotArea.TopRight.Y, 0d)),
                    new ReadOnlyCollection<AxisTitlePresentationItem>(leftItems)));
            }

            if (rightAxisBandWidth > 0d)
            {
                bands.Add(new AxisTitleBandGeometry(
                    AxisSide.Right,
                    new GeometryPoint3D(rightBandLeft, plotArea.BottomLeft.Y, 0d),
                    new GeometryPoint3D(rightBandRight, plotArea.TopRight.Y, 0d),
                    new AxisBandRegionGeometry(
                        new GeometryPoint3D(Math.Min(rightBandRight, Math.Max(rightBandLeft, rightTitleLeft)), plotArea.BottomLeft.Y, 0d),
                        new GeometryPoint3D(rightBandRight, plotArea.TopRight.Y, 0d)),
                    new AxisBandRegionGeometry(
                        new GeometryPoint3D(rightBandLeft, plotArea.BottomLeft.Y, 0d),
                        new GeometryPoint3D(Math.Max(rightBandLeft, rightTickRight), plotArea.TopRight.Y, 0d)),
                    new ReadOnlyCollection<AxisTitlePresentationItem>(rightItems)));
            }

            if (bottomAxisBandHeight > 0d)
            {
                bands.Add(new AxisTitleBandGeometry(
                    AxisSide.Bottom,
                    new GeometryPoint3D(plotArea.BottomLeft.X, bottomBandBottom, 0d),
                    new GeometryPoint3D(plotArea.TopRight.X, bottomBandTop, 0d),
                    new AxisBandRegionGeometry(
                        new GeometryPoint3D(plotArea.BottomLeft.X, bottomBandBottom, 0d),
                        new GeometryPoint3D(plotArea.TopRight.X, Math.Max(bottomBandBottom, bottomTitleTop), 0d)),
                    new AxisBandRegionGeometry(
                        new GeometryPoint3D(plotArea.BottomLeft.X, Math.Min(bottomBandTop, Math.Max(bottomBandBottom, bottomTickBottom)), 0d),
                        new GeometryPoint3D(plotArea.TopRight.X, bottomBandTop, 0d)),
                    new ReadOnlyCollection<AxisTitlePresentationItem>(bottomItems)));
            }

            if (topAxisBandHeight > 0d)
            {
                bands.Add(new AxisTitleBandGeometry(
                    AxisSide.Top,
                    new GeometryPoint3D(plotArea.BottomLeft.X, topBandBottom, 0d),
                    new GeometryPoint3D(plotArea.TopRight.X, topBandTop, 0d),
                    new AxisBandRegionGeometry(
                        new GeometryPoint3D(plotArea.BottomLeft.X, Math.Min(topBandTop, Math.Max(topBandBottom, topTitleBottom)), 0d),
                        new GeometryPoint3D(plotArea.TopRight.X, topBandTop, 0d)),
                    new AxisBandRegionGeometry(
                        new GeometryPoint3D(plotArea.BottomLeft.X, topBandBottom, 0d),
                        new GeometryPoint3D(plotArea.TopRight.X, Math.Max(topBandBottom, topTickTop), 0d)),
                    new ReadOnlyCollection<AxisTitlePresentationItem>(topItems)));
            }

            return new ReadOnlyCollection<AxisTitleBandGeometry>(bands);
        }

        private static TitlePresentationGeometry BuildTitleGeometry(
            string titleText,
            double topY,
            double titleHeight,
            double leftEdgePadding,
            double rightEdgePadding)
        {
            if (titleHeight <= 0d)
            {
                return null;
            }

            var titleBottom = topY - titleHeight;
            var titleTop = topY;

            return new TitlePresentationGeometry(
                titleText,
                new GeometryPoint3D(leftEdgePadding, titleBottom, 0d),
                new GeometryPoint3D(1d - rightEdgePadding, titleTop, 0d));
        }

        private static SubtitlePresentationGeometry BuildSubtitleGeometry(
            string subtitleText,
            TitlePresentationGeometry titleGeometry,
            PlotAreaLayout plotArea,
            double subtitleHeight,
            double leftEdgePadding,
            double rightEdgePadding)
        {
            if (subtitleHeight <= 0d)
            {
                return null;
            }

            // ADR-0003: Geometric positioning in Cartesian (Y-up) space.
            // When title exists: subtitle sits below title (lesser Y), separated by gap.
            // When title does NOT exist: subtitle sits at plot-area top (Y = plotArea.TopRight.Y).
            var subtitleTop = titleGeometry != null
                ? titleGeometry.BottomLeft.Y - TitleSubtitleGap
                : plotArea.TopRight.Y + subtitleHeight;

            var subtitleBottom = subtitleTop - subtitleHeight;

            return new SubtitlePresentationGeometry(
                subtitleText,
                new GeometryPoint3D(leftEdgePadding, subtitleBottom, 0d),
                new GeometryPoint3D(1d - rightEdgePadding, subtitleTop, 0d));
        }

        private static IReadOnlyList<EdgePaddingBandGeometry> BuildEdgePaddingBandsGeometry(
            double leftEdgePadding,
            double rightEdgePadding,
            double bottomEdgePadding,
            double topEdgePadding)
        {
            var bands = new List<EdgePaddingBandGeometry>
            {
                new EdgePaddingBandGeometry(
                    AxisSide.Left,
                    new GeometryPoint3D(0d, 0d, 0d),
                    new GeometryPoint3D(leftEdgePadding, 1d, 0d)),
                new EdgePaddingBandGeometry(
                    AxisSide.Right,
                    new GeometryPoint3D(1d - rightEdgePadding, 0d, 0d),
                    new GeometryPoint3D(1d, 1d, 0d)),
                new EdgePaddingBandGeometry(
                    AxisSide.Bottom,
                    new GeometryPoint3D(0d, 0d, 0d),
                    new GeometryPoint3D(1d, bottomEdgePadding, 0d)),
                new EdgePaddingBandGeometry(
                    AxisSide.Top,
                    new GeometryPoint3D(0d, 1d - topEdgePadding, 0d),
                    new GeometryPoint3D(1d, 1d, 0d)),
            };

            return new ReadOnlyCollection<EdgePaddingBandGeometry>(bands);
        }

        private static void AssertLayoutInvariants(
            PlotAreaLayout plotArea,
            IReadOnlyList<AxisTitleBandGeometry> axisTitleBands,
            LegendPresentationGeometry legend)
        {
            if (plotArea.TopRight.X - plotArea.BottomLeft.X < MinPlotAreaWidth - 1e-9 ||
                plotArea.TopRight.Y - plotArea.BottomLeft.Y < MinPlotAreaHeight - 1e-9)
            {
                throw new InvalidOperationException("Layout invariant violated: plot area below minimum size.");
            }

            if (legend == null || axisTitleBands == null)
            {
                return;
            }

            for (var i = 0; i < axisTitleBands.Count; i++)
            {
                var band = axisTitleBands[i];
                if (RectanglesOverlap(
                        band.BottomLeft.X,
                        band.BottomLeft.Y,
                        band.TopRight.X,
                        band.TopRight.Y,
                        legend.BottomLeft.X,
                        legend.BottomLeft.Y,
                        legend.TopRight.X,
                        legend.TopRight.Y))
                {
                    throw new InvalidOperationException("Layout invariant violated: axis-title band overlaps legend.");
                }
            }
        }

        private static bool RectanglesOverlap(
            double aLeft,
            double aBottom,
            double aRight,
            double aTop,
            double bLeft,
            double bBottom,
            double bRight,
            double bTop)
        {
            return aLeft < bRight && aRight > bLeft && aBottom < bTop && aTop > bBottom;
        }

        private static GridLinesGeometry BuildGridLinesGeometry(
            IReadOnlyList<AxisLayoutEntry> axisEntries,
            PlotAreaLayout plotArea,
            GraphPresentationOptions options)
        {
            var verticalLines = new List<GridLineGeometry>();
            var horizontalLines = new List<GridLineGeometry>();
            var hasXDomain = false;
            var hasYDomain = false;
            var domainMinX = 0d;
            var domainMaxX = 0d;
            var domainMinY = 0d;
            var domainMaxY = 0d;
            var xAxisEntry = (AxisLayoutEntry)null;
            var yAxisEntries = new List<AxisLayoutEntry>();

            for (var i = 0; i < axisEntries.Count; i++)
            {
                var entry = axisEntries[i];
                var axis = entry.Axis;

                if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
                {
                    continue;
                }

                if (axis.Orientation == AxisOrientation.Horizontal &&
                    (entry.Side == AxisSide.Bottom || entry.Side == AxisSide.Top))
                {
                    if (!hasXDomain)
                    {
                        hasXDomain = true;
                        xAxisEntry = entry;
                        domainMinX = axis.MinimumValue.Value;
                        domainMaxX = axis.MaximumValue.Value;
                    }
                    else
                    {
                        domainMinX = Math.Min(domainMinX, axis.MinimumValue.Value);
                        domainMaxX = Math.Max(domainMaxX, axis.MaximumValue.Value);
                    }
                }

                if (axis.Orientation == AxisOrientation.Vertical &&
                    (entry.Side == AxisSide.Left || entry.Side == AxisSide.Right))
                {
                    if (!hasYDomain)
                    {
                        hasYDomain = true;
                        domainMinY = axis.MinimumValue.Value;
                        domainMaxY = axis.MaximumValue.Value;
                    }
                    else
                    {
                        domainMinY = Math.Min(domainMinY, axis.MinimumValue.Value);
                        domainMaxY = Math.Max(domainMaxY, axis.MaximumValue.Value);
                    }

                    yAxisEntries.Add(entry);
                }
            }

            for (var i = 0; i < axisEntries.Count; i++)
            {
                var entry = axisEntries[i];
                var axis = entry.Axis;

                // Vertical grid lines from X-axis ticks in domain space, bound to the X-axis entry.
                if (hasYDomain &&
                    options.IsAxisGridLinesVisible(axis.AxisId) &&
                    axis.Orientation == AxisOrientation.Horizontal &&
                    (entry.Side == AxisSide.Bottom || entry.Side == AxisSide.Top))
                {
                    var ticks = axis.Ticks;
                    for (var tickIndex = 0; tickIndex < ticks.Count; tickIndex++)
                    {
                        var x = ticks[tickIndex].Value;
                        if (x < axis.MinimumValue.Value || x > axis.MaximumValue.Value)
                        {
                            continue;
                        }

                        var line = new GridLineGeometry(
                            AxisOrientation.Vertical,
                            new GeometryPoint3D(x, domainMinY, 0d),
                            new GeometryPoint3D(x, domainMaxY, 0d),
                            xAxisEntry);
                        verticalLines.Add(line);
                    }
                }

                // Horizontal grid lines from Y-axis ticks in domain space, bound to their source Y-axis entry.
                if (hasXDomain &&
                    options.IsAxisGridLinesVisible(axis.AxisId) &&
                    axis.Orientation == AxisOrientation.Vertical &&
                    (entry.Side == AxisSide.Left || entry.Side == AxisSide.Right))
                {
                    var ticks = axis.Ticks;
                    for (var tickIndex = 0; tickIndex < ticks.Count; tickIndex++)
                    {
                        var y = ticks[tickIndex].Value;
                        if (y < axis.MinimumValue.Value || y > axis.MaximumValue.Value)
                        {
                            continue;
                        }

                        var line = new GridLineGeometry(
                            AxisOrientation.Horizontal,
                            new GeometryPoint3D(domainMinX, y, 0d),
                            new GeometryPoint3D(domainMaxX, y, 0d),
                            entry);
                        horizontalLines.Add(line);
                    }
                }
            }

            return new GridLinesGeometry(verticalLines, horizontalLines);
        }

        private static GraphSemanticModel BuildSemanticModel(
            IGraphSnapshot snapshot,
            IReadOnlyList<SeriesGeometryContext> seriesContexts,
            IReadOnlyList<AxisPresentationGeometry> axes,
            GraphPresentationOptions options)
        {
            var legendEntries = BuildLegendEntries(seriesContexts);
            var axisDescriptors = BuildAxisDescriptors(axes);
            var annotations = BuildAnnotations(options, axisDescriptors);

            return new GraphSemanticModel(
                options.GraphTitle,
                options.GraphSubtitle,
                legendEntries,
                annotations,
                axisDescriptors);
        }

        private static IReadOnlyList<LegendEntrySemantic> BuildLegendEntries(IReadOnlyList<SeriesGeometryContext> seriesContexts)
        {
            var entries = new List<LegendEntrySemantic>(seriesContexts.Count);

            for (var index = 0; index < seriesContexts.Count; index++)
            {
                var context = seriesContexts[index];
                var item = context.Geometry;
                var text = item.Label ?? string.Empty;
                entries.Add(new LegendEntrySemantic(item.SeriesId, text));
            }

            return new ReadOnlyCollection<LegendEntrySemantic>(entries);
        }

        private static IReadOnlyList<AxisDescriptorSemantic> BuildAxisDescriptors(IReadOnlyList<AxisPresentationGeometry> axes)
        {
            var descriptors = new List<AxisDescriptorSemantic>(axes.Count);

            for (var index = 0; index < axes.Count; index++)
            {
                var axis = axes[index];

                descriptors.Add(
                    new AxisDescriptorSemantic(
                        axis.Identity,
                        axis.AxisId,
                        BuildAxisCaption(axis),
                        axis.DisplayUnitLabel,
                        axis.FormatterName));
            }

            return new ReadOnlyCollection<AxisDescriptorSemantic>(descriptors);
        }

        private static string BuildAxisCaption(AxisPresentationGeometry axis)
        {
            if (!string.IsNullOrWhiteSpace(axis.Title))
            {
                return axis.Title;
            }

            if (!string.IsNullOrWhiteSpace(axis.DisplayUnitLabel))
            {
                return axis.DisplayUnitLabel;
            }

            if (!string.IsNullOrWhiteSpace(axis.FormatterName))
            {
                return axis.FormatterName;
            }

            return axis.AxisId ?? string.Empty;
        }

        private static IReadOnlyList<AnnotationSemantic> BuildAnnotations(
            GraphPresentationOptions options,
            IReadOnlyList<AxisDescriptorSemantic> axisDescriptors)
        {
            var annotations = new List<AnnotationSemantic>();

            if (!string.IsNullOrWhiteSpace(options.GraphTitle))
            {
                annotations.Add(new AnnotationSemantic(options.GraphTitle, "graph:title"));
            }

            if (!string.IsNullOrWhiteSpace(options.GraphSubtitle))
            {
                annotations.Add(new AnnotationSemantic(options.GraphSubtitle, "graph:subtitle"));
            }

            var providedAnnotations = options.Annotations;
            for (var index = 0; index < providedAnnotations.Count; index++)
            {
                var annotation = providedAnnotations[index];
                if (annotation == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(annotation.Text))
                {
                    continue;
                }

                var anchor = string.IsNullOrWhiteSpace(annotation.Anchor)
                    ? "graph:note"
                    : annotation.Anchor;
                annotations.Add(new AnnotationSemantic(annotation.Text, anchor));
            }

            for (var axisIndex = 0; axisIndex < axisDescriptors.Count; axisIndex++)
            {
                var axis = axisDescriptors[axisIndex];
                annotations.Add(new AnnotationSemantic(axis.Caption, "axis:" + axis.AxisIdentity));
            }

            return new ReadOnlyCollection<AnnotationSemantic>(annotations);
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

        private static void BindSeriesAxisEntries(
            IReadOnlyList<SeriesGeometryContext> seriesContexts,
            IReadOnlyList<AxisLayoutEntry> layoutAxes)
        {
            var axisLookup = new Dictionary<string, AxisLayoutEntry>(StringComparer.Ordinal);

            for (var i = 0; i < layoutAxes.Count; i++)
            {
                var entry = layoutAxes[i];
                var axisId = entry.Axis.AxisId;
                if (!string.IsNullOrEmpty(axisId) && !axisLookup.ContainsKey(axisId))
                {
                    axisLookup[axisId] = entry;
                }
            }

            for (var i = 0; i < seriesContexts.Count; i++)
            {
                var context = seriesContexts[i];
                var xAxisId = context.Source.XAxisId;
                var yAxisId = context.Source.YAxisId;

                if (!string.IsNullOrEmpty(xAxisId))
                {
                    axisLookup.TryGetValue(xAxisId, out var xEntry);
                    context.Geometry.XAxisEntry = xEntry;
                }

                if (!string.IsNullOrEmpty(yAxisId))
                {
                    axisLookup.TryGetValue(yAxisId, out var yEntry);
                    context.Geometry.YAxisEntry = yEntry;
                }
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
