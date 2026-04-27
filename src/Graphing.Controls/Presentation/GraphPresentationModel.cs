using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Graphing.Controls.Models;
using Graphing.Controls.Rendering.Geometry;
using Graphing.Controls.Snapshot;
using UnitRegistry.Formatting;
using ModelAxisOrientation = Graphing.Controls.Models.AxisOrientation;
using ModelAxisSide = Graphing.Controls.Models.AxisSide;

namespace Graphing.Controls.Presentation
{
    public sealed class GraphPresentationModel
    {
        private const int MaxLeftAxisCount = 6;
        private const double AxisSlotSize = 0.1;

        private readonly IReadOnlyList<SeriesPresentationGeometry> _series;
        private readonly IReadOnlyList<AxisPresentationGeometry> _axes;
        private readonly GraphLayoutModel _layout;
        private readonly GraphSemanticModel _semantics;

        public GraphPresentationModel(IGraphSnapshot snapshot, GraphPresentationOptions options = null)
        {
            options = options ?? new GraphPresentationOptions();
            var seriesContexts = BuildSeriesGeometry(snapshot, options);
            _series = BuildSeriesList(seriesContexts);
            _axes = BuildAxisGeometry(snapshot, seriesContexts, options);
            _layout = BuildLayoutGeometry(_axes, _series);
            _semantics = BuildSemanticModel(snapshot, seriesContexts, _axes, options);
        }

        public IReadOnlyList<SeriesPresentationGeometry> Series
        {
            get { return _series; }
        }

        public IReadOnlyList<AxisPresentationGeometry> Axes
        {
            get { return _axes; }
        }

        public GraphLayoutModel Layout
        {
            get { return _layout; }
        }

        public GraphSemanticModel Semantics
        {
            get { return _semantics; }
        }

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
                var geometry = new SeriesPresentationGeometry(
                    seriesSnapshot.Identifier,
                    seriesSnapshot.Id,
                    seriesSnapshot.Label,
                    seriesSnapshot.ChartType,
                    ResolveConnectivity(seriesSnapshot.ChartType),
                    points);

                result.Add(new SeriesGeometryContext(seriesSnapshot, geometry));
            }

            return new ReadOnlyCollection<SeriesGeometryContext>(result);
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
                if (!options.IsAxisVisible(axisSnapshot, identity))
                {
                    continue;
                }

                var orientation = ResolveAxisOrientation(axisSnapshot.Orientation);
                var side = ResolveAxisSide(axisSnapshot.Side);
                var title = axisSnapshot.Title;
                var formatter = ResolveAxisFormatter(axisSnapshot);
                var linePoints = BuildAxisLine(axisSnapshot.MinimumValue, axisSnapshot.MaximumValue, orientation);
                var ticks = BuildAxisTicks(axisSnapshot.MinimumValue, axisSnapshot.MaximumValue, orientation, formatter);

