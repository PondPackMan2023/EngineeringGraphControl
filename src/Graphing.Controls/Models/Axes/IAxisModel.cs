using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Controls.Models
{
    public interface IAxisModel
    {
        string Id { get; }

        AxisOrientation Orientation { get; }

        AxisSide Side { get; }

        Unit Unit { get; }

        string UnitLabel { get; }

        NumericFormatter NumericFormatter { get; }

        AxisScaleType ScaleType { get; }

        bool IsAutoRange { get; }

        double? MinimumValue { get; }

        double? MaximumValue { get; }
    }
}