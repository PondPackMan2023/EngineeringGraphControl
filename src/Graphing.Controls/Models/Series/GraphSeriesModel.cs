using Graphing.Controls.Models.Series;

namespace Graphing.Controls.Models
{
    public class GraphSeriesModel : IGraphSeriesModel
    {
        public GraphSeriesModel(
                int id,
                string label,
                SeriesType seriesType,
                IGraphFieldDefinition xField,
                IGraphFieldDefinition yField,
                IAxisModel xAxis,
                IAxisModel yAxis)
        {
            SeriesId = new SeriesId($"{id}");
            Label = label;
            SeriesType = seriesType;
            XField = xField;
            YField = yField;
            XAxis = xAxis;
            YAxis = yAxis;
        }

        public SeriesId SeriesId { get; }
        public string Label { get; }
        public SeriesType SeriesType { get; }
        public IGraphFieldDefinition XField { get; }
        public IGraphFieldDefinition YField { get; }
        public IAxisModel XAxis { get; }
        public IAxisModel YAxis { get; }
    }
}
