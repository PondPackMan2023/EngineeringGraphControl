namespace Graphing.Core.Pie.Presentation
{
    /// <summary>
    /// Immutable context for a pie slice interaction (e.g., double-click).
    /// Provides all relevant data about the interacted slice.
    /// </summary>
    public sealed class PieSliceInteractionContext
    {
        /// <summary>
        /// Initializes a new instance of the PieSliceInteractionContext.
        /// </summary>
        public PieSliceInteractionContext(
            PieSliceId sliceId,
            string label,
            double value,
            string formattedValue,
            double percentage)
        {
            SliceId = sliceId;
            Label = label;
            Value = value;
            FormattedValue = formattedValue;
            Percentage = percentage;
        }

        /// <summary>
        /// Gets the unique identifier for the interacted slice.
        /// </summary>
        public PieSliceId SliceId { get; }

        /// <summary>
        /// Gets the label for the slice.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Gets the numeric value represented by the slice.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Gets the formatted value for the slice.
        /// </summary>
        public string FormattedValue { get; }

        /// <summary>
        /// Gets the percentage of the total represented by the slice.
        /// </summary>
        public double Percentage { get; }
    }
}
