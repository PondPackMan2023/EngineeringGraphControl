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
            var seriesSnapshots = new List<SeriesSnapshot>();
            var graphSeriesModels = graphModel != null ? graphModel.Series : null;

            if (graphSeriesModels != null)
            {
                for (var seriesIndex = 0; seriesIndex < graphSeriesModels.Count; seriesIndex++)
                {
                    var graphSeriesModel = graphSeriesModels[seriesIndex];
                    var xFieldSnapshot = BuildFieldSnapshot(graphSeriesModel != null ? graphSeriesModel.XField : null, graphSeriesModel != null ? graphSeriesModel.XAxis : null);
                    var yFieldSnapshot = BuildFieldSnapshot(graphSeriesModel != null ? graphSeriesModel.YField : null, graphSeriesModel != null ? graphSeriesModel.YAxis : null);

                    seriesSnapshots.Add(
                        new SeriesSnapshot(
                            graphSeriesModel.Identifier,
                            graphSeriesModel.Id,
                            graphSeriesModel.Label,
                            graphSeriesModel.ChartType,
                            graphSeriesModel.XAxis != null ? graphSeriesModel.XAxis.Id : string.Empty,
                            graphSeriesModel.YAxis != null ? graphSeriesModel.YAxis.Id : string.Empty,
                            xFieldSnapshot,
                            yFieldSnapshot));
                }
            }

            var axisSnapshots = BuildAxisSnapshots(graphModel, seriesSnapshots);
            return new GraphSnapshot(seriesSnapshots, axisSnapshots);
        }

        private FieldSnapshot BuildFieldSnapshot(IGraphFieldDefinition fieldDefinition, IAxisModel axis)
        {
            if (fieldDefinition == null)
            {
                return null;
            }

            var axisFormatter = axis != null ? axis.NumericFormatter : null;
            var formatter = axisFormatter;

            return new FieldSnapshot(
                fieldDefinition.Label,
                fieldDefinition.Name,
                fieldDefinition.GetValues(),
                fieldDefinition.Unit,
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

                    if (string.Equals(seriesSnapshot.XAxisId, axis.Id, StringComparison.Ordinal))
                    {
                        if (seriesSnapshot.XField != null)
                        {
                            contributingFields.Add(seriesSnapshot.XField);
                            UpdateBounds(ref minimumValue, ref maximumValue, seriesSnapshot.XField.Values);
                        }
                    }

                    if (string.Equals(seriesSnapshot.YAxisId, axis.Id, StringComparison.Ordinal))
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
                        axis.Id,
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
