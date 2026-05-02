using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Immutable, presentation-safe descriptor for an axis interaction hit.
    /// Contains only projection metadata and no domain model references.
    /// </summary>
    public sealed class AxisInteractionDescriptor
    {
        public AxisInteractionDescriptor(
            string axisId,
            AxisOrientation orientation,
            AxisSide side,
            int sideIndex,
            double normalizedPositionAlongAxis,
               IValueFormatter formatter,
            Unit displayUnit)
        {
            AxisId = axisId;
            Orientation = orientation;
            Side = side;
            SideIndex = sideIndex;
            NormalizedPositionAlongAxis = normalizedPositionAlongAxis;
                Formatter = formatter;
            DisplayUnit = displayUnit;
        }

        public string AxisId { get; }

        public AxisOrientation Orientation { get; }

        public AxisSide Side { get; }

        public int SideIndex { get; }

        public double NormalizedPositionAlongAxis { get; }

        public IValueFormatter Formatter { get; }

        public Unit DisplayUnit { get; }
    }
}
