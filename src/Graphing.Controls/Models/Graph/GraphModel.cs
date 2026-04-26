using System.Collections.Generic;

namespace Graphing.Controls.Models
{
    public class GraphModel : IGraphModel
    {
        public GraphModel(IReadOnlyList<IAxisModel> axes, IReadOnlyList<IGraphSeriesModel> series)
        {
            Axes = axes;
            Series = series;
        }

        public IReadOnlyList<IAxisModel> Axes { get; }
        public IReadOnlyList<IGraphSeriesModel> Series { get; }
    }
}
