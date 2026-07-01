using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Fixed edge-padding band reserved by the presentation model.
    /// </summary>
    public sealed class EdgePaddingBandGeometry
    {
        public EdgePaddingBandGeometry(
            AxisSide side,
            GeometryPoint3D bottomLeft,
            GeometryPoint3D topRight)
        {
            Side = side;
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        public AxisSide Side { get; }

        public GeometryPoint3D BottomLeft { get; }

        public GeometryPoint3D TopRight { get; }
    }
}