using Graphing.Controls.Rendering.Geometry;
using System.Collections.Generic;

namespace Graphing.Controls.Presentation
{
    public sealed class AxisPresentationGeometry
    {
        public AxisPresentationGeometry(
            string identity,
            string axisId,
            AxisSide side,
            AxisOrientation orientation,
            string title,
            string formatterName,
            string displayUnitLabel,
            double? minimumValue,
            double? maximumValue,
            IReadOnlyList<GeometryPoint3D> linePoints,
            IReadOnlyList<AxisTickPresentation> ticks)
        {
            Identity = identity;
            AxisId = axisId;
            Side = side;
            Orientation = orientation;
            Title = title;
            FormatterName = formatterName;
            DisplayUnitLabel = displayUnitLabel;
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            LinePoints = linePoints;
            Ticks = ticks;
        }

        public string Identity { get; }
        public string AxisId { get; }
        public AxisSide Side { get; }
        public AxisOrientation Orientation { get; }
        public string Title { get; }
        public string FormatterName { get; }
        public string DisplayUnitLabel { get; }
        public double? MinimumValue { get; }
        public double? MaximumValue { get; }
        public IReadOnlyList<GeometryPoint3D> LinePoints { get; }
        public IReadOnlyList<AxisTickPresentation> Ticks { get; }
    }
}