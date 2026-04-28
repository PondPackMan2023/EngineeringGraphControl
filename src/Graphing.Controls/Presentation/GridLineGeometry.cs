using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Represents a single grid line in abstract normalized space.
    /// A grid line is a line segment aligned with an axis tick.
    /// </summary>
    public sealed class GridLineGeometry
    {
        public GridLineGeometry(AxisOrientation orientation, GeometryPoint3D start, GeometryPoint3D end)
        {
            Orientation = orientation;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Orientation of this grid line (Horizontal or Vertical).
        /// </summary>
        public AxisOrientation Orientation { get; }

        /// <summary>
        /// Start point of the grid line segment in normalized abstract space (Z = 0).
        /// </summary>
        public GeometryPoint3D Start { get; }

        /// <summary>
        /// End point of the grid line segment in normalized abstract space (Z = 0).
        /// </summary>
        public GeometryPoint3D End { get; }
    }
}
