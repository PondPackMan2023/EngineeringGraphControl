namespace Graphing.Core.Pie.Models
{
    public interface IPieSliceModel
    {
        Graphing.Core.Pie.Presentation.PieSliceId Id { get; }

        string Label { get; }

        double Value { get; }
    }
}
