using Graphing.Controls.Models;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Immutable geometry for one legend entry.
    /// Includes series identity, display text, and placeholder glyph bounds.
    /// </summary>
    public sealed class LegendEntryPresentationGeometry
    {
        public LegendEntryPresentationGeometry(
            SeriesId seriesId,
            string displayText,
            GeometryPoint3D bottomLeft,
            GeometryPoint3D topRight,
            GeometryPoint3D glyphBottomLeft,
            GeometryPoint3D glyphTopRight,
            GraphColor seriesColor = default,
            LegendGlyphKind glyphKind = LegendGlyphKind.LineAndPoint)
        {
            SeriesId = seriesId;
            DisplayText = displayText;
            BottomLeft = bottomLeft;
            TopRight = topRight;
            GlyphBottomLeft = glyphBottomLeft;
            GlyphTopRight = glyphTopRight;
            SeriesColor = seriesColor == default ? GraphColor.SteelBlue : seriesColor;
            GlyphKind = glyphKind;
        }

        public SeriesId SeriesId { get; }

        public string DisplayText { get; }

        public GeometryPoint3D BottomLeft { get; }

        public GeometryPoint3D TopRight { get; }

        public GeometryPoint3D GlyphBottomLeft { get; }

        public GeometryPoint3D GlyphTopRight { get; }

        public GraphColor SeriesColor { get; }

        public LegendGlyphKind GlyphKind { get; }
    }
}