using System;
using System.Collections.Generic;
using System.Globalization;
using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Controls.Snapshot
{
    internal sealed class GraphSnapshotBuilder : IGraphSnapshotBuilder
    {
        private const int DefaultDesiredTickCount = 5;
        private const double MaxTemporalIntervals = 9d;

        public GraphSnapshotBuilder()
        {

        }

        public IGraphSnapshot Build(IGraphModel graphModel, GraphPresentationOptions options = null)
        {
            var axisLookup = BuildAxisLookup(graphModel);

            var seriesSnapshots = new List<SeriesSnapshot>();
            var graphSeriesModels = ResolveOrderedSeries(graphModel, options);

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
                            graphSeriesModel.LineRenderMode,
                            xAxisId,
                            yAxisId,
                            xFieldSnapshot,
                            yFieldSnapshot));
                }
            }

            var axisSnapshots = BuildAxisSnapshots(graphModel, seriesSnapshots, options);
            return new GraphSnapshot(seriesSnapshots, axisSnapshots);
        }

        private static IReadOnlyList<IGraphSeriesModel> ResolveOrderedSeries(IGraphModel graphModel, GraphPresentationOptions options)
        {
            var graphSeriesModels = graphModel != null ? graphModel.Series : null;
            var requestedOrder = options != null ? options.SeriesOrder : null;
            return GraphPresentationOptions.ResolveSeriesOrder(graphSeriesModels, requestedOrder);
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

            var axisFormatter = axis != null ? axis.Formatter : null;
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

        private List<AxisSnapshot> BuildAxisSnapshots(
            IGraphModel graphModel,
            IReadOnlyList<SeriesSnapshot> seriesSnapshots,
            GraphPresentationOptions options)
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
                double? actualMinimumValue = null;
                double? actualMaximumValue = null;

                for (var seriesIndex = 0; seriesIndex < seriesSnapshots.Count; seriesIndex++)
                {
                    var seriesSnapshot = seriesSnapshots[seriesIndex];

                    if (seriesSnapshot.XAxisId == axis.Id.Value)
                    {
                        if (seriesSnapshot.XField != null)
                        {
                            contributingFields.Add(seriesSnapshot.XField);
                            UpdateBounds(ref minimumValue, ref maximumValue, seriesSnapshot.XField.Values);
                            UpdateBounds(ref actualMinimumValue, ref actualMaximumValue, seriesSnapshot.XField.Values);
                            UpdateMaximum(ref actualMaximumValue, seriesSnapshot.XField.Values);
                        }
                    }

                    if (seriesSnapshot.YAxisId == axis.Id.Value)
                    {
                        if (seriesSnapshot.YField != null)
                        {
                            contributingFields.Add(seriesSnapshot.YField);
                            UpdateBounds(ref minimumValue, ref maximumValue, seriesSnapshot.YField.Values);
                            UpdateBounds(ref actualMinimumValue, ref actualMaximumValue, seriesSnapshot.YField.Values);
                            UpdateMaximum(ref actualMaximumValue, seriesSnapshot.YField.Values);
                        }
                    }
                }

                var axisOverride = ResolveAxisOverrides(axis, options);

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

                if (axisOverride != null && axisOverride.HasFixedRange)
                {
                    minimumValue = axisOverride.Minimum;
                    maximumValue = axisOverride.Maximum;
                }

                var hasUserRangeOverride = axisOverride != null && axisOverride.HasFixedRange;
                var hasUserIncrementOverride = axisOverride != null && axisOverride.HasFixedIncrement && axisOverride.Increment > 0d;
                var enforceMinimumZero = axisOverride != null && axisOverride.EnforceMinimumZero;
                var enableDenseNumericYAxisTicks = options == null || options.EnableDenseNumericYAxisTicks;
                var denseNumericYAxisExcludedDimensions = options != null
                    ? options.DenseNumericYAxisExcludedDimensions
                    : GraphPresentationOptions.CreateDefaultDenseNumericYAxisExcludedDimensions();

                double? resolvedIncrement = null;
                var isAutoIncrement = true;
                var majorTickStride = 1;

                if (hasUserIncrementOverride)
                {
                    resolvedIncrement = axisOverride.Increment;
                    isAutoIncrement = false;
                }
                else if (minimumValue.HasValue && maximumValue.HasValue)
                {
                    var range = AxisRangeCalculator.Calculate(
                        minimumValue.Value,
                        maximumValue.Value,
                        DefaultDesiredTickCount);

                    resolvedIncrement = range.Increment;

                    if (!hasUserRangeOverride)
                    {
                        minimumValue = range.Minimum;
                        maximumValue = range.Maximum;
                    }

                    if (!hasUserRangeOverride
                        && axis.Orientation == Graphing.Controls.Models.AxisOrientation.X
                        && actualMaximumValue.HasValue
                        && maximumValue.HasValue
                        && maximumValue.Value > actualMaximumValue.Value)
                    {
                        maximumValue = actualMaximumValue.Value;
                    }

                    if (axis.Orientation == Graphing.Controls.Models.AxisOrientation.X
                        && isAutoIncrement
                        && minimumValue.HasValue
                        && maximumValue.HasValue
                        && IsTemporalAxis(axis))
                    {
                        resolvedIncrement = ApplyTemporalIncrementPolicy(
                            resolvedIncrement,
                            minimumValue.Value,
                            maximumValue.Value,
                            axis.Unit);
                    }

                    var yAxisPolicyResolution = AxisPolicyApplier.Apply(
                        axis,
                        contributingFields,
                        actualMinimumValue,
                        actualMaximumValue,
                        minimumValue,
                        maximumValue,
                        resolvedIncrement,
                        hasUserRangeOverride,
                        hasUserIncrementOverride,
                        enforceMinimumZero,
                        enableDenseNumericYAxisTicks,
                        denseNumericYAxisExcludedDimensions);

                    minimumValue = yAxisPolicyResolution.Minimum;
                    maximumValue = yAxisPolicyResolution.Maximum;
                    resolvedIncrement = yAxisPolicyResolution.Increment;
                    majorTickStride = yAxisPolicyResolution.MajorTickStride;
                }

                var sourceFormatter = axis.Formatter;
                var axisTitle = BuildAxisTitle(axis);

                axisSnapshots.Add(
                    new AxisSnapshot(
                        axis.Id.Value,
                        axis.Orientation,
                        axis.Side,
                        sourceFormatter != null ? sourceFormatter.Id.ToString() : null,
                        axis.LabelValueConverter,
                        axis.Unit,
                        axis.UnitLabel,
                        axis.ScaleType,
                        axis.IsAutoRange,
                        contributingFields,
                        minimumValue,
                        maximumValue,
                        resolvedIncrement,
                        isAutoIncrement,
                        majorTickStride,
                        axisTitle));
            }

            return axisSnapshots;
        }

        private static string BuildAxisTitle(IAxisModel axis)
        {
            if (axis == null)
            {
                return string.Empty;
            }

                var formatter = axis.Formatter;
                var numericFormatter = formatter as ILabel;
                var label = numericFormatter != null ? numericFormatter.Label : null;

            if (string.IsNullOrWhiteSpace(label))
            {
                return string.Empty;
            }

            var unit = axis.Unit;
            var unitLabel = unit != null ? unit.Label : null;

            if (!string.IsNullOrWhiteSpace(unitLabel))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", label, unitLabel);
            }

            return label;
        }

        private static AxisOverrides ResolveAxisOverrides(IAxisModel axis, GraphPresentationOptions options)
        {
            if (axis == null || axis.Id == null || options == null || options.AxisOverrides == null)
            {
                return null;
            }

            AxisOverrides axisOverride;
            if (options.AxisOverrides.TryGetValue(axis.Id, out axisOverride))
            {
                return axisOverride;
            }

            return null;
        }

        private static bool IsTemporalAxis(IAxisModel axis)
        {
            return axis != null
                && axis.Unit != null
                && axis.Unit.Dimension == UnitRegistry.Dimensions.Time;
        }

        private static double? ApplyTemporalIncrementPolicy(double? increment, double minimumValue, double maximumValue, UnitRegistry.Unit unit)
        {
            if (!increment.HasValue || increment.Value <= 0d || unit == null)
            {
                return increment;
            }

            if (unit == UnitRegistry.Units.Time.Hours || unit == UnitRegistry.Units.Time.Hour)
            {
                return ApplyHoursIncrementPolicy(increment.Value, minimumValue, maximumValue);
            }

            if (unit == UnitRegistry.Units.Time.Minutes
                || unit == UnitRegistry.Units.Time.Minute
                || unit == UnitRegistry.Units.Time.Seconds
                || unit == UnitRegistry.Units.Time.Second)
            {
                return ApplyMinutesSecondsIncrementPolicy(increment.Value, minimumValue, maximumValue);
            }

            return increment;
        }

        private static double ApplyHoursIncrementPolicy(double increment, double minimumValue, double maximumValue)
        {
            if (increment <= 4d)
            {
                return increment;
            }

            var adjusted = 6d;
            var span = maximumValue - minimumValue;

            while (span / adjusted > MaxTemporalIntervals)
            {
                adjusted *= 2d;
            }

            return adjusted;
        }

        private static double ApplyMinutesSecondsIncrementPolicy(double increment, double minimumValue, double maximumValue)
        {
            if (increment <= 3d)
            {
                return increment;
            }

            if (increment <= 4d)
            {
                return 3d;
            }

            if (increment <= 9d)
            {
                return 5d;
            }

            if (increment <= 14d)
            {
                return 10d;
            }

            var adjusted = 15d;
            var span = maximumValue - minimumValue;

            while (span / adjusted > MaxTemporalIntervals)
            {
                adjusted *= 2d;
            }

            return adjusted;
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

        private static void UpdateMaximum(ref double? maximumValue, Array values)
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

                if (!maximumValue.HasValue || numericValue > maximumValue.Value)
                {
                    maximumValue = numericValue;
                }
            }
        }
    }
}
