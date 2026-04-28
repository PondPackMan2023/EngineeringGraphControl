using System.Collections.Generic;
using System.Collections.ObjectModel;
using Graphing.Controls.Models;
using Graphing.Controls.Snapshot;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Semantic presentation options consumed while building presentation geometry.
    /// </summary>
    public sealed class GraphPresentationOptions
    {
        private readonly HashSet<SeriesId> _hiddenSeriesIds;
        private readonly HashSet<AxisId> _hiddenAxisIds;
        private readonly IReadOnlyList<AnnotationSemantic> _annotations;

        public GraphPresentationOptions(
            IEnumerable<SeriesId> hiddenSeriesIds = null,
            IEnumerable<AxisId> hiddenAxisIds = null,
            string graphTitle = null,
            string graphSubtitle = null,
            IEnumerable<AnnotationSemantic> annotations = null,
            bool showGraphBorder = true)
        {
            _hiddenSeriesIds = hiddenSeriesIds != null
                ? [.. hiddenSeriesIds]
                : new HashSet<SeriesId>();

            _hiddenAxisIds = hiddenAxisIds != null
                ? [.. hiddenAxisIds]
                : new HashSet<AxisId>();

            GraphTitle = graphTitle;
            GraphSubtitle = graphSubtitle;
            HiddenAxisIds = new ReadOnlyCollection<AxisId>([.. _hiddenAxisIds]);
            _annotations = new ReadOnlyCollection<AnnotationSemantic>(
                [.. annotations ?? []]);
            ShowGraphBorder = showGraphBorder;
        }

        public string GraphTitle { get; }

        public string GraphSubtitle { get; }

        public bool ShowGraphBorder { get; }

        public IReadOnlyList<AnnotationSemantic> Annotations
        {
            get { return _annotations; }
        }

        public IReadOnlyCollection<AxisId> HiddenAxisIds { get; }

        public bool IsSeriesVisible(ISeriesSnapshot series)
        {
            if (series == null)
            {
                return false;
            }

            if (series.SeriesId != null && _hiddenSeriesIds.Contains(series.SeriesId))
            {
                return false;
            }

            return true;
        }

        public bool IsAxisVisible(IAxisSnapshot axis)
        {
            if (axis == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(axis.AxisId) && _hiddenAxisIds.Contains(new AxisId(axis.AxisId)))
            {
                return false;
            }

            return true;
        }
    }
}
