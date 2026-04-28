using System.Collections.Generic;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Collection of grid lines for a graph, organized by orientation.
    /// Grid lines are derived from axis ticks and are clipped to plot area bounds.
    /// </summary>
    public sealed class GridLinesGeometry
    {
        public GridLinesGeometry(
            IReadOnlyList<GridLineGeometry> verticalLines,
            IReadOnlyList<GridLineGeometry> horizontalLines)
        {
            VerticalLines = verticalLines ?? new List<GridLineGeometry>();
            HorizontalLines = horizontalLines ?? new List<GridLineGeometry>();
        }

        /// <summary>
        /// Vertical grid lines (aligned with X-axis ticks).
        /// </summary>
        public IReadOnlyList<GridLineGeometry> VerticalLines { get; }

        /// <summary>
        /// Horizontal grid lines (aligned with Y-axis ticks).
        /// </summary>
        public IReadOnlyList<GridLineGeometry> HorizontalLines { get; }
    }
}
