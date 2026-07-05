using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Graphing.Core.Pie.Presentation
{
    public sealed class PieGraphPresentationModel
    {
        private readonly IReadOnlyList<PieSlicePresentationGeometry> _slices;

        public PieGraphPresentationModel(
            string title,
            PiePoint center,
            double radius,
            IEnumerable<PieSlicePresentationGeometry> slices,
            PieLegendPresentationGeometry? legend,
            PieGraphPresentationOptions options)
        {
            Title = title;
            Center = center;
            Radius = radius;
            Legend = legend;
            Options = options ?? new PieGraphPresentationOptions();
            _slices = new ReadOnlyCollection<PieSlicePresentationGeometry>(
                new List<PieSlicePresentationGeometry>(slices ?? Array.Empty<PieSlicePresentationGeometry>()));
        }

        public string Title { get; }

        public PiePoint Center { get; }

        public double Radius { get; }

        public IReadOnlyList<PieSlicePresentationGeometry> Slices => _slices;

        public PieLegendPresentationGeometry? Legend { get; }

        public PieGraphPresentationOptions Options { get; }
    }
}
