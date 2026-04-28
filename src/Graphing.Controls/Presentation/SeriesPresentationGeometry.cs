using System.Collections.Generic;
using System.Drawing;
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
            Color seriesColor = default)
        {
            SeriesId = seriesId;
            Label = label;
            SeriesType = seriesType;
            ConnectivityIntent = connectivityIntent;
            Points = points;
            SeriesColor = seriesColor == default ? Color.SteelBlue : seriesColor;
        }

        public SeriesId SeriesId { get; }
        public string Label { get; }
        public SeriesType SeriesType { get; }
        public SeriesConnectivityIntent ConnectivityIntent { get; }
        public IReadOnlyList<GeometryPoint3D> Points { get; }

        public Color SeriesColor { get; }
    }
}