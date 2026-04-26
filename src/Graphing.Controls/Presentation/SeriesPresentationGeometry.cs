using System.Collections.Generic;
using Graphing.Controls.Models;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    public sealed class SeriesPresentationGeometry
    {
        public SeriesPresentationGeometry(
            object identifier,
            int seriesId,
            string label,
            ChartType chartType,
            SeriesConnectivityIntent connectivityIntent,
            IReadOnlyList<GeometryPoint3D> points)
        {
            Identifier = identifier;
            SeriesId = seriesId;
            Label = label;
            ChartType = chartType;
            ConnectivityIntent = connectivityIntent;
            Points = points;
        }

        public object Identifier { get; }
        public int SeriesId { get; }
        public string Label { get; }
        public ChartType ChartType { get; }
        public SeriesConnectivityIntent ConnectivityIntent { get; }
        public IReadOnlyList<GeometryPoint3D> Points { get; }
    }
}