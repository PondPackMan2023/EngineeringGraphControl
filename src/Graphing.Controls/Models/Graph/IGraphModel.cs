using System.Collections.Generic;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Controls.Models
{
    public interface IGraphModel
    {
        IReadOnlyList<IAxisModel> Axes { get; }

        IReadOnlyList<IGraphSeriesModel> Series { get; }

        IGraphModel ChangeAxisUnit(AxisId axisId, Unit unit);

        IGraphModel ChangeAxisFormat(AxisId axisId, IValueFormatter formatter);

        IGraphModel ChangeAxisUnitAndFormat(AxisId axisId, Unit unit, IValueFormatter formatter);

        IGraphModel ChangeAxisUnits(IReadOnlyDictionary<AxisId, Unit> unitChanges);
    }
}