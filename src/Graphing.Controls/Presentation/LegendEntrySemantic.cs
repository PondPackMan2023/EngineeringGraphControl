using Graphing.Controls.Models;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Semantic legend entry mapped from a series.
    /// Contains identity and display text only; no layout or styling.
    /// </summary>
    public sealed class LegendEntrySemantic
    {
        public LegendEntrySemantic(SeriesId seriesId, string displayText)
        {
            SeriesId = seriesId;
            DisplayText = displayText;
        }

        public SeriesId SeriesId { get; }
        public string DisplayText { get; }
    }
}
