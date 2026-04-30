using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.Editors.EditorModels;
using UnitRegistry;

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
            Axes = ConstructAxesEditorModel(graphModel, existingOptions);
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

            return new GraphPresentationOptions(
                hiddenSeriesIds: hiddenSeriesIds,
                hiddenAxisIds: hiddenAxisIds,
                graphTitle: Titles.HasTitleTextOverride ? Titles.TitleText : null,
                graphSubtitle: Titles.HasSubtitleTextOverride ? Titles.SubtitleText : null,
                legendPlacement: Legend.Position);
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
                    IsVisible = true,
                    HasLabelOverride = false,
                    Label = string.Empty,
                    HasColorOverride = false,
                    Color = Color.Black
                };
                model.Series.Add(item);
            }

            return model;
        }

        private static AxesEditorModel ConstructAxesEditorModel(
            IGraphModel graphModel,
            GraphPresentationOptions existingOptions)
        {
            var model = new AxesEditorModel();

            foreach (var axis in graphModel.Axes)
            {
                var isVisible = !existingOptions.HiddenAxisIds.Contains(axis.Id);
                var item = new AxisItemEditorModel(axis.Id)
                {
                    IsVisible = isVisible,
                    HasTitleOverride = false,
                    Title = string.Empty,
                    HasFixedRange = !axis.IsAutoRange,
                    Minimum = axis.MinimumValue ?? 0.0,
                    Maximum = axis.MaximumValue ?? 1.0,
                    HasFixedIncrement = false,
                    Increment = 1.0,
                    DisplayUnit = axis.Unit
                };
                model.Axes.Add(item);
            }

            return model;
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
