namespace Graphing.Core.Pie.Presentation
{
    public sealed class PieGraphPresentationOptions
    {
        public PieGraphPresentationOptions(bool legendVisible = true, bool useShortLegend = false, bool showLegendBorder = false)
        {
            LegendVisible = legendVisible;
            UseShortLegend = useShortLegend;
            ShowLegendBorder = showLegendBorder;
        }

        public bool LegendVisible { get; }

        public bool UseShortLegend { get; }

        public bool ShowLegendBorder { get; }
    }
}
