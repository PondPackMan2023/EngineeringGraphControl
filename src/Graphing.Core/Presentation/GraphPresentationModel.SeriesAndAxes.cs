using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Rendering.Geometry;
using Graphing.Controls.Snapshot;
using UnitRegistry.Formatting;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

namespace Graphing.Controls.Presentation
{
    public sealed partial class GraphPresentationModel
    {
        private static IReadOnlyList<SeriesGeometryContext> BuildSeriesGeometry(
            IGraphSnapshot snapshot,
            GraphPresentationOptions options)
        {
            var result = new List<SeriesGeometryContext>();
            var seriesSnapshots = snapshot != null ? snapshot.Series : null;

            if (seriesSnapshots == null)
            {
                return new ReadOnlyCollection<SeriesGeometryContext>(result);
            }

            for (var seriesIndex = 0; seriesIndex < seriesSnapshots.Count; seriesIndex++)
            {
                var seriesSnapshot = seriesSnapshots[seriesIndex];
                if (seriesSnapshot == null)
                {
                    continue;
                }

                if (!options.IsSeriesVisible(seriesSnapshot))
                {
                    continue;
                }

                var points = BuildPoints(seriesSnapshot.XField, seriesSnapshot.YField);
                var resolvedSeriesColor = ResolveSeriesColor(seriesSnapshot, result.Count, options);
                AddSeriesGeometry(result, seriesSnapshot, points, resolvedSeriesColor);
            }

            return new ReadOnlyCollection<SeriesGeometryContext>(result);
        }

        private static void AddSeriesGeometry(
            List<SeriesGeometryContext> result,
            ISeriesSnapshot seriesSnapshot,
            IReadOnlyList<GeometryPoint3D> points,
            GraphColor resolvedSeriesColor)
        {
            if (seriesSnapshot.SeriesType == SeriesType.Line)
            {
                switch (seriesSnapshot.LineRenderMode)
                {
                    case LineRenderMode.PointsOnly:
                        result.Add(CreateSeriesGeometryContext(seriesSnapshot, points, SeriesConnectivityIntent.Discrete, resolvedSeriesColor));
                        return;

                    case LineRenderMode.LineAndPoints:
                        result.Add(CreateSeriesGeometryContext(seriesSnapshot, points, SeriesConnectivityIntent.Continuous, resolvedSeriesColor));
                        result.Add(CreateSeriesGeometryContext(seriesSnapshot, points, SeriesConnectivityIntent.Discrete, resolvedSeriesColor));
                        return;

                    case LineRenderMode.LineOnly:
                    default:
                        result.Add(CreateSeriesGeometryContext(seriesSnapshot, points, SeriesConnectivityIntent.Continuous, resolvedSeriesColor));
                        return;
                }
            }

            result.Add(
                CreateSeriesGeometryContext(
                    seriesSnapshot,
                    points,
                    ResolveConnectivity(seriesSnapshot.SeriesType),
                    resolvedSeriesColor));
        }

        private static SeriesGeometryContext CreateSeriesGeometryContext(
            ISeriesSnapshot seriesSnapshot,
            IReadOnlyList<GeometryPoint3D> points,
            SeriesConnectivityIntent connectivityIntent,
            GraphColor resolvedSeriesColor)
        {
            var geometry = new SeriesPresentationGeometry(
                seriesSnapshot.SeriesId,
                seriesSnapshot.Label,
                seriesSnapshot.SeriesType,
                connectivityIntent,
                points,
                resolvedSeriesColor);

            return new SeriesGeometryContext(seriesSnapshot, geometry);
        }

        private static IReadOnlyList<SeriesPresentationGeometry> BuildSeriesList(IReadOnlyList<SeriesGeometryContext> contexts)
        {
            var result = new List<SeriesPresentationGeometry>(contexts.Count);

            for (var index = 0; index < contexts.Count; index++)
            {
                result.Add(contexts[index].Geometry);
            }

            return new ReadOnlyCollection<SeriesPresentationGeometry>(result);
        }

        private static IReadOnlyList<AxisPresentationGeometry> BuildAxisGeometry(
            IGraphSnapshot snapshot,
            IReadOnlyList<SeriesGeometryContext> seriesContexts,
            GraphPresentationOptions options)
        {
            var result = new List<AxisPresentationGeometry>();
            var axisSnapshots = snapshot != null ? snapshot.Axes : null;

            if (axisSnapshots == null)
            {
                return new ReadOnlyCollection<AxisPresentationGeometry>(result);
            }

            for (var axisIndex = 0; axisIndex < axisSnapshots.Count; axisIndex++)
            {
                var axisSnapshot = axisSnapshots[axisIndex];
                if (axisSnapshot == null)
                {
                    continue;
                }

                var identity = BuildAxisIdentity(axisSnapshot);
                if (!options.IsAxisVisible(axisSnapshot))
                {
                    continue;
                }

                var orientation = ResolveAxisOrientation(axisSnapshot.Orientation);
                var side = ResolveAxisSide(axisSnapshot.Side);
                var title = axisSnapshot.Title;
                var formatter = ResolveAxisFormatter(axisSnapshot);
                var linePoints = BuildAxisLine(axisSnapshot.MinimumValue, axisSnapshot.MaximumValue, orientation);
                var ticks = BuildAxisTicks(
                    axisSnapshot.MinimumValue,
                    axisSnapshot.MaximumValue,
                    axisSnapshot.Increment,
                    formatter,
                    axisSnapshot.Unit,
                    side,
                    orientation,
                    axisSnapshot.MajorTickStride);

                result.Add(
                    new AxisPresentationGeometry(
                        identity,
                        axisSnapshot.AxisId,
                        side,
                        orientation,
                        title,
                        axisSnapshot.FormatterName,
                        formatter,
                        axisSnapshot.Unit,
                        axisSnapshot.DisplayUnitLabel,
                        axisSnapshot.MinimumValue,
                        axisSnapshot.MaximumValue,
                        axisSnapshot.MajorTickStride,
                        DefaultAxisLineThickness,
                        linePoints,
                        ticks));
            }

            return new ReadOnlyCollection<AxisPresentationGeometry>(result);
        }

