using System;
using System.Collections.Generic;
using System.Linq;
using Graphing.Core.Pie.Snapshot;

namespace Graphing.Core.Pie.Presentation
{
    public sealed class PieGraphPresentationBuilder
    {
        private static readonly PieBounds PieAreaWithLegend = new PieBounds(0.05d, 0.10d, 0.70d, 0.90d);
        private static readonly PieBounds PieAreaWithoutLegend = new PieBounds(0.05d, 0.10d, 0.95d, 0.90d);
        private const double LegendRowHeightNormalized = 0.035; // ~17.5px at 500px device height, ~35px at 1000px
        private const double LegendLeftPaddingNormalized = 0.01d; // ~5px at 500px
        private const double LegendRightPaddingNormalized = 0.01d; // ~5px at 500px
        private const double LegendTopPaddingNormalized = 0.005d; // ~2.5px at 500px
        private const double LegendBottomPaddingNormalized = 0.005d; // ~2.5px at 500px
        private const double LegendMinimumWidthNormalized = 0.15d; // Minimum width to accommodate labels
        private const double LegendHorizontalGapNormalized = 0.02d; // ~10px at 500px width - gap between pie and legend

        public PieGraphPresentationModel Build(PieGraphSnapshot snapshot, PieGraphPresentationOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            var effectiveOptions = options ?? new PieGraphPresentationOptions();
            var pieBounds = effectiveOptions.LegendVisible ? PieAreaWithLegend : PieAreaWithoutLegend;

            var center = new PiePoint(
                (pieBounds.Left + pieBounds.Right) / 2d,
                (pieBounds.Bottom + pieBounds.Top) / 2d);

            var radius = Math.Min(pieBounds.Width, pieBounds.Height) / 2d;

            var slices = BuildSlices(snapshot);
            var legend = effectiveOptions.LegendVisible ? BuildLegend(slices, effectiveOptions, pieBounds) : null;

            return new PieGraphPresentationModel(
                snapshot.Title,
                center,
                radius,
                slices,
                legend,
                effectiveOptions);
        }

        private static IReadOnlyList<PieSlicePresentationGeometry> BuildSlices(PieGraphSnapshot snapshot)
        {
            var source = snapshot.Slices;
            var result = new List<PieSlicePresentationGeometry>();

            if (source == null)
            {
                return result;
            }

            for (var i = 0; i < source.Count; i++)
            {
                var slice = source[i];
                if (slice == null)
                {
                    continue;
                }

                result.Add(
                    new PieSlicePresentationGeometry(
                        slice.Id,
                        slice.Label,
                        slice.Value,
                        slice.FormattedValue,
                        slice.Percentage,
                        slice.StartAngle,
                        slice.SweepAngle,
                        PiePalette.GetColorForIndex(i)));
            }

            return result;
        }

        private static PieLegendPresentationGeometry BuildLegend(IReadOnlyList<PieSlicePresentationGeometry> slices, PieGraphPresentationOptions options, PieBounds pieBounds)
        {
            const int MaxEntriesInShortLegend = 5;
            
            var entries = new List<PieLegendEntryPresentationGeometry>();

            if (slices != null && slices.Count > 0)
            {
                var displaySlices = slices;
                var hasMoreIndicator = false;

                // Apply short legend truncation if enabled
                if (options.UseShortLegend && slices.Count > MaxEntriesInShortLegend)
                {
                    displaySlices = slices.Take(MaxEntriesInShortLegend).ToList();
                    hasMoreIndicator = true;
                }

                var totalEntries = displaySlices.Count + (hasMoreIndicator ? 1 : 0);
                
                // Calculate legend bounds based on content and pie position
                var legendBounds = CalculateLegendBounds(displaySlices, hasMoreIndicator, pieBounds);
                
                var entryHeight = LegendRowHeightNormalized;
                var currentTop = legendBounds.Top;

                for (var i = 0; i < displaySlices.Count; i++)
                {
                    var slice = displaySlices[i];
                    var bottom = currentTop - entryHeight;
                    var top = currentTop;
                    var bounds = new PieBounds(legendBounds.Left, bottom, legendBounds.Right, top);

                    entries.Add(new PieLegendEntryPresentationGeometry(slice.Label, slice.Color, bounds));
                    currentTop -= entryHeight;
                }

                // Add "More..." indicator if needed
                if (hasMoreIndicator)
                {
                    var bottom = currentTop - entryHeight;
                    var top = currentTop;
                    var moreBounds = new PieBounds(legendBounds.Left, bottom, legendBounds.Right, top);
                    
                    // Use a transparent color for the "More..." indicator
                    var transparentColor = new PieColor(0, 0, 0, 0);
                    entries.Add(new PieLegendEntryPresentationGeometry("More...", transparentColor, moreBounds));
                }

                return new PieLegendPresentationGeometry(PieLegendPlacement.Right, legendBounds, entries);
            }

            return new PieLegendPresentationGeometry(PieLegendPlacement.Right, new PieBounds(0.74d, 0.10d, 0.98d, 0.90d), entries);
        }

        private static PieBounds CalculateLegendBounds(IReadOnlyList<PieSlicePresentationGeometry> displaySlices, bool hasMoreIndicator, PieBounds pieBounds)
        {
            // Estimate the longest label length (simple heuristic: character count)
            var maxLabelLength = 0;
            foreach (var slice in displaySlices)
            {
                if (!string.IsNullOrWhiteSpace(slice.Label))
                {
                    maxLabelLength = Math.Max(maxLabelLength, slice.Label.Length);
                }
            }

            // If "More..." is shown, account for it
            if (hasMoreIndicator)
            {
                maxLabelLength = Math.Max(maxLabelLength, "More...".Length);
            }

            // Estimate width: swatch (0.035 * 0.10 ≈ 0.0035) + text (character-based) + gaps + padding
            // Use a simple heuristic: 6 pixels per character at 500px width = 0.012 normalized
            var estimatedTextWidth = maxLabelLength * 0.012d;
            var legendWidth = Math.Max(
                LegendMinimumWidthNormalized,
                LegendLeftPaddingNormalized + 0.035d * 0.10d + 0.003d + estimatedTextWidth + LegendRightPaddingNormalized);

            // Calculate height: entries + padding
            var entryCount = displaySlices.Count + (hasMoreIndicator ? 1 : 0);
            var legendHeight = LegendTopPaddingNormalized + (entryCount * LegendRowHeightNormalized) + LegendBottomPaddingNormalized;

            // Position legend to the right of the pie, vertically centered
            // Legend left edge is positioned just after the pie right edge with a small gap
            var legendLeft = pieBounds.Right + LegendHorizontalGapNormalized;
            var legendRight = legendLeft + legendWidth;

            // Calculate pie center Y and center legend vertically at that position
            var pieCenterY = (pieBounds.Bottom + pieBounds.Top) / 2d;
            var legendTop = pieCenterY + (legendHeight / 2d);
            var legendBottom = pieCenterY - (legendHeight / 2d);

            return new PieBounds(legendLeft, legendBottom, legendRight, legendTop);
        }
    }
}
