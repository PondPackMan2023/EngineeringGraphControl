using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    public sealed partial class GraphPresentationModel
    {
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
                PlotAreaLayout plotArea)
        {
            if (titleHeight <= 0d)
            {
                return null;
            }

            var titleBottom = topY - titleHeight;
            var titleTop = topY;

            return new TitlePresentationGeometry(
                titleText,
                new GeometryPoint3D(plotArea.BottomLeft.X, titleBottom, 0d),
                new GeometryPoint3D(plotArea.TopRight.X, titleTop, 0d));
        }

        private static SubtitlePresentationGeometry BuildSubtitleGeometry(
            string subtitleText,
            TitlePresentationGeometry titleGeometry,
            PlotAreaLayout plotArea,
            double subtitleHeight)
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
                new GeometryPoint3D(plotArea.BottomLeft.X, subtitleBottom, 0d),
                new GeometryPoint3D(plotArea.TopRight.X, subtitleTop, 0d));
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
    }
}