        private static IReadOnlyList<GeometryPoint3D> BuildPoints(IFieldSnapshot xField, IFieldSnapshot yField)
        {
            if (xField == null || yField == null)
            {
                return new ReadOnlyCollection<GeometryPoint3D>(new List<GeometryPoint3D>());
            }

            var xValues = xField.Values;
            var yValues = yField.Values;

            if (xValues == null || yValues == null)
            {
                return new ReadOnlyCollection<GeometryPoint3D>(new List<GeometryPoint3D>());
            }

            var pointCount = Math.Min(xValues.Length, yValues.Length);
            var points = new List<GeometryPoint3D>(pointCount);

            for (var index = 0; index < pointCount; index++)
            {
                var xVal = TryToDouble(xValues.GetValue(index));
                var yVal = TryToDouble(yValues.GetValue(index));

                if (double.IsNaN(xVal) || double.IsNaN(yVal))
                {
                    continue;
                }

                points.Add(new GeometryPoint3D(xVal, yVal, 0d));
            }

            return new ReadOnlyCollection<GeometryPoint3D>(points);
        }

        private static AxisOrientation ResolveAxisOrientation(ModelAxisOrientation orientation)
        {
            return orientation == ModelAxisOrientation.X
                ? AxisOrientation.Horizontal
                : AxisOrientation.Vertical;
        }

        private static AxisSide ResolveAxisSide(ModelAxisSide side)
        {
            switch (side)
            {
                case ModelAxisSide.Left:
                    return AxisSide.Left;

                case ModelAxisSide.Right:
                    return AxisSide.Right;

                case ModelAxisSide.Bottom:
                    return AxisSide.Bottom;

                case ModelAxisSide.Top:
                    return AxisSide.Top;

                default:
                    return AxisSide.Other;
            }
        }

        private static string BuildAxisIdentity(IAxisSnapshot axisSnapshot)
        {
            if (!string.IsNullOrWhiteSpace(axisSnapshot.AxisId))
            {
                return axisSnapshot.AxisId;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}",
                axisSnapshot.FormatterName ?? string.Empty,
                axisSnapshot.DisplayUnitLabel ?? string.Empty);
        }

        private static IValueFormatter ResolveAxisFormatter(IAxisSnapshot axisSnapshot)
        {
            var fields = axisSnapshot.Fields;
            if (fields == null)
            {
                return null;
            }

            for (var index = 0; index < fields.Count; index++)
            {
                if (fields[index] != null && fields[index].Formatter != null)
                {
                    return fields[index].Formatter;
                }
            }

            return null;
        }

        private static IReadOnlyList<GeometryPoint3D> BuildAxisLine(
            double? minimumValue,
            double? maximumValue,
            AxisOrientation orientation)
        {
            var points = new List<GeometryPoint3D>();

            if (!minimumValue.HasValue || !maximumValue.HasValue)
            {
                return new ReadOnlyCollection<GeometryPoint3D>(points);
            }

            if (orientation == AxisOrientation.Horizontal)
            {
                points.Add(new GeometryPoint3D(minimumValue.Value, 0d, 0d));
                points.Add(new GeometryPoint3D(maximumValue.Value, 0d, 0d));
            }
            else
            {
                points.Add(new GeometryPoint3D(0d, minimumValue.Value, 0d));
                points.Add(new GeometryPoint3D(0d, maximumValue.Value, 0d));
            }

            return new ReadOnlyCollection<GeometryPoint3D>(points);
        }

