using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private const double AxisSlotSize = 0.1;
        private const double AxisStackGap = 0.025;
        private const double TitleHeight = 0.06;
        private const double SubtitleHeight = 0.04;
        private const double TitleSubtitleGap = 0.01;
        private const double LegendBandHeight = 0.12;
        private const double LegendBandWidth = 0.18;
        private const double LegendOuterPaddingX = 0.01;
        private const double LegendOuterPaddingY = 0.015;
        private const double LegendInnerPaddingX = 0.015;
        private const double LegendInnerPaddingY = 0.008;
        private const double LegendEntryGap = 0.01;
        private const double LegendEntryPaddingX = 0.008;
        private const double LegendGlyphWidth = 0.03;
        private const double LegendGlyphHeight = 0.012;

        private readonly IReadOnlyList<SeriesPresentationGeometry> _series;
        private readonly IReadOnlyList<AxisPresentationGeometry> _axes;
        private readonly GraphLayoutModel _layout;
        private readonly GraphSemanticModel _semantics;

        public GraphPresentationModel(IGraphSnapshot snapshot, GraphPresentationOptions options = null)
        {
            options = options ?? new GraphPresentationOptions();
            var seriesContexts = BuildSeriesGeometry(snapshot, options);
            _series = BuildSeriesList(seriesContexts);
            _axes = BuildAxisGeometry(snapshot, seriesContexts, options);
            _layout = BuildLayoutGeometry(_axes, _series, options);
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
                var geometry = new SeriesPresentationGeometry(
                    seriesSnapshot.SeriesId,
                    seriesSnapshot.Label,
                    seriesSnapshot.SeriesType,
                    ResolveConnectivity(seriesSnapshot.SeriesType),
                    points);

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
                var ticks = BuildAxisTicks(axisSnapshot.MinimumValue, axisSnapshot.MaximumValue, orientation, formatter, axisSnapshot.Unit);

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
            AxisOrientation orientation,
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
                var anchor = orientation == AxisOrientation.Horizontal
                    ? new GeometryPoint3D(value, 0d, 0d)
                    : new GeometryPoint3D(0d, value, 0d);
                ticks.Add(new AxisTickPresentation(value, anchor, FormatAxisLabel(formatter, value)));
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

        private static GraphLayoutModel BuildLayoutGeometry(
            IReadOnlyList<AxisPresentationGeometry> axes,
            IReadOnlyList<SeriesPresentationGeometry> series,
            GraphPresentationOptions options = null)
        {
            options = options ?? new GraphPresentationOptions();

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

                entries.Add(new AxisLayoutEntry(axis, AxisSide.Left, leftIndex, normalizedSpanStart, normalizedSpanEnd));
            }

            for (var rightIndex = 0; rightIndex < rightAxes.Count; rightIndex++)
            {
                entries.Add(new AxisLayoutEntry(rightAxes[rightIndex], AxisSide.Right, rightIndex, 0d, 1d));
            }

            for (var bottomIndex = 0; bottomIndex < bottomAxes.Count; bottomIndex++)
            {
                entries.Add(new AxisLayoutEntry(bottomAxes[bottomIndex], AxisSide.Bottom, bottomIndex, 0d, 1d));
            }

            for (var topIndex = 0; topIndex < topAxes.Count; topIndex++)
            {
                entries.Add(new AxisLayoutEntry(topAxes[topIndex], AxisSide.Top, topIndex, 0d, 1d));
            }

            for (var otherIndex = 0; otherIndex < otherAxes.Count; otherIndex++)
            {
                entries.Add(new AxisLayoutEntry(otherAxes[otherIndex], AxisSide.Other, 0, 0d, 1d));
            }

            var rightCount = rightAxes.Count;
            var bottomCount = bottomAxes.Count;
            var topCount = topAxes.Count;
            var hasLegend = series != null && series.Count > 0;
            var legendPlacement = options.LegendPlacement;
            var resizeChart = options.ResizeChart;
            var legendBandHeight = hasLegend && (legendPlacement == LegendPlacement.Bottom || legendPlacement == LegendPlacement.Top)
                ? LegendBandHeight
                : 0d;
            var legendBandWidth = hasLegend && (legendPlacement == LegendPlacement.Left || legendPlacement == LegendPlacement.Right)
                ? LegendBandWidth
                : 0d;
            var reservedLegendHeight = resizeChart ? legendBandHeight : 0d;
            var reservedLegendWidth = resizeChart ? legendBandWidth : 0d;

            // Calculate space reserved for title and subtitle above plot area
            var titleExists = !string.IsNullOrEmpty(options.GraphTitle);
            var subtitleExists = !string.IsNullOrEmpty(options.GraphSubtitle);
            var titleSpaceReserved = 0d;

            if (titleExists)
            {
                titleSpaceReserved += TitleHeight;
            }

            if (subtitleExists)
            {
                titleSpaceReserved += SubtitleHeight;
            }

            if (titleExists || subtitleExists)
            {
                titleSpaceReserved += TitleSubtitleGap;
            }

            // AxisSlotSize represents a fixed outer margin per side, not per axis.
            // Stacked axes affect internal layout only; plot area margins remain constant
            // regardless of how many axes are present on a given side.
            var baseBottomReserved = bottomCount > 0 ? AxisSlotSize : 0d;
            var baseTopReserved = (topCount > 0 ? AxisSlotSize : 0d) + titleSpaceReserved;
            var baseLeftReserved = leftCount > 0 ? AxisSlotSize : 0d;
            var baseRightReserved = rightCount > 0 ? AxisSlotSize : 0d;

            var plotBottom = baseBottomReserved + (legendPlacement == LegendPlacement.Bottom ? reservedLegendHeight : 0d);
            var plotTop = 1.0 - baseTopReserved - (legendPlacement == LegendPlacement.Top ? reservedLegendHeight : 0d);
            var plotLeft = baseLeftReserved + (legendPlacement == LegendPlacement.Left ? reservedLegendWidth : 0d);
            var plotRight = 1.0 - baseRightReserved - (legendPlacement == LegendPlacement.Right ? reservedLegendWidth : 0d);

            var plotArea = new PlotAreaLayout(
                new GeometryPoint3D(plotLeft, plotBottom, 0d),
                new GeometryPoint3D(plotRight, plotTop, 0d));

            var legendGeometry = hasLegend
                ? BuildLegendGeometry(
                    series,
                    legendPlacement,
                    resizeChart,
                    plotArea,
                    legendBandWidth,
                    legendBandHeight)
                : null;

            // Create title and subtitle geometries
            var titleGeometry = titleExists
                ? BuildTitleGeometry(options.GraphTitle, plotArea.TopRight.Y + titleSpaceReserved)
                : null;

            var subtitleGeometry = subtitleExists
                ? BuildSubtitleGeometry(options.GraphSubtitle, titleGeometry, plotArea)
                : null;

            // Create grid lines geometry
            var gridLines = BuildGridLinesGeometry(entries, plotArea);

            return new GraphLayoutModel(
                plotArea,
                new ReadOnlyCollection<AxisLayoutEntry>(entries),
                series,
                titleGeometry,
                subtitleGeometry,
                gridLines,
                legendGeometry);
        }

        private static LegendPresentationGeometry BuildLegendGeometry(
            IReadOnlyList<SeriesPresentationGeometry> series,
            LegendPlacement placement,
            bool resizeChart,
            PlotAreaLayout plotArea,
            double legendBandWidth,
            double legendBandHeight)
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
                        bandLeft = plotArea.BottomLeft.X;
                        bandRight = plotArea.TopRight.X;
                        bandTop = 1.0;
                        bandBottom = bandTop - legendBandHeight;
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
                        bandLeft = 0d;
                        bandRight = legendBandWidth;
                        bandBottom = plotArea.BottomLeft.Y;
                        bandTop = plotArea.TopRight.Y;
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
                        bandRight = 1.0;
                        bandLeft = bandRight - legendBandWidth;
                        bandBottom = plotArea.BottomLeft.Y;
                        bandTop = plotArea.TopRight.Y;
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
                        bandLeft = plotArea.BottomLeft.X;
                        bandRight = plotArea.TopRight.X;
                        bandBottom = 0d;
                        bandTop = legendBandHeight;
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
            var totalGap = entryCount > 1 ? (entryCount - 1) * LegendEntryGap : 0d;
            var entryHeight = (contentTop - contentBottom - totalGap) / entryCount;
            if (entryHeight <= 0d)
            {
                totalGap = 0d;
                entryHeight = (contentTop - contentBottom) / entryCount;
            }

            var entryLeft = contentLeft;
            var entryRight = contentRight;
            var entryWidth = Math.Max(0d, entryRight - entryLeft);
            var glyphHeight = Math.Min(LegendGlyphHeight, entryHeight * 0.7);

            var entries = new List<LegendEntryPresentationGeometry>(entryCount);

            for (var index = 0; index < entryCount; index++)
            {
                var entryTop = contentTop - (index * (entryHeight + totalGap));
                var entryBottom = entryTop - entryHeight;
                var glyphCenterY = entryBottom + (entryHeight * 0.5);
                var glyphBottom = glyphCenterY - (glyphHeight * 0.5);
                var glyphTop = glyphCenterY + (glyphHeight * 0.5);

                var glyphLeft = entryLeft + LegendEntryPaddingX;
                var maxGlyphWidth = Math.Max(0d, entryWidth * 0.28);
                var glyphWidth = Math.Min(LegendGlyphWidth, maxGlyphWidth);
                var glyphRight = Math.Min(glyphLeft + glyphWidth, entryRight - LegendEntryPaddingX);
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
                        new GeometryPoint3D(glyphRight, glyphTop, 0d)));
            }

            return new LegendPresentationGeometry(
                new GeometryPoint3D(containerLeft, containerBottom, 0d),
                new GeometryPoint3D(containerRight, containerTop, 0d),
                new ReadOnlyCollection<LegendEntryPresentationGeometry>(entries),
                showBorder: true);
        }

        private static TitlePresentationGeometry BuildTitleGeometry(string titleText, double topY)
        {
            var titleBottom = topY - TitleHeight;
            var titleTop = topY;

            return new TitlePresentationGeometry(
                titleText,
                new GeometryPoint3D(0d, titleBottom, 0d),
                new GeometryPoint3D(1d, titleTop, 0d));
        }

        private static SubtitlePresentationGeometry BuildSubtitleGeometry(
            string subtitleText,
            TitlePresentationGeometry titleGeometry,
            PlotAreaLayout plotArea)
        {
            var subtitleTop = titleGeometry != null
                ? titleGeometry.BottomLeft.Y - TitleSubtitleGap
                : plotArea.TopRight.Y + SubtitleHeight + TitleSubtitleGap;

            var subtitleBottom = subtitleTop - SubtitleHeight;

            return new SubtitlePresentationGeometry(
                subtitleText,
                new GeometryPoint3D(0d, subtitleBottom, 0d),
                new GeometryPoint3D(1d, subtitleTop, 0d));
        }

        private static GridLinesGeometry BuildGridLinesGeometry(
            IReadOnlyList<AxisLayoutEntry> axisEntries,
            PlotAreaLayout plotArea)
        {
            var verticalLines = new List<GridLineGeometry>();
            var horizontalLines = new List<GridLineGeometry>();

            // Grid lines are rendered inside the plot rectangle. In this geometry,
            // tick-derived positions remain normalized in plot-local space [0,1],
            // and extents must always span the full plot-local bounds.
            const double PlotLocalStart = 0d;
            const double PlotLocalEnd = 1d;

            for (var i = 0; i < axisEntries.Count; i++)
            {
                var entry = axisEntries[i];
                var axis = entry.Axis;

                // Vertical grid lines from X-axis (Bottom/Top) ticks
                if ((entry.Side == AxisSide.Bottom || entry.Side == AxisSide.Top) &&
                    axis.Orientation == AxisOrientation.Horizontal)
                {
                    if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
                    {
                        continue;
                    }

                    var domainMin = axis.MinimumValue.Value;
                    var domainMax = axis.MaximumValue.Value;
                    var domainRange = domainMax - domainMin;

                    var ticks = axis.Ticks;
                    for (var tickIndex = 0; tickIndex < ticks.Count; tickIndex++)
                    {
                        var tick = ticks[tickIndex];
                        // Normalize domain value to [0, 1] within axis bounds
                        var normalizedX = Math.Abs(domainRange) > double.Epsilon
                            ? (tick.Value - domainMin) / domainRange
                            : 0.5;

                        // Clip to plot-local bounds.
                        if (normalizedX >= PlotLocalStart && normalizedX <= PlotLocalEnd)
                        {
                            verticalLines.Add(new GridLineGeometry(
                                AxisOrientation.Vertical,
                                new GeometryPoint3D(normalizedX, PlotLocalStart, 0d),
                                new GeometryPoint3D(normalizedX, PlotLocalEnd, 0d)));
                        }
                    }
                }

                // Horizontal grid lines from Y-axis (Left/Right) ticks
                if ((entry.Side == AxisSide.Left || entry.Side == AxisSide.Right) &&
                    axis.Orientation == AxisOrientation.Vertical)
                {
                    if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
                    {
                        continue;
                    }

                    var domainMin = axis.MinimumValue.Value;
                    var domainMax = axis.MaximumValue.Value;
                    var domainRange = domainMax - domainMin;
                    var spanStart = entry.NormalizedSpanStart;
                    var spanEnd = entry.NormalizedSpanEnd;
                    var spanHeight = spanEnd - spanStart;

                    if (spanHeight <= 0d)
                    {
                        continue;
                    }

                    var ticks = axis.Ticks;
                    for (var tickIndex = 0; tickIndex < ticks.Count; tickIndex++)
                    {
                        var tick = ticks[tickIndex];
                        // Normalize domain value to [0,1] within the owning axis domain,
                        // then map into this axis' allocated vertical span.
                        var axisRelativeY = Math.Abs(domainRange) > double.Epsilon
                            ? (tick.Value - domainMin) / domainRange
                            : 0.5;

                        var normalizedY = spanStart + (axisRelativeY * spanHeight);

                        // Clip to the owning axis span so lines do not bleed into other stacked bands.
                        if (normalizedY >= spanStart && normalizedY <= spanEnd)
                        {
                            horizontalLines.Add(new GridLineGeometry(
                                AxisOrientation.Horizontal,
                                new GeometryPoint3D(PlotLocalStart, normalizedY, 0d),
                                new GeometryPoint3D(PlotLocalEnd, normalizedY, 0d)));
                        }
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
