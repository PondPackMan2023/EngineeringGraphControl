using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Abstract presentation geometry for a graph subtitle.
    /// Expresses subtitle position and bounds in normalized abstract space [0,1] x [0,1].
    /// </summary>
    public sealed class SubtitlePresentationGeometry
    {
        public SubtitlePresentationGeometry(string text, GeometryPoint3D bottomLeft, GeometryPoint3D topRight)
        {
            Text = text;
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        /// <summary>
        /// Subtitle text to be rendered.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Bottom-left corner of the subtitle bounds in abstract space (Z = 0).
        /// </summary>
        public GeometryPoint3D BottomLeft { get; }

        /// <summary>
        /// Top-right corner of the subtitle bounds in abstract space (Z = 0).
        /// </summary>
        public GeometryPoint3D TopRight { get; }
    }
}
