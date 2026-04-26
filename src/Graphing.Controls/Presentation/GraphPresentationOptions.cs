using System.Collections.Generic;
using System.Collections.ObjectModel;
using Graphing.Controls.Snapshot;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Semantic presentation options consumed while building presentation geometry.
    /// </summary>
    public sealed class GraphPresentationOptions
    {
        private readonly HashSet<object> _hiddenSeriesIdentifiers;
        private readonly HashSet<int> _hiddenSeriesIds;
        private readonly HashSet<string> _hiddenAxisIdentities;
        private readonly HashSet<string> _hiddenAxisIds;
        private readonly IReadOnlyList<AnnotationSemantic> _annotations;

        public GraphPresentationOptions(
            IEnumerable<object> hiddenSeriesIdentifiers = null,
            IEnumerable<int> hiddenSeriesIds = null,
            IEnumerable<string> hiddenAxisIdentities = null,
            IEnumerable<string> hiddenAxisIds = null,
            string graphTitle = null,
            string graphSubtitle = null,
            IEnumerable<AnnotationSemantic> annotations = null)
        {
            _hiddenSeriesIdentifiers = hiddenSeriesIdentifiers != null
                ? new HashSet<object>(hiddenSeriesIdentifiers)
                : new HashSet<object>();

            _hiddenSeriesIds = hiddenSeriesIds != null
                ? new HashSet<int>(hiddenSeriesIds)
                : new HashSet<int>();

            _hiddenAxisIdentities = hiddenAxisIdentities != null
                ? new HashSet<string>(hiddenAxisIdentities)
                : new HashSet<string>();

            _hiddenAxisIds = hiddenAxisIds != null
                ? new HashSet<string>(hiddenAxisIds)
                : new HashSet<string>();

            GraphTitle = graphTitle;
            GraphSubtitle = graphSubtitle;
            _annotations = new ReadOnlyCollection<AnnotationSemantic>(
                new List<AnnotationSemantic>(annotations ?? new AnnotationSemantic[0]));
        }

        public string GraphTitle { get; }

        public string GraphSubtitle { get; }

        public IReadOnlyList<AnnotationSemantic> Annotations
        {
            get { return _annotations; }
        }

        public bool IsSeriesVisible(ISeriesSnapshot series)
        {
            if (series == null)
            {
                return false;
            }

            if (_hiddenSeriesIds.Contains(series.Id))
            {
                return false;
            }

            if (series.Identifier != null && _hiddenSeriesIdentifiers.Contains(series.Identifier))
            {
                return false;
            }

            return true;
        }

        public bool IsAxisVisible(IAxisSnapshot axis, string axisIdentity)
        {
            if (axis == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(axis.AxisId) && _hiddenAxisIds.Contains(axis.AxisId))
            {
                return false;
            }

            if (axisIdentity != null && _hiddenAxisIdentities.Contains(axisIdentity))
            {
                return false;
            }

            return true;
        }
    }
}