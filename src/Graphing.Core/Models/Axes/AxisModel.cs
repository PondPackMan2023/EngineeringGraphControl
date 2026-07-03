using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Controls.Models
{
    public class AxisModel : IAxisModel
    {
        public AxisModel(
                AxisId id,
                AxisOrientation orientation,
                AxisSide side,
                Unit unit,
                string unitLabel,
                    IValueFormatter formatter,
                IAxisLabelValueConverter labelValueConverter = null)
        {
            Id = id;
            Orientation = orientation;
            Side = side;
            Unit = unit;
            UnitLabel = unitLabel;
                Formatter = formatter;
            LabelValueConverter = labelValueConverter;
            ScaleType = AxisScaleType.Linear;
            IsAutoRange = true;
        }

        public AxisId Id { get; }
        public AxisOrientation Orientation { get; }
        public AxisSide Side { get; }
        public Unit Unit { get; }
        public string UnitLabel { get; }
        public IValueFormatter Formatter { get; }
        public IAxisLabelValueConverter LabelValueConverter { get; }
        public AxisScaleType ScaleType { get; }
        public bool IsAutoRange { get; }
        public double? MinimumValue => null;
        public double? MaximumValue => null;

        public IAxisModel ChangeUnit(Unit newUnit)
        {
            var newUnitLabel = newUnit != null && newUnit.Id != null ? newUnit.Id.Value : null;
              return new AxisModel(Id, Orientation, Side, newUnit, newUnitLabel, Formatter, LabelValueConverter);
        }

        public IAxisModel ChangeFormat(IValueFormatter newFormatter)
        {
            return new AxisModel(Id, Orientation, Side, Unit, UnitLabel, newFormatter, LabelValueConverter);
        }
    }
}
