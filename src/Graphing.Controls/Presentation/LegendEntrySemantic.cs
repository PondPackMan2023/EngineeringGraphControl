namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Semantic legend entry mapped from a series.
    /// Contains identity and display text only; no layout or styling.
    /// </summary>
    public sealed class LegendEntrySemantic
    {
        public LegendEntrySemantic(object seriesIdentifier, int seriesId, string displayText)
        {
            SeriesIdentifier = seriesIdentifier;
            SeriesId = seriesId;
            DisplayText = displayText;
        }

        public object SeriesIdentifier { get; }
        public int SeriesId { get; }
        public string DisplayText { get; }
    }
}
