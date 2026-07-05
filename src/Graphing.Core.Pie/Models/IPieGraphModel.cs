using System.Collections.Generic;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Core.Pie.Models
{
    public interface IPieGraphModel
    {
        string Title { get; }

        Unit Unit { get; }

        IValueFormatter Formatter { get; }

        IReadOnlyList<IPieSliceModel> Slices { get; }
    }
}
