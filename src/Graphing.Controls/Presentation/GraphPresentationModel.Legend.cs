using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    public sealed partial class GraphPresentationModel
    {
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
    }
}
