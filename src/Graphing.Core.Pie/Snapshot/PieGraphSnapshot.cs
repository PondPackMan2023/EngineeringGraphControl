using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Core.Pie.Snapshot
{
    public sealed class PieGraphSnapshot
    {
        private readonly IReadOnlyList<PieSliceSnapshot> _slices;

        public PieGraphSnapshot(
            string title,
            Unit unit,
            IValueFormatter? formatter,
            double totalValue,
            IEnumerable<PieSliceSnapshot> slices)
        {
            Title = title;
            Unit = unit;
            Formatter = formatter;
            TotalValue = totalValue;
            _slices = new ReadOnlyCollection<PieSliceSnapshot>(
                new List<PieSliceSnapshot>(slices ?? Array.Empty<PieSliceSnapshot>()));
        }

        public string Title { get; }

        public Unit Unit { get; }

        public IValueFormatter? Formatter { get; }

        public double TotalValue { get; }

        public IReadOnlyList<PieSliceSnapshot> Slices => _slices;
    }
}
