using Graphing.Controls.Models.Series;

namespace Graphing.Controls.Models
{
    public interface IGraphSeriesModel
    {
        SeriesId SeriesId { get; }

        string Label { get; }

        SeriesType SeriesType { get; }

        LineRenderMode LineRenderMode { get; }

        IGraphFieldDefinition XField { get; }

        IGraphFieldDefinition YField { get; }

        IAxisModel XAxis { get; }

        IAxisModel YAxis { get; }
    }
}
