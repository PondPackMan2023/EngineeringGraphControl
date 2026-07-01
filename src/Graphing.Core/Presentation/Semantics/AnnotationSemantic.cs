namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Semantic annotation payload.
    /// Anchor is semantic-only and intentionally geometry-free.
    /// </summary>
    public sealed class AnnotationSemantic
    {
        public AnnotationSemantic(string text, string anchor)
        {
            Text = text;
            Anchor = anchor;
        }

        public string Text { get; }
        public string Anchor { get; }
    }
}
