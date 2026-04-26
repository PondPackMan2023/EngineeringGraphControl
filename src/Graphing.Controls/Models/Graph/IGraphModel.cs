using System.Collections.Generic;

namespace Graphing.Controls.Models
{
    public interface IGraphModel
    {
        IReadOnlyList<IAxisModel> Axes { get; }

        IReadOnlyList<IGraphSeriesModel> Series { get; }
    }
}