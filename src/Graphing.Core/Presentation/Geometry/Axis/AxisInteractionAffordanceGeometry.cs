using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Axis interaction affordance geometry in normalized abstract space.
    /// This is semantic interaction intent geometry and is distinct from visual hit geometry.
    /// </summary>
    public sealed class AxisInteractionAffordanceGeometry
    {
        public AxisInteractionAffordanceGeometry(
            string axisId,
            AxisSide side,
            AxisOrientation orientation,
            GeometryPoint3D bottomLeft,
            GeometryPoint3D topRight)
        {
            AxisId = axisId;
            Side = side;
            Orientation = orientation;
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        public string AxisId { get; }

        public AxisSide Side { get; }

        public AxisOrientation Orientation { get; }

        public GeometryPoint3D BottomLeft { get; }

        public GeometryPoint3D TopRight { get; }
    }
}
