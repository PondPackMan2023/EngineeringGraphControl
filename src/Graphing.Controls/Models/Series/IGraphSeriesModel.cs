namespace Graphing.Controls.Models
{
    public interface IGraphSeriesModel
    {
        object Identifier { get; }

        int Id { get; }

        string Label { get; }

        ChartType ChartType { get; }

        IGraphFieldDefinition XField { get; }

        IGraphFieldDefinition YField { get; }

        IAxisModel XAxis { get; }

        IAxisModel YAxis { get; }
    }
}