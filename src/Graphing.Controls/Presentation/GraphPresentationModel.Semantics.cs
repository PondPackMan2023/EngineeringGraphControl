using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Graphing.Controls.Snapshot;

namespace Graphing.Controls.Presentation
{
    public sealed partial class GraphPresentationModel
    {
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
            var uniqueContexts = BuildUniqueSeriesContexts(seriesContexts);
            var entries = new List<LegendEntrySemantic>(uniqueContexts.Count);

            for (var index = 0; index < uniqueContexts.Count; index++)
            {
                var context = uniqueContexts[index];
                var item = context.Geometry;
                var text = item.Label ?? string.Empty;
                entries.Add(new LegendEntrySemantic(item.SeriesId, text));
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

        private static IReadOnlyList<SeriesGeometryContext> BuildUniqueSeriesContexts(IReadOnlyList<SeriesGeometryContext> seriesContexts)
        {
            var unique = new List<SeriesGeometryContext>();
            var seenSeriesIds = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < seriesContexts.Count; index++)
            {
                var context = seriesContexts[index];
                if (context == null || context.Geometry == null)
                {
                    continue;
                }

                var seriesId = context.Geometry.SeriesId != null
                    ? context.Geometry.SeriesId.ToString()
                    : null;

                if (seriesId != null)
                {
                    if (!seenSeriesIds.Add(seriesId))
                    {
                        continue;
                    }
                }

                unique.Add(context);
            }

            return new ReadOnlyCollection<SeriesGeometryContext>(unique);
        }

        private static IReadOnlyList<SeriesPresentationGeometry> BuildLegendSeriesList(IReadOnlyList<SeriesGeometryContext> seriesContexts)
        {
            var uniqueContexts = BuildUniqueSeriesContexts(seriesContexts);
            var legendSeries = new List<SeriesPresentationGeometry>(uniqueContexts.Count);

            for (var index = 0; index < uniqueContexts.Count; index++)
            {
                legendSeries.Add(uniqueContexts[index].Geometry);
            }

            return new ReadOnlyCollection<SeriesPresentationGeometry>(legendSeries);
        }
    }
}
