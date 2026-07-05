using System;
using System.Globalization;

namespace Graphing.Core.Pie.Presentation
{
    /// <summary>
    /// Generates formatted tooltip content for pie slices.
    /// </summary>
    public static class PieTooltipContentGenerator
    {
        /// <summary>
        /// Generates tooltip content for a slice.
        /// Format: {Label}\n{FormattedValue}\n{Percentage}%
        /// </summary>
        /// <param name="slice">The slice to generate content for.</param>
        /// <returns>Formatted tooltip string.</returns>
        public static string GenerateTooltip(PieSlicePresentationGeometry slice)
        {
            if (slice == null)
            {
                return string.Empty;
            }

            // Format percentage with one decimal place
            var percentString = (slice.Percentage * 100).ToString("F1", CultureInfo.InvariantCulture);

            return $"{slice.Label}\n{slice.FormattedValue}\n{percentString}%";
        }
    }
}
