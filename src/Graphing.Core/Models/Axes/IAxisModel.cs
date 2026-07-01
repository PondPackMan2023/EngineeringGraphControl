using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Controls.Models
{
    public interface IAxisModel
    {
        AxisId Id { get; }

        AxisOrientation Orientation { get; }

        AxisSide Side { get; }

        Unit Unit { get; }

        string UnitLabel { get; }

        IValueFormatter Formatter { get; }

        AxisScaleType ScaleType { get; }

        bool IsAutoRange { get; }

        double? MinimumValue { get; }

        double? MaximumValue { get; }

        IAxisModel ChangeUnit(Unit newUnit);

        IAxisModel ChangeFormat(IValueFormatter newFormatter);
    }
}