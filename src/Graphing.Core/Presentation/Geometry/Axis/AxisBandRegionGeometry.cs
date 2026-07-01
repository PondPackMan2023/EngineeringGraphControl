using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Immutable rectangular region in normalized abstract space.
    /// </summary>
    public sealed class AxisBandRegionGeometry
    {
        public AxisBandRegionGeometry(GeometryPoint3D bottomLeft, GeometryPoint3D topRight)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        public GeometryPoint3D BottomLeft { get; }

        public GeometryPoint3D TopRight { get; }
    }
}
