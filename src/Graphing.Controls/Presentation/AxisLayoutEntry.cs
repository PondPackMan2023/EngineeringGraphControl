namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Associates an axis with its resolved abstract layout position:
    /// which side of the plot area it occupies and its zero-based order among axes on that side.
    /// </summary>
    public sealed class AxisLayoutEntry
    {
        public AxisLayoutEntry(
            AxisPresentationGeometry axis,
            AxisSide side,
            int sideIndex,
            double normalizedSpanStart,
            double normalizedSpanEnd)
        {
            Axis = axis;
            Side = side;
            SideIndex = sideIndex;
            NormalizedSpanStart = normalizedSpanStart;
            NormalizedSpanEnd = normalizedSpanEnd;
        }

        /// <summary>
        /// The axis presentation geometry resolved in Phase P2.
        /// </summary>
        public AxisPresentationGeometry Axis { get; }

        /// <summary>
        /// The semantic side of the plot area this axis occupies.
        /// </summary>
        public AxisSide Side { get; }

        /// <summary>
        /// Zero-based position index among all axes on the same side, in deterministic order.
        /// </summary>
        public int SideIndex { get; }

        /// <summary>
        /// Start of the axis span in normalized plot coordinates [0,1].
        /// For stacked left-side axes this represents the lower Y bound of the axis region.
        /// </summary>
        public double NormalizedSpanStart { get; }

        /// <summary>
        /// End of the axis span in normalized plot coordinates [0,1].
        /// For stacked left-side axes this represents the upper Y bound of the axis region.
        /// </summary>
        public double NormalizedSpanEnd { get; }
    }
}
