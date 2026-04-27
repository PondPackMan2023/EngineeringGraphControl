using System.Collections.Generic;
using Graphing.Controls.Models;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    public sealed class SeriesPresentationGeometry
    {
        public SeriesPresentationGeometry(
            SeriesId seriesId,
            string label,
            ChartType chartType,
            SeriesConnectivityIntent connectivityIntent,
            IReadOnlyList<GeometryPoint3D> points)
        {
            SeriesId = seriesId;
            Label = label;
            ChartType = chartType;
            ConnectivityIntent = connectivityIntent;
            Points = points;
        }

        public SeriesId SeriesId { get; }
        public string Label { get; }
        public ChartType ChartType { get; }
        public SeriesConnectivityIntent ConnectivityIntent { get; }
        public IReadOnlyList<GeometryPoint3D> Points { get; }
    }
}