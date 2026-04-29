namespace Graphing.Controls.Presentation
{
    public sealed class AxisTickPresentation
    {
        public AxisTickPresentation(double value, string label)
        {
            Value = value;
            Label = label;
        }

        public double Value { get; }
        public string Label { get; }
    }
}