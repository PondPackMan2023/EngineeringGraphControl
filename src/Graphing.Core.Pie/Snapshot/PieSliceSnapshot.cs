namespace Graphing.Core.Pie.Snapshot
{
    using Graphing.Core.Pie.Presentation;

    public sealed class PieSliceSnapshot
    {
        public PieSliceSnapshot(
            PieSliceId id,
            string label,
            double value,
            string formattedValue,
            double percentage,
            double startAngle,
            double sweepAngle)
        {
            Id = id;
            Label = label;
            Value = value;
            FormattedValue = formattedValue;
            Percentage = percentage;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
        }

        public PieSliceId Id { get; }

        public string Label { get; }

        public double Value { get; }

        public string FormattedValue { get; }

        public double Percentage { get; }

        public double StartAngle { get; }

        public double SweepAngle { get; }
    }
}
