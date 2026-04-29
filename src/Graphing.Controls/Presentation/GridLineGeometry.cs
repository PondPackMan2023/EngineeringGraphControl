using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Represents a single grid line in abstract domain space.
    /// A grid line is a line segment aligned with an axis tick.
    /// Each grid line explicitly declares the axis space it is normalized against,
    /// enabling renderers to apply axis-relative transformations.
    /// </summary>
    public sealed class GridLineGeometry
    {
        public GridLineGeometry(AxisOrientation orientation, GeometryPoint3D start, GeometryPoint3D end, AxisLayoutEntry axisEntry = null)
        {
            Orientation = orientation;
            Start = start;
            End = end;
            AxisEntry = axisEntry;
        }

        /// <summary>
        /// Orientation of this grid line (Horizontal or Vertical).
        /// </summary>
        public AxisOrientation Orientation { get; }

        /// <summary>
        /// Start point of the grid line segment in abstract domain space (Z = 0).
        /// </summary>
        public GeometryPoint3D Start { get; }

        /// <summary>
        /// End point of the grid line segment in abstract domain space (Z = 0).
        /// </summary>
        public GeometryPoint3D End { get; }

        /// <summary>
        /// The axis layout entry that defines how this grid line's domain coordinates
        /// map to normalized plot-relative coordinates. Vertical lines bind to the X-axis entry;
        /// horizontal lines bind to their source Y-axis entry.
        /// </summary>
        public AxisLayoutEntry AxisEntry { get; set; }
    }
}
