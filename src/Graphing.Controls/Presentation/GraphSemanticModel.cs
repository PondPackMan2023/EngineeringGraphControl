using System.Collections.Generic;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Phase P4 semantic-only presentation output.
    /// Contains legend mappings, annotations, titles, and axis descriptors.
    /// </summary>
    public sealed class GraphSemanticModel
    {
        public GraphSemanticModel(
            string graphTitle,
            string graphSubtitle,
            IReadOnlyList<LegendEntrySemantic> legendEntries,
            IReadOnlyList<AnnotationSemantic> annotations,
            IReadOnlyList<AxisDescriptorSemantic> axisDescriptors)
        {
            GraphTitle = graphTitle;
            GraphSubtitle = graphSubtitle;
            LegendEntries = legendEntries;
            Annotations = annotations;
            AxisDescriptors = axisDescriptors;
        }

        public string GraphTitle { get; }
        public string GraphSubtitle { get; }
        public IReadOnlyList<LegendEntrySemantic> LegendEntries { get; }
        public IReadOnlyList<AnnotationSemantic> Annotations { get; }
        public IReadOnlyList<AxisDescriptorSemantic> AxisDescriptors { get; }
    }
}
