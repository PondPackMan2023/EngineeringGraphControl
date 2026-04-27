using System.Collections.Generic;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Phase P3 output: layout-resolved presentation model.
    /// Describes where the plot area and axes are positioned in abstract space,
    /// completely device- and renderer-neutral.
    /// </summary>
    public sealed class GraphLayoutModel
    {
        public GraphLayoutModel(
            PlotAreaLayout plotArea,
            IReadOnlyList<AxisLayoutEntry> axes,
            IReadOnlyList<SeriesPresentationGeometry> series)
        {
            PlotArea = plotArea;
            Axes = axes;
            Series = series;
        }

        public GraphLayoutModel(
            PlotAreaLayout plotArea,
            IReadOnlyList<AxisLayoutEntry> axes,
            IReadOnlyList<SeriesPresentationGeometry> series,
            TitlePresentationGeometry title = null,
            SubtitlePresentationGeometry subtitle = null)
        {
            PlotArea = plotArea;
            Axes = axes;
            Series = series;
            Title = title;
            Subtitle = subtitle;
        }

        /// <summary>
        /// Abstract bounds of the plot area, excluding space allocated to axes.
        /// Expressed in a normalized unit square [0,1] x [0,1].
        /// </summary>
        public PlotAreaLayout PlotArea { get; }

        /// <summary>
        /// Axes included in the layout, each associated with a semantic side and ordered position.
        /// Left-side axes are capped at 6; any additional left-side axes are excluded.
        /// </summary>
        public IReadOnlyList<AxisLayoutEntry> Axes { get; }

        /// <summary>
        /// Series geometry forwarded from Phase P1.
        /// </summary>
        public IReadOnlyList<SeriesPresentationGeometry> Series { get; }

        /// <summary>
        /// Title presentation geometry (if title is present).
        /// </summary>
        public TitlePresentationGeometry Title { get; }

        /// <summary>
        /// Subtitle presentation geometry (if subtitle is present).
        /// </summary>
        public SubtitlePresentationGeometry Subtitle { get; }
    }
}
