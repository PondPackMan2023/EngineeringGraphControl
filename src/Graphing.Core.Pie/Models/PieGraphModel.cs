using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Core.Pie.Models
{
    public class PieGraphModel : IPieGraphModel
    {
        private readonly IReadOnlyList<IPieSliceModel> _slices;

        public PieGraphModel(
            string title,
            Unit unit,
            IValueFormatter formatter,
            IReadOnlyList<IPieSliceModel> slices)
        {
            Title = title;
            Unit = unit;
            Formatter = formatter;
            _slices = new ReadOnlyCollection<IPieSliceModel>(
                [.. slices ?? []]);
        }

        public string Title { get; }

        public Unit Unit { get; }

        public IValueFormatter Formatter { get; }

        public IReadOnlyList<IPieSliceModel> Slices => _slices;
    }
}
