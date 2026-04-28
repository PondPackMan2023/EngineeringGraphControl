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
            GeometryPoint3D contentBottomLeft,
            GeometryPoint3D contentTopRight,
            IReadOnlyList<LegendEntryPresentationGeometry> entries,
            bool showBorder = true)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;
            ContentBottomLeft = contentBottomLeft;
            ContentTopRight = contentTopRight;
            Entries = entries;
            ShowBorder = showBorder;
        }

        public GeometryPoint3D BottomLeft { get; }

        public GeometryPoint3D TopRight { get; }

        public GeometryPoint3D ContentBottomLeft { get; }

        public GeometryPoint3D ContentTopRight { get; }

        public IReadOnlyList<LegendEntryPresentationGeometry> Entries { get; }

        public bool ShowBorder { get; }
    }
}