                result.Add(
                    new AxisPresentationGeometry(
                        identity,
                        axisSnapshot.AxisId,
                        side,
                        orientation,
                        title,
                        axisSnapshot.FormatterName,
                        axisSnapshot.DisplayUnitLabel,
                        axisSnapshot.MinimumValue,
                        axisSnapshot.MaximumValue,
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



        private static NumericFormatter ResolveAxisFormatter(IAxisSnapshot axisSnapshot)
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
            AxisOrientation orientation,
            NumericFormatter formatter)
        {
            var ticks = new List<AxisTickPresentation>();

            if (!minimumValue.HasValue || !maximumValue.HasValue)
            {
                return new ReadOnlyCollection<AxisTickPresentation>(ticks);
            }

            var tickValues = BuildTickValues(minimumValue.Value, maximumValue.Value);

            for (var index = 0; index < tickValues.Count; index++)
            {
                var value = tickValues[index];
                var anchor = orientation == AxisOrientation.Horizontal
                    ? new GeometryPoint3D(value, 0d, 0d)
                    : new GeometryPoint3D(0d, value, 0d);
                ticks.Add(new AxisTickPresentation(value, anchor, FormatAxisLabel(formatter, value)));
            }

            return new ReadOnlyCollection<AxisTickPresentation>(ticks);
        }

        private static IReadOnlyList<double> BuildTickValues(double minimumValue, double maximumValue)
        {
            if (minimumValue == maximumValue)
            {
                return new ReadOnlyCollection<double>(new List<double> { minimumValue });
            }

            const int TickCount = 5;
            var ticks = new List<double>(TickCount);
            var increment = (maximumValue - minimumValue) / (TickCount - 1);

            for (var index = 0; index < TickCount; index++)
            {
                ticks.Add(minimumValue + (increment * index));
            }

            return new ReadOnlyCollection<double>(ticks);
        }

        private static string FormatAxisLabel(NumericFormatter formatter, double value)
        {
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

        private static SeriesConnectivityIntent ResolveConnectivity(ChartType chartType)
        {
            switch (chartType)
            {
                case ChartType.Line:
                case ChartType.Profile:
                case ChartType.Contour:
                    return SeriesConnectivityIntent.Continuous;

                case ChartType.Bar:
                    return SeriesConnectivityIntent.Step;

                case ChartType.Scatter:
                case ChartType.Shape:
                    return SeriesConnectivityIntent.Discrete;

                case ChartType.Auto:
                default:
                    return SeriesConnectivityIntent.Unspecified;
            }
        }

        private static GraphLayoutModel BuildLayoutGeometry(
            IReadOnlyList<AxisPresentationGeometry> axes,
            IReadOnlyList<SeriesPresentationGeometry> series)
        {
            var entries = new List<AxisLayoutEntry>();
            var leftAxes = new List<AxisPresentationGeometry>();
            var rightAxes = new List<AxisPresentationGeometry>();
            var bottomAxes = new List<AxisPresentationGeometry>();
            var topAxes = new List<AxisPresentationGeometry>();
            var otherAxes = new List<AxisPresentationGeometry>();

            for (var index = 0; index < axes.Count; index++)
            {
                var axis = axes[index];

                switch (axis.Side)
                {
                    case AxisSide.Left:
                        leftAxes.Add(axis);
                        break;

                    case AxisSide.Right:
                        rightAxes.Add(axis);
                        break;

                    case AxisSide.Bottom:
                        bottomAxes.Add(axis);
                        break;

                    case AxisSide.Top:
                        topAxes.Add(axis);
                        break;

                    default:
                        otherAxes.Add(axis);
                        break;
                }
            }

            var leftCount = Math.Min(leftAxes.Count, MaxLeftAxisCount);

            for (var leftIndex = 0; leftIndex < leftCount; leftIndex++)
            {
                var axis = leftAxes[leftIndex];
                var spanHeight = 1.0 / leftCount;
                var normalizedSpanStart = (leftCount - leftIndex - 1) * spanHeight;
                var normalizedSpanEnd = (leftCount - leftIndex) * spanHeight;

                entries.Add(new AxisLayoutEntry(axis, AxisSide.Left, leftIndex, normalizedSpanStart, normalizedSpanEnd));
            }

            for (var rightIndex = 0; rightIndex < rightAxes.Count; rightIndex++)
            {
                entries.Add(new AxisLayoutEntry(rightAxes[rightIndex], AxisSide.Right, rightIndex, 0d, 1d));
            }

            for (var bottomIndex = 0; bottomIndex < bottomAxes.Count; bottomIndex++)
            {
                entries.Add(new AxisLayoutEntry(bottomAxes[bottomIndex], AxisSide.Bottom, bottomIndex, 0d, 1d));
            }

            for (var topIndex = 0; topIndex < topAxes.Count; topIndex++)
            {
                entries.Add(new AxisLayoutEntry(topAxes[topIndex], AxisSide.Top, topIndex, 0d, 1d));
            }

            for (var otherIndex = 0; otherIndex < otherAxes.Count; otherIndex++)
            {
                entries.Add(new AxisLayoutEntry(otherAxes[otherIndex], AxisSide.Other, 0, 0d, 1d));
            }

            var rightCount = rightAxes.Count;
            var bottomCount = bottomAxes.Count;
            var topCount = topAxes.Count;

            var plotArea = new PlotAreaLayout(
                new GeometryPoint3D(leftCount * AxisSlotSize, bottomCount * AxisSlotSize, 0d),
                new GeometryPoint3D(1.0 - (rightCount * AxisSlotSize), 1.0 - (topCount * AxisSlotSize), 0d));

            return new GraphLayoutModel(
                plotArea,
                new ReadOnlyCollection<AxisLayoutEntry>(entries),
                series);
        }

        private static GraphSemanticModel BuildSemanticModel(
            IGraphSnapshot snapshot,
            IReadOnlyList<SeriesGeometryContext> seriesContexts,
            IReadOnlyList<AxisPresentationGeometry> axes,
            GraphPresentationOptions options)
        {
            var legendEntries = BuildLegendEntries(seriesContexts);
            var axisDescriptors = BuildAxisDescriptors(axes);
            var annotations = BuildAnnotations(options, axisDescriptors);

            return new GraphSemanticModel(
                options.GraphTitle,
                options.GraphSubtitle,
                legendEntries,
                annotations,
                axisDescriptors);
        }

        private static IReadOnlyList<LegendEntrySemantic> BuildLegendEntries(IReadOnlyList<SeriesGeometryContext> seriesContexts)
        {
            var entries = new List<LegendEntrySemantic>(seriesContexts.Count);

            for (var index = 0; index < seriesContexts.Count; index++)
            {
                var context = seriesContexts[index];
                var item = context.Geometry;
                var text = item.Label ?? string.Empty;
                entries.Add(new LegendEntrySemantic(item.Identifier, item.SeriesId, text));
            }

            return new ReadOnlyCollection<LegendEntrySemantic>(entries);
        }

        private static IReadOnlyList<AxisDescriptorSemantic> BuildAxisDescriptors(IReadOnlyList<AxisPresentationGeometry> axes)
        {
            var descriptors = new List<AxisDescriptorSemantic>(axes.Count);

            for (var index = 0; index < axes.Count; index++)
            {
                var axis = axes[index];

                descriptors.Add(
                    new AxisDescriptorSemantic(
                        axis.Identity,
                        axis.AxisId,
                        BuildAxisCaption(axis),
                        axis.DisplayUnitLabel,
                        axis.FormatterName));
            }

            return new ReadOnlyCollection<AxisDescriptorSemantic>(descriptors);
        }

        private static string BuildAxisCaption(AxisPresentationGeometry axis)
        {
            if (!string.IsNullOrWhiteSpace(axis.Title))
            {
                return axis.Title;
            }

            if (!string.IsNullOrWhiteSpace(axis.DisplayUnitLabel))
            {
                return axis.DisplayUnitLabel;
            }

            if (!string.IsNullOrWhiteSpace(axis.FormatterName))
            {
                return axis.FormatterName;
            }

            return axis.AxisId ?? string.Empty;
        }

        private static IReadOnlyList<AnnotationSemantic> BuildAnnotations(
            GraphPresentationOptions options,
            IReadOnlyList<AxisDescriptorSemantic> axisDescriptors)
        {
            var annotations = new List<AnnotationSemantic>();

            if (!string.IsNullOrWhiteSpace(options.GraphTitle))
            {
                annotations.Add(new AnnotationSemantic(options.GraphTitle, "graph:title"));
            }

            if (!string.IsNullOrWhiteSpace(options.GraphSubtitle))
            {
                annotations.Add(new AnnotationSemantic(options.GraphSubtitle, "graph:subtitle"));
            }

            var providedAnnotations = options.Annotations;
            for (var index = 0; index < providedAnnotations.Count; index++)
            {
                var annotation = providedAnnotations[index];
                if (annotation == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(annotation.Text))
                {
                    continue;
                }

                var anchor = string.IsNullOrWhiteSpace(annotation.Anchor)
                    ? "graph:note"
                    : annotation.Anchor;
                annotations.Add(new AnnotationSemantic(annotation.Text, anchor));
            }

            for (var axisIndex = 0; axisIndex < axisDescriptors.Count; axisIndex++)
            {
                var axis = axisDescriptors[axisIndex];
                annotations.Add(new AnnotationSemantic(axis.Caption, "axis:" + axis.AxisIdentity));
            }

            return new ReadOnlyCollection<AnnotationSemantic>(annotations);
        }

        private sealed class SeriesGeometryContext
        {
            public SeriesGeometryContext(
                ISeriesSnapshot source,
                SeriesPresentationGeometry geometry)
            {
                Source = source;
                Geometry = geometry;
            }

            public ISeriesSnapshot Source { get; }
            public SeriesPresentationGeometry Geometry { get; }
        }
    }
}
