using Graphing.Controls.Rendering.Geometry;
using System.Collections.Generic;
using UnitRegistry;
using UnitRegistry.Formatting;

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
               IValueFormatter formatter,
            Unit displayUnit,
            string displayUnitLabel,
            double? minimumValue,
            double? maximumValue,
            int majorTickStride,
            double axisLineThickness,
            IReadOnlyList<GeometryPoint3D> linePoints,
            IReadOnlyList<AxisTickPresentation> ticks)
        {
            Identity = identity;
            AxisId = axisId;
            Side = side;
            Orientation = orientation;
            Title = title;
            FormatterName = formatterName;
                Formatter = formatter;
            DisplayUnit = displayUnit;
            DisplayUnitLabel = displayUnitLabel;
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            MajorTickStride = majorTickStride > 0 ? majorTickStride : 1;
            AxisLineThickness = axisLineThickness;
            LinePoints = linePoints;
            Ticks = ticks;
        }

        public string Identity { get; }
        public string AxisId { get; }
        public AxisSide Side { get; }
        public AxisOrientation Orientation { get; }
        public string Title { get; }
        public string FormatterName { get; }
        public IValueFormatter Formatter { get; }
        public Unit DisplayUnit { get; }
        public string DisplayUnitLabel { get; }
        public double? MinimumValue { get; }
        public double? MaximumValue { get; }
        public int MajorTickStride { get; }
        public double AxisLineThickness { get; }
        public IReadOnlyList<GeometryPoint3D> LinePoints { get; }
        public IReadOnlyList<AxisTickPresentation> Ticks { get; }
    }
}
