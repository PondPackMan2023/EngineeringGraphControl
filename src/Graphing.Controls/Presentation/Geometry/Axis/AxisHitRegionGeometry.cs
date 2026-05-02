using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    public sealed class AxisHitRegionGeometry
    {
        public AxisHitRegionGeometry(
            string axisId,
            AxisSide side,
            AxisOrientation orientation,
            double axisLineThickness,
            double halfHitThickness,
            GeometryPoint3D bottomLeft,
            GeometryPoint3D topRight)
        {
            AxisId = axisId;
            Side = side;
            Orientation = orientation;
            AxisLineThickness = axisLineThickness;
            HalfHitThickness = halfHitThickness;
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        public string AxisId { get; }

        public AxisSide Side { get; }

        public AxisOrientation Orientation { get; }

        public double AxisLineThickness { get; }

        public double HalfHitThickness { get; }

        public GeometryPoint3D BottomLeft { get; }

        public GeometryPoint3D TopRight { get; }
    }
}
