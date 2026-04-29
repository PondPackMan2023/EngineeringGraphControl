using System;
using System.Collections.Generic;
using System.Globalization;
using Graphing.Controls.Models;

namespace Graphing.Controls.Snapshot
{
    internal sealed class GraphSnapshotBuilder
    {
        public GraphSnapshotBuilder()
        {

        }

        public IGraphSnapshot Build(IGraphModel graphModel)
        {
            var axisLookup = BuildAxisLookup(graphModel);

            var seriesSnapshots = new List<SeriesSnapshot>();
            var graphSeriesModels = graphModel != null ? graphModel.Series : null;

            if (graphSeriesModels != null)
            {
                for (var seriesIndex = 0; seriesIndex < graphSeriesModels.Count; seriesIndex++)
                {
                    var graphSeriesModel = graphSeriesModels[seriesIndex];

                    var xAxisId = graphSeriesModel != null && graphSeriesModel.XAxis != null
                        ? graphSeriesModel.XAxis.Id.Value
                        : string.Empty;
                    var yAxisId = graphSeriesModel != null && graphSeriesModel.YAxis != null
                        ? graphSeriesModel.YAxis.Id.Value
                        : string.Empty;

                    IAxisModel resolvedXAxis = null;
                    IAxisModel resolvedYAxis = null;
                    axisLookup.TryGetValue(xAxisId, out resolvedXAxis);
                    axisLookup.TryGetValue(yAxisId, out resolvedYAxis);

                    var xFieldSnapshot = BuildFieldSnapshot(graphSeriesModel != null ? graphSeriesModel.XField : null, resolvedXAxis);
                    var yFieldSnapshot = BuildFieldSnapshot(graphSeriesModel != null ? graphSeriesModel.YField : null, resolvedYAxis);

                    seriesSnapshots.Add(
                        new SeriesSnapshot(
                            graphSeriesModel.SeriesId,
                            graphSeriesModel.Label,
                            graphSeriesModel.SeriesType,
                            xAxisId,
                            yAxisId,
                            xFieldSnapshot,
                            yFieldSnapshot));
                }
            }

            var axisSnapshots = BuildAxisSnapshots(graphModel, seriesSnapshots);
            return new GraphSnapshot(seriesSnapshots, axisSnapshots);
        }

        private static Dictionary<string, IAxisModel> BuildAxisLookup(IGraphModel graphModel)
        {
            var lookup = new Dictionary<string, IAxisModel>(StringComparer.Ordinal);
            var axes = graphModel != null ? graphModel.Axes : null;
            if (axes != null)
            {
                for (var axisIndex = 0; axisIndex < axes.Count; axisIndex++)
                {
                    var axis = axes[axisIndex];
                    if (axis != null && axis.Id != null)
                    {
                        var axisId = axis.Id.Value;
                        if (lookup.ContainsKey(axisId))
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Graph model contains duplicate AxisId '{0}'. Axis ids must be unique.",
                                    axisId));
                        }

                        lookup[axisId] = axis;
                    }
                }
            }
            return lookup;
        }

        private FieldSnapshot BuildFieldSnapshot(IGraphFieldDefinition fieldDefinition, IAxisModel axis)
        {
            if (fieldDefinition == null)
            {
                return null;
            }

            var axisFormatter = axis != null ? axis.NumericFormatter : null;
            var formatter = axisFormatter;

            var rawValues = fieldDefinition.GetValues();
            var storageUnit = fieldDefinition.Unit;
            var displayUnit = axis != null ? axis.Unit : null;

            var valuesForGraph = rawValues;
            if (rawValues != null && storageUnit != null && displayUnit != null && !storageUnit.Equals(displayUnit))
            {
                var converted = new double[rawValues.Length];
                for (var i = 0; i < rawValues.Length; i++)
                {
                    var raw = rawValues.GetValue(i);
                    var rawDouble = Convert.ToDouble(raw, CultureInfo.InvariantCulture);
                    converted[i] = displayUnit.FromBaseValue(storageUnit.ToBaseValue(rawDouble));
                }
                valuesForGraph = converted;
            }

            return new FieldSnapshot(
                fieldDefinition.Label,
                fieldDefinition.Name,
                valuesForGraph,
                displayUnit ?? storageUnit,
                axis != null ? axis.UnitLabel : null,
                axisFormatter != null ? axisFormatter.Id.ToString() : null,
                formatter);
        }

        private List<AxisSnapshot> BuildAxisSnapshots(IGraphModel graphModel, IReadOnlyList<SeriesSnapshot> seriesSnapshots)
        {
            var axisSnapshots = new List<AxisSnapshot>();
            var axes = graphModel != null ? graphModel.Axes : null;

            if (axes == null)
            {
                return axisSnapshots;
            }

            for (var axisIndex = 0; axisIndex < axes.Count; axisIndex++)
            {
                var axis = axes[axisIndex];
                if (axis == null)
                {
                    continue;
                }

                var contributingFields = new List<IFieldSnapshot>();
                double? minimumValue = null;
                double? maximumValue = null;

                for (var seriesIndex = 0; seriesIndex < seriesSnapshots.Count; seriesIndex++)
                {
                    var seriesSnapshot = seriesSnapshots[seriesIndex];

                    if (seriesSnapshot.XAxisId == axis.Id.Value)
                    {
                        if (seriesSnapshot.XField != null)
                        {
                            contributingFields.Add(seriesSnapshot.XField);
                            UpdateBounds(ref minimumValue, ref maximumValue, seriesSnapshot.XField.Values);
                        }
                    }

                    if (seriesSnapshot.YAxisId == axis.Id.Value)
                    {
                        if (seriesSnapshot.YField != null)
                        {
                            contributingFields.Add(seriesSnapshot.YField);
                            UpdateBounds(ref minimumValue, ref maximumValue, seriesSnapshot.YField.Values);
                        }
                    }
                }

                if (!axis.IsAutoRange)
                {
                    if (axis.MinimumValue.HasValue)
                    {
                        minimumValue = axis.MinimumValue.Value;
                    }

                    if (axis.MaximumValue.HasValue)
                    {
                        maximumValue = axis.MaximumValue.Value;
                    }
                }

                var sourceFormatter = axis.NumericFormatter;

                axisSnapshots.Add(
                    new AxisSnapshot(
                        axis.Id.Value,
                        axis.Orientation,
                        axis.Side,
                        sourceFormatter != null ? sourceFormatter.Id.ToString() : null,
                        axis.Unit,
                        axis.UnitLabel,
                        axis.ScaleType,
                        axis.IsAutoRange,
                        contributingFields,
                        minimumValue,
                        maximumValue));
            }

            return axisSnapshots;
        }

        private static void UpdateBounds(ref double? minimumValue, ref double? maximumValue, Array values)
        {
            if (values == null)
            {
                return;
            }

            foreach (var value in values)
            {
                if (value == null)
                {
                    continue;
                }

                var numericValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);

                if (!minimumValue.HasValue || numericValue < minimumValue.Value)
                {
                    minimumValue = numericValue;
                }

                if (!maximumValue.HasValue || numericValue > maximumValue.Value)
                {
                    maximumValue = numericValue;
                }
            }
        }
    }
}
