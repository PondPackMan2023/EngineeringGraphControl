using Graphing.Controls.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.TestScenarios.AxisUnits
{
    internal enum AxisNumericFormatKind
    {
        Scientific,
        FixedPoint,
        General,
        Number
    }

    internal sealed class AxisUnitAndNumericFormatPresentationModel
    {
        private const double PreviewSourceValue = 1.0d;

        private readonly IValueFormatter currentFormatter;
        private readonly NumericFormatter numericFormatter;
        private readonly DateTimeCompositeFormatter dateTimeFormatter;
        private readonly Unit previewSourceUnit;
        private readonly List<Unit> availableUnits;

        public AxisUnitAndNumericFormatPresentationModel(AxisId axisId, Unit currentDisplayUnit, IValueFormatter formatter)
        {
            if (axisId == null)
            {
                throw new ArgumentNullException(nameof(axisId));
            }

            if (currentDisplayUnit == null)
            {
                throw new ArgumentNullException(nameof(currentDisplayUnit));
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            AxisId = axisId;
            currentFormatter = formatter;
            numericFormatter = formatter as NumericFormatter;
            dateTimeFormatter = formatter as DateTimeCompositeFormatter;
            previewSourceUnit = currentDisplayUnit;

            var unitRegistry = numericFormatter != null
                ? numericFormatter.UnitRegistry ?? UnitsRegistry.Default
                : UnitsRegistry.Default;

            availableUnits = unitRegistry
                .GetUnits(currentDisplayUnit.Dimension)
                .OrderBy(unit => unit.Label, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!availableUnits.Any(unit => unit.Equals(currentDisplayUnit)))
            {
                availableUnits.Add(currentDisplayUnit);
            }

            SelectedUnit = availableUnits.FirstOrDefault(unit => unit.Equals(currentDisplayUnit)) ?? currentDisplayUnit;

            if (IsDateTimeMode)
            {
                SelectedDateTimeFormat = dateTimeFormatter != null
                    ? dateTimeFormatter.SelectedFormat
                    : DateTimeFormats.ShortDateAndShortTime;

                var dateTimeNumericFormatSpecifier = dateTimeFormatter != null
                    ? dateTimeFormatter.NumericFormatSpecifier
                    : NumericFormat.General();
                ParseFormatSpecifier(dateTimeNumericFormatSpecifier, out var dateTimeFormatKind, out var dateTimePrecision);
                SelectedFormatKind = dateTimeFormatKind;
                DisplayPrecision = dateTimePrecision;
            }
            else
            {
                var formatSpecifier = numericFormatter != null ? numericFormatter.FormatSpecifier : NumericFormat.General();
                ParseFormatSpecifier(formatSpecifier, out var formatKind, out var precision);
                SelectedFormatKind = formatKind;
                DisplayPrecision = precision;
            }
        }

        public AxisId AxisId { get; }

        public IReadOnlyList<Unit> AvailableUnits => availableUnits;

        public Unit SelectedUnit { get; private set; }

        public bool IsDateTimeMode
        {
            get
            {
                return currentFormatter != null
                    && currentFormatter.Id != null
                    && string.Equals(currentFormatter.Id.Value, DateTimeCompositeFormatter.FormatterIdentity, StringComparison.Ordinal);
            }
        }

        public AxisNumericFormatKind SelectedFormatKind { get; private set; }

        public DateTimeFormats SelectedDateTimeFormat { get; private set; }

        public int DisplayPrecision { get; private set; }

        public bool ShouldShowNumericFormattingControls
        {
            get
            {
                if (!IsDateTimeMode)
                {
                    return true;
                }

                return SelectedDateTimeFormat == DateTimeFormats.ElapsedTimeShort;
            }
        }

        public IReadOnlyList<DateTimeFormats> AvailableDateTimeFormats
        {
            get
            {
                if (dateTimeFormatter != null)
                {
                    return dateTimeFormatter.GetSupportedFormats();
                }

                return (DateTimeFormats[])Enum.GetValues(typeof(DateTimeFormats));
            }
        }

        public double PreviewValue => PreviewSourceValue;

        public string PreviewSourceUnitLabel => previewSourceUnit != null ? previewSourceUnit.Label : string.Empty;

        public string PreviewUnitLabel => SelectedUnit != null ? SelectedUnit.Label : string.Empty;

        public void SelectUnit(Unit unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            SelectedUnit = unit;
        }

        public void SetFormatKind(AxisNumericFormatKind formatKind)
        {
            SelectedFormatKind = formatKind;
        }

        public void SetDateTimeFormat(DateTimeFormats format)
        {
            SelectedDateTimeFormat = format;

            if (dateTimeFormatter != null)
            {
                dateTimeFormatter.SetSelectedFormat(format);
            }
        }

        public bool TrySetDisplayPrecision(string precisionText)
        {
            if (!ShouldShowNumericFormattingControls)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(precisionText))
            {
                return false;
            }

            if (!int.TryParse(precisionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return false;
            }

            if (parsed < 0)
            {
                return false;
            }

            DisplayPrecision = parsed;
            return true;
        }

        public string BuildPreviewText()
        {
            if (SelectedUnit == null)
            {
                return string.Empty;
            }

            var valueInBaseUnit = previewSourceUnit.ToBaseValue(PreviewSourceValue);
            var valueInSelectedUnit = SelectedUnit.FromBaseValue(valueInBaseUnit);
            var previewFormatter = BuildFormatterToApply();

            return previewFormatter.Format(valueInSelectedUnit);
        }

        public IValueFormatter BuildFormatterToApply()
        {
            if (IsDateTimeMode)
            {
                if (dateTimeFormatter != null)
                {
                    return dateTimeFormatter.WithFormatting(SelectedDateTimeFormat, BuildFormatSpecifier());
                }

                return currentFormatter;
            }

            if (numericFormatter == null)
            {
                return currentFormatter;
            }

            return new NumericFormatter(
                numericFormatter.Id,
                numericFormatter.UnitRegistry,
                numericFormatter.Label,
                BuildFormatSpecifier(),
                numericFormatter.FormatProvider);
        }

        public bool TryParseValue(string text, out double value)
        {
            value = 0d;

            var parser = BuildFormatterToApply() as IValueParser;
            if (parser == null)
            {
                return false;
            }

            if (!parser.TryParse(text, null, out var parsed))
            {
                return false;
            }

            if (!(parsed is double parsedValue))
            {
                return false;
            }

            value = parsedValue;
            return true;
        }

        public string GetDateTimeFormatDisplayName(DateTimeFormats format)
        {
            switch (format)
            {
                case DateTimeFormats.ElapsedTimeShort:
                    return "Elapsed Time (Short)";
                case DateTimeFormats.ElapsedTimeLong:
                    return "Elapsed Time (Long)";
                case DateTimeFormats.ShortTime:
                    return "Duration (Short Time)";
                case DateTimeFormats.LongTime:
                    return "Duration (Long Time)";
                case DateTimeFormats.ShortDate:
                    return "Short Date";
                case DateTimeFormats.LongDate:
                    return "Long Date";
                case DateTimeFormats.ShortDateAndShortTime:
                    return "Short Date and Short Time";
                case DateTimeFormats.ShortDateAndLongTime:
                    return "Short Date and Long Time";
                case DateTimeFormats.LongDateAndShortTime:
                    return "Long Date and Short Time";
                case DateTimeFormats.LongDateAndLongTime:
                    return "Long Date and Long Time";
                case DateTimeFormats.SortableDateTime:
                    return "Sortable Date/Time";
                case DateTimeFormats.UniversalSortableDateTime:
                    return "Universal Sortable Date/Time";
                case DateTimeFormats.UniversalFullDateAndTime:
                    return "Universal Full Date/Time";
                default:
                    return format.ToString();
            }
        }

        private string BuildFormatSpecifier()
        {
            switch (SelectedFormatKind)
            {
                case AxisNumericFormatKind.Scientific:
                    return NumericFormat.Scientific(DisplayPrecision);
                case AxisNumericFormatKind.FixedPoint:
                    return NumericFormat.Fixed(DisplayPrecision);
                case AxisNumericFormatKind.Number:
                    return "N" + DisplayPrecision.ToString(CultureInfo.InvariantCulture);
                default:
                    return DisplayPrecision > 0
                        ? "G" + DisplayPrecision.ToString(CultureInfo.InvariantCulture)
                        : "G";
            }
        }

        private static void ParseFormatSpecifier(string formatSpecifier, out AxisNumericFormatKind formatKind, out int precision)
        {
            formatKind = AxisNumericFormatKind.General;
            precision = 0;

            if (string.IsNullOrWhiteSpace(formatSpecifier))
            {
                return;
            }

            var first = char.ToUpperInvariant(formatSpecifier[0]);
            var precisionText = formatSpecifier.Length > 1 ? formatSpecifier.Substring(1) : string.Empty;

            switch (first)
            {
                case 'E':
                    formatKind = AxisNumericFormatKind.Scientific;
                    break;
                case 'F':
                    formatKind = AxisNumericFormatKind.FixedPoint;
                    break;
                case 'N':
                    formatKind = AxisNumericFormatKind.Number;
                    break;
                default:
                    formatKind = AxisNumericFormatKind.General;
                    break;
            }

            if (precisionText.Length > 0 && int.TryParse(precisionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPrecision) && parsedPrecision >= 0)
            {
                precision = parsedPrecision;
            }
        }
    }
}
