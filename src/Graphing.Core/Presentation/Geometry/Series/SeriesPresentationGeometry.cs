using System.Collections.Generic;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    public sealed class SeriesPresentationGeometry
    {
        public SeriesPresentationGeometry(
            SeriesId seriesId,
            string label,
            SeriesType seriesType,
            SeriesConnectivityIntent connectivityIntent,
            IReadOnlyList<GeometryPoint3D> points,
            GraphColor seriesColor = default)
        {
            SeriesId = seriesId;
            Label = label;
            SeriesType = seriesType;
            ConnectivityIntent = connectivityIntent;
            Points = points;
            SeriesColor = seriesColor == default ? GraphColor.SteelBlue : seriesColor;
        }

        public SeriesId SeriesId { get; }
        public string Label { get; }
        public SeriesType SeriesType { get; }
        public SeriesConnectivityIntent ConnectivityIntent { get; }
        public IReadOnlyList<GeometryPoint3D> Points { get; }
        public GraphColor SeriesColor { get; }

        /// <summary>
        /// The resolved X-axis layout entry for this series, bound by the presentation model.
        /// Null if the series did not declare an X axis ID or the axis was not found in the layout.
        /// </summary>
        public AxisLayoutEntry XAxisEntry { get; internal set; }

        /// <summary>
        /// The resolved Y-axis layout entry for this series, bound by the presentation model.
        /// Null if the series did not declare a Y axis ID or the axis was not found in the layout.
        /// </summary>
        public AxisLayoutEntry YAxisEntry { get; internal set; }
    }
}