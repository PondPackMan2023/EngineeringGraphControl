namespace Graphing.Controls.Models
{
    public class GraphSeriesModel : IGraphSeriesModel
    {
        public GraphSeriesModel(
                int id,
                string label,
                ChartType chartType,
                IGraphFieldDefinition xField,
                IGraphFieldDefinition yField,
                IAxisModel xAxis,
                IAxisModel yAxis)
        {
            Identifier = "series-" + id;
            Id = id;
            Label = label;
            ChartType = chartType;
            XField = xField;
            YField = yField;
            XAxis = xAxis;
            YAxis = yAxis;
        }

        public object Identifier { get; }
        public int Id { get; }
        public string Label { get; }
        public ChartType ChartType { get; }
        public IGraphFieldDefinition XField { get; }
        public IGraphFieldDefinition YField { get; }
        public IAxisModel XAxis { get; }
        public IAxisModel YAxis { get; }
    }
}
