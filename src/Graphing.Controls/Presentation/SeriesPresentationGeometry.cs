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
            IReadOnlyList<GeometryPoint3D> points)
        {
            SeriesId = seriesId;
            Label = label;
            SeriesType = seriesType;
            ConnectivityIntent = connectivityIntent;
            Points = points;
        }

        public SeriesId SeriesId { get; }
        public string Label { get; }
        public SeriesType SeriesType { get; }
        public SeriesConnectivityIntent ConnectivityIntent { get; }
        public IReadOnlyList<GeometryPoint3D> Points { get; }
    }
}