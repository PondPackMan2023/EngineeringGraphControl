using System.Collections.Generic;
using UnitRegistry;
using UnitRegistry.Formatting;

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

        public IGraphModel ChangeAxisUnit(AxisId axisId, Unit unit)
        {
            var unitChanges = new Dictionary<AxisId, Unit>();
            unitChanges[axisId] = unit;
            return ChangeAxisUnits(unitChanges);
        }

        public IGraphModel ChangeAxisFormat(AxisId axisId, NumericFormatter formatter)
        {
            var updatedAxes = new List<IAxisModel>(Axes.Count);

            for (var axisIndex = 0; axisIndex < Axes.Count; axisIndex++)
            {
                var axis = Axes[axisIndex];
                if (axis == null)
                {
                    updatedAxes.Add(null);
                    continue;
                }

                if (!axis.Id.Equals(axisId))
                {
                    updatedAxes.Add(axis);
                    continue;
                }

                updatedAxes.Add(axis.ChangeFormat(formatter));
            }

            return new GraphModel(updatedAxes, Series);
        }

        public IGraphModel ChangeAxisUnitAndFormat(AxisId axisId, Unit unit, NumericFormatter formatter)
        {
            var updatedAxes = new List<IAxisModel>(Axes.Count);

            for (var axisIndex = 0; axisIndex < Axes.Count; axisIndex++)
            {
                var axis = Axes[axisIndex];
                if (axis == null)
                {
                    updatedAxes.Add(null);
                    continue;
                }

                if (!axis.Id.Equals(axisId))
                {
                    updatedAxes.Add(axis);
                    continue;
                }

                var newUnitLabel = unit != null && unit.Id != null
                    ? unit.Id.Value
                    : null;

                updatedAxes.Add(new AxisModel(
                    axis.Id,
                    axis.Orientation,
                    axis.Side,
                    unit,
                    newUnitLabel,
                    formatter));
            }

            return new GraphModel(updatedAxes, Series);
        }

        public IGraphModel ChangeAxisUnits(IReadOnlyDictionary<AxisId, Unit> unitChanges)
        {
            var updatedAxes = new List<IAxisModel>(Axes.Count);

            for (var axisIndex = 0; axisIndex < Axes.Count; axisIndex++)
            {
                var axis = Axes[axisIndex];
                if (axis == null)
                {
                    updatedAxes.Add(null);
                    continue;
                }

                var replacementUnit = default(Unit);
                var hasReplacement = false;

                if (unitChanges != null)
                {
                    hasReplacement = unitChanges.TryGetValue(axis.Id, out replacementUnit);
                }

                if (!hasReplacement)
                {
                    updatedAxes.Add(axis);
                    continue;
                }

                updatedAxes.Add(axis.ChangeUnit(replacementUnit));
            }

            return new GraphModel(updatedAxes, Series);
        }
    }
}
