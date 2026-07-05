namespace Graphing.Core.Pie.Models
{
    public class PieSliceModel : IPieSliceModel
    {
        public PieSliceModel(string id, string label, double value)
        {
            Id = new Presentation.PieSliceId(id);
            Label = label;
            Value = value;
        }

        public Presentation.PieSliceId Id { get; }

        public string Label { get; }

        public double Value { get; }
    }
}
