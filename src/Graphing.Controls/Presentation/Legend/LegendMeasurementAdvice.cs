namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Advisory legend layout measurement provided by the renderer.
    /// Values are normalized to abstract [0,1] model units.
    /// </summary>
    public sealed class LegendMeasurementAdvice
    {
        public LegendMeasurementAdvice(
            double requiredThickness,
            double itemWidth,
            double itemHeight,
            double availablePrimarySpan,
            int itemsPerPrimarySpan,
            int secondaryLineCount)
        {
            RequiredThickness = requiredThickness;
            ItemWidth = itemWidth;
            ItemHeight = itemHeight;
            AvailablePrimarySpan = availablePrimarySpan;
            ItemsPerPrimarySpan = itemsPerPrimarySpan;
            SecondaryLineCount = secondaryLineCount;
        }

        public double RequiredThickness { get; }

        public double ItemWidth { get; }

        public double ItemHeight { get; }

        public double AvailablePrimarySpan { get; }

        public int ItemsPerPrimarySpan { get; }

        public int SecondaryLineCount { get; }

        public int ItemsPerRow => ItemsPerPrimarySpan;

        public int RowCount => SecondaryLineCount;
    }
}
