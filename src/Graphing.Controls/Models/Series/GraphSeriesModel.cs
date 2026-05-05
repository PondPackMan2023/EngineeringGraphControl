using Graphing.Controls.Models.Series;

namespace Graphing.Controls.Models
{
    public class GraphSeriesModel : IGraphSeriesModel
    {
        public GraphSeriesModel(
                SeriesId seriesId,
                string label,
                SeriesType seriesType,
                IGraphFieldDefinition xField,
                IGraphFieldDefinition yField,
                IAxisModel xAxis,
                IAxisModel yAxis,
                LineRenderMode lineRenderMode = LineRenderMode.LineOnly)
        {
            SeriesId = seriesId;
            Label = label;
            SeriesType = seriesType;
            LineRenderMode = lineRenderMode;
            XField = xField;
            YField = yField;
            XAxis = xAxis;
            YAxis = yAxis;
        }

        public SeriesId SeriesId { get; }
        public string Label { get; }
        public SeriesType SeriesType { get; }
        public LineRenderMode LineRenderMode { get; }
        public IGraphFieldDefinition XField { get; }
        public IGraphFieldDefinition YField { get; }
        public IAxisModel XAxis { get; }
        public IAxisModel YAxis { get; }
    }
}
