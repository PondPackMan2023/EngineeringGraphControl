namespace Graphing.Core.Pie.Presentation
{
    public sealed class PieLegendEntryPresentationGeometry
    {
        public PieLegendEntryPresentationGeometry(string label, PieColor color, PieBounds bounds)
        {
            Label = label;
            Color = color;
            Bounds = bounds;
        }

        public string Label { get; }

        public PieColor Color { get; }

        public PieBounds Bounds { get; }
    }
}
