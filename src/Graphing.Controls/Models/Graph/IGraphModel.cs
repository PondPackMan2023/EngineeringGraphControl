using System.Collections.Generic;
using UnitRegistry;

namespace Graphing.Controls.Models
{
    public interface IGraphModel
    {
        IReadOnlyList<IAxisModel> Axes { get; }

        IReadOnlyList<IGraphSeriesModel> Series { get; }

        IGraphModel ChangeAxisUnit(AxisId axisId, Unit unit);

        IGraphModel ChangeAxisUnits(IReadOnlyDictionary<AxisId, Unit> unitChanges);
    }
}