        private static IReadOnlyList<AxisTickPresentation> BuildAxisTicks(
            double? minimumValue,
            double? maximumValue,
            double? increment,
               IValueFormatter formatter,
            UnitRegistry.Unit unit,
            AxisSide side,
            AxisOrientation orientation,
            int majorTickStride)
        {
            var ticks = new List<AxisTickPresentation>();

            if (!minimumValue.HasValue || !maximumValue.HasValue || !increment.HasValue || increment.Value <= 0d)
            {
                return new ReadOnlyCollection<AxisTickPresentation>(ticks);
            }

            _ = unit;

            var stride = majorTickStride > 0 ? majorTickStride : 1;
            var tickIncrement = increment.Value;
            if (orientation == AxisOrientation.Vertical)
            {
                tickIncrement = increment.Value * stride;
            }

            if (tickIncrement <= 0d)
            {
                return new ReadOnlyCollection<AxisTickPresentation>(ticks);
            }

            var tickValues = BuildTickValues(minimumValue.Value, maximumValue.Value, tickIncrement);

            for (var index = 0; index < tickValues.Count; index++)
            {
                var value = tickValues[index];
                var start = BuildTickStart(value, orientation);
                var end = BuildTickEnd(value, side, orientation);
                ticks.Add(new AxisTickPresentation(value, FormatAxisLabel(formatter, value), start, end));
            }

            return new ReadOnlyCollection<AxisTickPresentation>(ticks);
        }

        private static IReadOnlyList<double> BuildTickValues(double minimumValue, double maximumValue, double increment)
        {
            if (minimumValue == maximumValue)
            {
                return new ReadOnlyCollection<double>(new List<double> { minimumValue });
            }

            var ticks = new List<double>();
            var current = minimumValue;
            var maxStepCount = 1000;
            var stepCount = 0;

            while (current <= maximumValue + (increment * 0.5d) && stepCount < maxStepCount)
            {
                var value = current;
                if (value > maximumValue && value - maximumValue <= increment * 0.5d)
                {
                    value = maximumValue;
                }

                ticks.Add(value);
                current += increment;
                stepCount++;
            }

            if (ticks.Count == 0)
            {
                ticks.Add(minimumValue);
                if (maximumValue != minimumValue)
                {
                    ticks.Add(maximumValue);
                }
            }
            else
            {
                var last = ticks[ticks.Count - 1];
                if (Math.Abs(last - maximumValue) > increment * 0.5d)
                {
                    ticks.Add(maximumValue);
                }
                else
                {
                    ticks[ticks.Count - 1] = maximumValue;
                }
            }

            return new ReadOnlyCollection<double>(ticks);
        }

        private static GeometryPoint3D BuildTickStart(double value, AxisOrientation orientation)
        {
            if (orientation == AxisOrientation.Horizontal)
            {
                return new GeometryPoint3D(value, 0d, 0d);
            }

            return new GeometryPoint3D(0d, value, 0d);
        }

        private static GeometryPoint3D BuildTickEnd(double value, AxisSide side, AxisOrientation orientation)
        {
            if (orientation == AxisOrientation.Horizontal)
            {
                var yExtent = side == AxisSide.Top
                    ? -AxisTickMarkVerticalExtentEstimate
                    : AxisTickMarkVerticalExtentEstimate;
                return new GeometryPoint3D(value, yExtent, 0d);
            }

            var xExtent = side == AxisSide.Right
                ? AxisTickMarkExtentEstimate
                : -AxisTickMarkExtentEstimate;
            return new GeometryPoint3D(xExtent, value, 0d);
        }

        private static string FormatAxisLabel(IValueFormatter formatter, double value)
        {
            if (formatter != null)
            {
            return formatter.Format(value);
            }

            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static double TryToDouble(object value)
        {
            if (value == null)
            {
                return double.NaN;
            }

            try
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return double.NaN;
            }
        }

        private static SeriesConnectivityIntent ResolveConnectivity(SeriesType seriesType)
        {
            switch (seriesType)
            {
                case SeriesType.Line:
                case SeriesType.Profile:
                case SeriesType.Contour:
                    return SeriesConnectivityIntent.Continuous;

                case SeriesType.Bar:
                    return SeriesConnectivityIntent.Step;

                case SeriesType.Scatter:
                case SeriesType.Shape:
                    return SeriesConnectivityIntent.Discrete;

                case SeriesType.Auto:
                default:
                    return SeriesConnectivityIntent.Unspecified;
            }
        }

        private static void BindSeriesAxisEntries(
            IReadOnlyList<SeriesGeometryContext> seriesContexts,
            IReadOnlyList<AxisLayoutEntry> layoutAxes)
        {
            var axisLookup = new Dictionary<string, AxisLayoutEntry>(StringComparer.Ordinal);

            for (var i = 0; i < layoutAxes.Count; i++)
            {
                var entry = layoutAxes[i];
                var axisId = entry.Axis.AxisId;
                if (!string.IsNullOrEmpty(axisId) && !axisLookup.ContainsKey(axisId))
                {
                    axisLookup[axisId] = entry;
                }
            }

            for (var i = 0; i < seriesContexts.Count; i++)
            {
                var context = seriesContexts[i];
                var xAxisId = context.Source.XAxisId;
                var yAxisId = context.Source.YAxisId;

                if (!string.IsNullOrEmpty(xAxisId))
                {
                    axisLookup.TryGetValue(xAxisId, out var xEntry);
                    context.Geometry.XAxisEntry = xEntry;
                }

                if (!string.IsNullOrEmpty(yAxisId))
                {
                    axisLookup.TryGetValue(yAxisId, out var yEntry);
                    context.Geometry.YAxisEntry = yEntry;
                }
            }
        }
    }
}
