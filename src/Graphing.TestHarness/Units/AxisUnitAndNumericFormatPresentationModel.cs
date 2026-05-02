using Graphing.Controls.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.TestHarness.AxisUnits
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

        private readonly NumericFormatter currentFormatter;
        private readonly Unit previewSourceUnit;
        private readonly List<Unit> availableUnits;

        public AxisUnitAndNumericFormatPresentationModel(AxisId axisId, Unit currentDisplayUnit, NumericFormatter numericFormatter)
        {
            if (axisId == null)
            {
                throw new ArgumentNullException(nameof(axisId));
            }

            if (currentDisplayUnit == null)
            {
                throw new ArgumentNullException(nameof(currentDisplayUnit));
            }

            if (numericFormatter == null)
            {
                throw new ArgumentNullException(nameof(numericFormatter));
            }

            AxisId = axisId;
            currentFormatter = numericFormatter;
            previewSourceUnit = currentDisplayUnit;

            var unitRegistry = numericFormatter.UnitRegistry ?? UnitsRegistry.Default;
            availableUnits = unitRegistry
                .GetUnits(currentDisplayUnit.Dimension)
                .OrderBy(unit => unit.Label, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!availableUnits.Any(unit => unit.Equals(currentDisplayUnit)))
            {
                availableUnits.Add(currentDisplayUnit);
            }

            SelectedUnit = availableUnits.FirstOrDefault(unit => unit.Equals(currentDisplayUnit)) ?? currentDisplayUnit;

            ParseFormatSpecifier(numericFormatter.FormatSpecifier, out var formatKind, out var precision);
            SelectedFormatKind = formatKind;
            DisplayPrecision = precision;
        }

        public AxisId AxisId { get; }

        public IReadOnlyList<Unit> AvailableUnits => availableUnits;

        public Unit SelectedUnit { get; private set; }

        public AxisNumericFormatKind SelectedFormatKind { get; private set; }

        public int DisplayPrecision { get; private set; }

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

        public bool TrySetDisplayPrecision(string precisionText)
        {
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

        public NumericFormatter BuildFormatterToApply()
        {
            return new NumericFormatter(
                currentFormatter.Id,
                currentFormatter.UnitRegistry,
                currentFormatter.Label,
                BuildFormatSpecifier(),
                currentFormatter.FormatProvider);
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
