using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.Controls.Snapshot;
using Graphing.Editors.EditorModels;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Editors.Presentation
{
    public class GraphOptionsPresentationModel
    {
        public TitlesEditorModel Titles { get; }
        public SeriesEditorModel Series { get; }
        public AxesEditorModel Axes { get; }
        public LegendEditorModel Legend { get; }

        public GraphOptionsPresentationModel(
            IGraphModel graphModel,
            GraphPresentationOptions existingOptions)
            : this(graphModel, existingOptions, null)
        {
        }

        public GraphOptionsPresentationModel(
            IGraphModel graphModel,
            GraphPresentationOptions existingOptions,
            IGraphSnapshot snapshot)
        {
            if (graphModel == null)
            {
                throw new ArgumentNullException(nameof(graphModel));
            }

            if (existingOptions == null)
            {
                throw new ArgumentNullException(nameof(existingOptions));
            }

            Titles = ConstructTitlesEditorModel(existingOptions);
            Series = ConstructSeriesEditorModel(graphModel, existingOptions);
            Axes = ConstructAxesEditorModel(graphModel, existingOptions, snapshot);
            Legend = ConstructLegendEditorModel(existingOptions);
        }

        public GraphPresentationOptions BuildGraphPresentationOptions()
        {
            var hiddenSeriesIds = Series.Series
                .Where(s => !s.IsVisible)
                .Select(s => s.SeriesId)
                .ToList();

            var hiddenAxisIds = Axes.Axes
                .Where(a => !a.IsVisible)
                .Select(a => a.AxisId)
                .ToList();

            // Capture series overrides (label, color)
            var seriesOverrides = new Dictionary<SeriesId, SeriesOverrides>();
            foreach (var series in Series.Series)
            {
                if (series.HasLabelOverride || series.HasColorOverride)
                {
                    seriesOverrides[series.SeriesId] = new SeriesOverrides
                    {
                        HasLabelOverride = series.HasLabelOverride,
                        Label = series.Label,
                        HasColorOverride = series.HasColorOverride,
                        Color = series.Color
                    };
                }
            }

            // Capture axis overrides (title, range, increment)
            var axisOverrides = new Dictionary<AxisId, AxisOverrides>();
            foreach (var axis in Axes.Axes)
            {
                if (axis.HasTitleOverride || axis.HasFixedRange || axis.HasFixedIncrement)
                {
                    axisOverrides[axis.AxisId] = new AxisOverrides
                    {
                        HasTitleOverride = axis.HasTitleOverride,
                        Title = axis.Title,
                        HasFixedRange = axis.HasFixedRange,
                        Minimum = axis.Minimum,
                        Maximum = axis.Maximum,
                        HasFixedIncrement = axis.HasFixedIncrement,
                        Increment = axis.Increment
                    };
                }
            }

            return new GraphPresentationOptions(
                hiddenSeriesIds: hiddenSeriesIds,
                hiddenAxisIds: hiddenAxisIds,
                graphTitle: Titles.HasTitleTextOverride ? Titles.TitleText : null,
                graphSubtitle: Titles.HasSubtitleTextOverride ? Titles.SubtitleText : null,
                legendPlacement: Legend.Position,
                seriesOverrides: seriesOverrides,
                axisOverrides: axisOverrides);
        }

        private static TitlesEditorModel ConstructTitlesEditorModel(GraphPresentationOptions existingOptions)
        {
            var model = new TitlesEditorModel
            {
                HasTitleTextOverride = !string.IsNullOrEmpty(existingOptions.GraphTitle),
                TitleText = existingOptions.GraphTitle ?? string.Empty,
                HasSubtitleTextOverride = !string.IsNullOrEmpty(existingOptions.GraphSubtitle),
                SubtitleText = existingOptions.GraphSubtitle ?? string.Empty,
                HasTitleVisibilityOverride = false,
                IsTitleVisible = true,
                HasSubtitleVisibilityOverride = false,
                IsSubtitleVisible = true
            };
            return model;
        }

        private static SeriesEditorModel ConstructSeriesEditorModel(
            IGraphModel graphModel,
            GraphPresentationOptions existingOptions)
        {
            var model = new SeriesEditorModel();

            foreach (var series in graphModel.Series)
            {
                var item = new SeriesItemEditorModel(series.SeriesId)
                {
                    IsVisible = existingOptions.HiddenSeriesIds.Contains(series.SeriesId) ? false : true,
                    HasLabelOverride = false,
                    Label = series.Label,
                    HasColorOverride = false,
                    Color = Color.Black
                };

                // Load series overrides if they exist
                if (existingOptions.SeriesOverrides.TryGetValue(series.SeriesId, out var overrides))
                {
                    item.HasLabelOverride = overrides.HasLabelOverride;
                    item.Label = overrides.Label ?? series.Label;
                    item.HasColorOverride = overrides.HasColorOverride;
                    item.Color = overrides.Color;
                }

                model.Series.Add(item);
            }

            return model;
        }

        private static AxesEditorModel ConstructAxesEditorModel(
            IGraphModel graphModel,
            GraphPresentationOptions existingOptions,
            IGraphSnapshot snapshot)
        {
            var model = new AxesEditorModel();

            var snapshotAxisLookup = BuildSnapshotAxisLookup(snapshot);

            foreach (var axis in graphModel.Axes)
            {
                var isVisible = !existingOptions.HiddenAxisIds.Contains(axis.Id);
                
                var defaultTitle = string.Empty;
                var minimum = axis.MinimumValue ?? 0.0;
                var maximum = axis.MaximumValue ?? 1.0;
                var increment = 1.0;
                if (snapshotAxisLookup.TryGetValue(axis.Id.Value, out var snapshotAxis))
                {
                    defaultTitle = snapshotAxis.Title ?? string.Empty;
                    minimum = snapshotAxis.MinimumValue ?? 0.0;
                    maximum = snapshotAxis.MaximumValue ?? 1.0;
                    increment = snapshotAxis.Increment ?? 1.0;
                }

                var item = new AxisItemEditorModel(axis.Id, axis.Side)
                {
                    IsVisible = isVisible,
                    HasTitleOverride = false,
                    Title = defaultTitle,
                    HasFixedRange = !axis.IsAutoRange,
                    Minimum = minimum,
                    Maximum = maximum,
                    HasFixedIncrement = false,
                    Increment = increment,
                    DisplayUnit = axis.Unit,
                    NumericFormatter = axis.NumericFormatter ?? CreateFallbackNumericFormatter(axis)
                };

                // Load axis overrides if they exist
                if (existingOptions.AxisOverrides.TryGetValue(axis.Id, out var overrides))
                {
                    item.HasTitleOverride = overrides.HasTitleOverride;
                    item.Title = overrides.Title ?? defaultTitle;
                    item.HasFixedRange = overrides.HasFixedRange;
                    item.Minimum = overrides.Minimum;
                    item.Maximum = overrides.Maximum;
                    item.HasFixedIncrement = overrides.HasFixedIncrement;
                    item.Increment = overrides.Increment;
                }

                model.Axes.Add(item);
            }

            return model;
        }

        private static Dictionary<string, IAxisSnapshot> BuildSnapshotAxisLookup(IGraphSnapshot snapshot)
        {
            var lookup = new Dictionary<string, IAxisSnapshot>(StringComparer.Ordinal);
            if (snapshot == null || snapshot.Axes == null)
            {
                return lookup;
            }

            for (var axisIndex = 0; axisIndex < snapshot.Axes.Count; axisIndex++)
            {
                var axis = snapshot.Axes[axisIndex];
                if (axis != null && !string.IsNullOrEmpty(axis.AxisId))
                {
                    lookup[axis.AxisId] = axis;
                }
            }

            return lookup;
        }

        private static NumericFormatter CreateFallbackNumericFormatter(IAxisModel axis)
        {
            var formatterLabel = string.IsNullOrWhiteSpace(axis.UnitLabel)
                ? axis.Side.ToString()
                : axis.UnitLabel;

            return new NumericFormatter(
                "axis-editor-fallback-" + axis.Id.Value,
                UnitsRegistry.Default,
                formatterLabel,
                "R",
                CultureInfo.CurrentCulture);
        }

        private static LegendEditorModel ConstructLegendEditorModel(GraphPresentationOptions existingOptions)
        {
            var model = new LegendEditorModel
            {
                Position = existingOptions.LegendPlacement
            };
            return model;
        }
    }
}
