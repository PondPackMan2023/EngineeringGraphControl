using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Graphing.Controls.Rendering.Geometry;
using Graphing.Controls.Snapshot;

namespace Graphing.Controls.Presentation
{
    public sealed partial class GraphPresentationModel
    {
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
                    ? BuildTitleGeometry(options.GraphTitle, titleTopY, titleBandHeight, finalPlotArea)
                : null;

            var subtitleGeometry = subtitleExists
                    ? BuildSubtitleGeometry(options.GraphSubtitle, titleGeometry, finalPlotArea, subtitleBandHeight)
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

            var axisHitRegions = BuildAxisHitRegionsGeometry(
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
                topEdgePadding);

            var axisInteractionAffordanceRegions = BuildAxisInteractionAffordanceGeometry(
                entries,
                finalPlotArea);

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
                edgePaddingBands,
                axisHitRegions,
                axisInteractionAffordanceRegions);
        }

        private static IReadOnlyList<AxisInteractionAffordanceGeometry> BuildAxisInteractionAffordanceGeometry(
            IReadOnlyList<AxisLayoutEntry> axisEntries,
            PlotAreaLayout plotArea)
        {
            var regions = new List<AxisInteractionAffordanceGeometry>();
            var plotLeft = plotArea.BottomLeft.X;
            var plotBottom = plotArea.BottomLeft.Y;
            var plotRight = plotArea.TopRight.X;
            var plotTop = plotArea.TopRight.Y;
            var plotWidth = Math.Max(0d, plotRight - plotLeft);
            var plotHeight = Math.Max(0d, plotTop - plotBottom);

            for (var i = 0; i < axisEntries.Count; i++)
            {
                var entry = axisEntries[i];
                if (entry == null || entry.Axis == null || string.IsNullOrWhiteSpace(entry.Axis.AxisId))
                {
                    continue;
                }

                var inset = Math.Max(0d, entry.TickEndpointInset);
                var spanStart = 0d;
                var spanEnd = 0d;
                var axisLineCoordinate = 0d;

                double left;
                double bottom;
                double right;
                double top;

                if (entry.Axis.Orientation == AxisOrientation.Horizontal)
                {
                    axisLineCoordinate = entry.Side == AxisSide.Top
                        ? plotTop
                        : plotBottom;
                    spanStart = plotLeft + inset;
                    spanEnd = plotRight - inset;

                    if (spanEnd < spanStart)
                    {
                        var center = plotLeft + (plotWidth * 0.5d);
                        spanStart = center;
                        spanEnd = center;
                    }

                    left = spanStart;
                    right = spanEnd;
                    bottom = axisLineCoordinate - HorizontalAxisInteractionAffordanceHalfHeight;
                    top = axisLineCoordinate + HorizontalAxisInteractionAffordanceHalfHeight;
                }
                else
                {
                    var normalizedStart = Math.Max(0d, Math.Min(1d, entry.NormalizedSpanStart));
                    var normalizedEnd = Math.Max(0d, Math.Min(1d, entry.NormalizedSpanEnd));
                    if (normalizedEnd < normalizedStart)
                    {
                        var swap = normalizedStart;
                        normalizedStart = normalizedEnd;
                        normalizedEnd = swap;
                    }

                    spanStart = plotBottom + (normalizedStart * plotHeight) + inset;
                    spanEnd = plotBottom + (normalizedEnd * plotHeight) - inset;
                    if (spanEnd < spanStart)
                    {
                        var center = plotBottom + (((normalizedStart + normalizedEnd) * 0.5d) * plotHeight);
                        spanStart = center;
                        spanEnd = center;
                    }

                    axisLineCoordinate = entry.Side == AxisSide.Right
                        ? plotRight
                        : plotLeft;
                    left = axisLineCoordinate - VerticalAxisAffordanceHalfThickness;
                    right = axisLineCoordinate + VerticalAxisAffordanceHalfThickness;
                    bottom = spanStart;
                    top = spanEnd;
                }

                if (right < left)
                {
                    right = left;
                }

                if (top < bottom)
                {
                    top = bottom;
                }

                regions.Add(
                    new AxisInteractionAffordanceGeometry(
                        entry.Axis.AxisId,
                        entry.Side,
                        entry.Axis.Orientation,
                        new GeometryPoint3D(left, bottom, 0d),
                        new GeometryPoint3D(right, top, 0d)));
            }

            return new ReadOnlyCollection<AxisInteractionAffordanceGeometry>(regions);
        }

        private static IReadOnlyList<AxisHitRegionGeometry> BuildAxisHitRegionsGeometry(
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
            double topEdgePadding)
        {
            var regions = new List<AxisHitRegionGeometry>();

            for (var i = 0; i < axisEntries.Count; i++)
            {
                var entry = axisEntries[i];
                if (entry == null || entry.Axis == null || string.IsNullOrWhiteSpace(entry.Axis.AxisId))
                {
                    continue;
                }

                if (!TryGetAxisBandBounds(
                        entry,
                        plotArea,
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
                        out var bandLeft,
                        out var bandBottom,
                        out var bandRight,
                        out var bandTop))
                {
                    continue;
                }

                var halfHitThickness = ComputeAxisHitHalfThickness(
                    entry.Axis.AxisLineThickness,
                    entry.Axis.Orientation);
                if (halfHitThickness <= 0d)
                {
                    continue;
                }

                var centerX = (bandLeft + bandRight) * 0.5d;
                var centerY = (bandBottom + bandTop) * 0.5d;

                double left;
                double bottom;
                double right;
                double top;

                if (entry.Axis.Orientation == AxisOrientation.Horizontal)
                {
                    left = bandLeft;
                    right = bandRight;
                    bottom = centerY - halfHitThickness;
                    top = centerY + halfHitThickness;
                }
                else
                {
                    left = centerX - halfHitThickness;
                    right = centerX + halfHitThickness;
                    bottom = bandBottom;
                    top = bandTop;
                }

                left = Math.Max(bandLeft, left);
                right = Math.Min(bandRight, right);
                bottom = Math.Max(bandBottom, bottom);
                top = Math.Min(bandTop, top);

                if (right < left)
                {
                    right = left;
                }

                if (top < bottom)
                {
                    top = bottom;
                }

                regions.Add(
                    new AxisHitRegionGeometry(
                        entry.Axis.AxisId,
                        entry.Side,
                        entry.Axis.Orientation,
                        entry.Axis.AxisLineThickness,
                        halfHitThickness,
                        new GeometryPoint3D(left, bottom, 0d),
                        new GeometryPoint3D(right, top, 0d)));
            }

            return new ReadOnlyCollection<AxisHitRegionGeometry>(regions);
        }

        private static bool TryGetAxisBandBounds(
            AxisLayoutEntry entry,
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
            out double left,
            out double bottom,
            out double right,
            out double top)
        {
            left = 0d;
            bottom = 0d;
            right = 0d;
            top = 0d;

            switch (entry.Side)
            {
                case AxisSide.Left:
                    if (leftAxisBandWidth <= 0d)
                    {
                        return false;
                    }

                    left = leftEdgePadding + leftLegendBandWidth;
                    right = left + leftAxisBandWidth;

                    var leftSpanStart = Clamp01(entry.NormalizedSpanStart);
                    var leftSpanEnd = Clamp01(entry.NormalizedSpanEnd);
                    bottom = plotArea.BottomLeft.Y + ((plotArea.TopRight.Y - plotArea.BottomLeft.Y) * leftSpanStart);
                    top = plotArea.BottomLeft.Y + ((plotArea.TopRight.Y - plotArea.BottomLeft.Y) * leftSpanEnd);
                    if (top < bottom)
                    {
                        var temp = top;
                        top = bottom;
                        bottom = temp;
                    }

                    return true;

                case AxisSide.Right:
                    if (rightAxisBandWidth <= 0d)
                    {
                        return false;
                    }

                    left = 1.0 - rightEdgePadding - rightLegendBandWidth - rightAxisBandWidth;
                    right = 1.0 - rightEdgePadding - rightLegendBandWidth;
                    bottom = plotArea.BottomLeft.Y;
                    top = plotArea.TopRight.Y;
                    return true;

                case AxisSide.Bottom:
                    if (bottomAxisBandHeight <= 0d)
                    {
                        return false;
                    }

                    left = plotArea.BottomLeft.X;
                    right = plotArea.TopRight.X;
                    bottom = bottomEdgePadding + bottomLegendBandHeight;
                    top = bottom + bottomAxisBandHeight;
                    return true;

                case AxisSide.Top:
                    if (topAxisBandHeight <= 0d)
                    {
                        return false;
                    }

                    left = plotArea.BottomLeft.X;
                    right = plotArea.TopRight.X;
                    bottom = 1.0 - topEdgePadding - topLegendBandHeight - topAxisBandHeight;
                    top = 1.0 - topEdgePadding - topLegendBandHeight;
                    return true;

                default:
                    return false;
            }
        }

        private static double ComputeAxisHitHalfThickness(
            double axisLineThickness,
            AxisOrientation orientation)
        {
            var visualAxisHalfThickness = Math.Max(0d, axisLineThickness) * AxisHitInflationFactor;
            var minimumInteractionHalfThickness = orientation == AxisOrientation.Horizontal
                ? HorizontalAxisMinimumInteractionHalfThickness
                : VerticalAxisMinimumInteractionHalfThickness;

            return Math.Max(visualAxisHalfThickness, minimumInteractionHalfThickness);
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

            if (axis.Side != AxisSide.Left && axis.Side != AxisSide.Right)
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
            var factor = VerticalAxisEndpointInsetAutoFactor;

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
    }
}
