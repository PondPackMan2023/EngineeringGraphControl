using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Graphing.Core.Pie.Presentation
{
    public sealed class PieLegendPresentationGeometry
    {
        private readonly IReadOnlyList<PieLegendEntryPresentationGeometry> _entries;

        public PieLegendPresentationGeometry(
            PieLegendPlacement placement,
            PieBounds bounds,
            IEnumerable<PieLegendEntryPresentationGeometry> entries)
        {
            Placement = placement;
            Bounds = bounds;
            _entries = new ReadOnlyCollection<PieLegendEntryPresentationGeometry>(
                new List<PieLegendEntryPresentationGeometry>(entries ?? Array.Empty<PieLegendEntryPresentationGeometry>()));
        }

        public PieLegendPlacement Placement { get; }

        public PieBounds Bounds { get; }

        public IReadOnlyList<PieLegendEntryPresentationGeometry> Entries => _entries;
    }
}
