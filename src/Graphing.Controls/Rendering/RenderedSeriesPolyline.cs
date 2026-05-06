using System.Collections.Generic;
using System.Drawing;
using Graphing.Controls.Models;
using Graphing.Controls.Models.Series;

namespace Graphing.Controls.Rendering
{
    internal sealed class RenderedSeriesPolyline
    {
        public RenderedSeriesPolyline(
            SeriesId seriesId,
            SeriesType seriesType,
            Color seriesColor,
            IReadOnlyList<PointF> devicePoints)
        {
            SeriesId = seriesId;
            SeriesType = seriesType;
            SeriesColor = seriesColor;
            DevicePoints = devicePoints;
        }

        public SeriesId SeriesId { get; }

        public SeriesType SeriesType { get; }

        public Color SeriesColor { get; }

        public IReadOnlyList<PointF> DevicePoints { get; }
    }
}
