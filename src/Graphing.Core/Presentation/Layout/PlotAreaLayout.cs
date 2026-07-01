using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Abstract plot area bounds expressed as two corner points in normalized abstract space.
    /// Z is always 0 for 2D presentations.
    /// </summary>
    public sealed class PlotAreaLayout
    {
        public PlotAreaLayout(GeometryPoint3D bottomLeft, GeometryPoint3D topRight)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        /// <summary>
        /// Bottom-left corner of the plot area in abstract space (Z = 0).
        /// </summary>
        public GeometryPoint3D BottomLeft { get; }

        /// <summary>
        /// Top-right corner of the plot area in abstract space (Z = 0).
        /// </summary>
        public GeometryPoint3D TopRight { get; }
    }
}
