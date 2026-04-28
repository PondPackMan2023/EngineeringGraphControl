using System.Collections.Generic;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Immutable legend container geometry in normalized abstract space.
    /// </summary>
    public sealed class LegendPresentationGeometry
    {
        public LegendPresentationGeometry(
            GeometryPoint3D bottomLeft,
            GeometryPoint3D topRight,
            IReadOnlyList<LegendEntryPresentationGeometry> entries,
            bool showBorder = true)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;
            Entries = entries;
            ShowBorder = showBorder;
        }

        public GeometryPoint3D BottomLeft { get; }

        public GeometryPoint3D TopRight { get; }

        public IReadOnlyList<LegendEntryPresentationGeometry> Entries { get; }

        public bool ShowBorder { get; }
    }
}