using System;
using System.Globalization;
using Graphing.Core.Pie.Models;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.TestScenarios.Scenarios
{
    internal static class PieScenarioDefinitions
    {
        internal static IPieGraphModel BuildBasicPie()
        {
            return BuildCurrencyModel(
                "Basic Pie",
                "basic-pie",
                new[]
                {
                    ("North", 35d),
                    ("South", 25d),
                    ("East", 20d),
                    ("West", 20d)
                });
        }

        internal static IPieGraphModel BuildSpendingByCategory()
        {
            return BuildCurrencyModel(
                "Spending By Category",
                "spending-by-category",
                new[]
                {
                    ("Housing", 1850d),
                    ("Food", 620d),
                    ("Transport", 310d),
                    ("Utilities", 270d),
                    ("Insurance", 240d),
                    ("Savings", 510d)
                });
        }

        internal static IPieGraphModel BuildSingleSlice()
        {
            return BuildCurrencyModel(
                "Single Slice",
                "single-slice",
                new[]
                {
                    ("Whole", 100d)
                });
        }

        internal static IPieGraphModel BuildManySlices()
        {
            return BuildCurrencyModel(
                "Many Slices",
                "many-slices",
                new[]
                {
                    ("A", 12d),
                    ("B", 9d),
                    ("C", 15d),
                    ("D", 6d),
                    ("E", 8d),
                    ("F", 10d),
                    ("G", 7d),
                    ("H", 11d),
                    ("I", 13d),
                    ("J", 9d)
                });
        }

        internal static IPieGraphModel BuildPaletteRepeat()
        {
            return BuildCurrencyModel(
                "Palette Repeat",
                "palette-repeat",
                new[]
                {
                    ("Slice 01", 5d),
                    ("Slice 02", 6d),
                    ("Slice 03", 7d),
                    ("Slice 04", 8d),
                    ("Slice 05", 9d),
                    ("Slice 06", 10d),
                    ("Slice 07", 11d),
                    ("Slice 08", 12d),
                    ("Slice 09", 13d),
                    ("Slice 10", 14d),
                    ("Slice 11", 15d),
                    ("Slice 12", 16d),
                    ("Slice 13", 17d),
                    ("Slice 14", 18d),
                    ("Slice 15", 19d),
                    ("Slice 16", 20d),
                    ("Slice 17", 21d),
                    ("Slice 18", 22d)
                });
        }

        internal static IPieGraphModel BuildZeroValueSlice()
        {
            return BuildCurrencyModel(
                "Zero Value Slice",
                "zero-value-slice",
                new[]
                {
                    ("Operations", 70d),
                    ("Maintenance", 30d),
                    ("Future", 0d)
                });
        }

        internal static IPieGraphModel BuildAllZeroSlices()
        {
            return BuildCurrencyModel(
                "All Zero Slices",
                "all-zero-slices",
                new[]
                {
                    ("A", 0d),
                    ("B", 0d),
                    ("C", 0d),
                    ("D", 0d)
                });
        }

        internal static IPieGraphModel BuildLegendHidden()
        {
            return BuildCurrencyModel(
                "Legend Hidden",
                "legend-hidden",
                new[]
                {
                    ("Alpha", 45d),
                    ("Beta", 30d),
                    ("Gamma", 25d)
                });
        }

        private static IPieGraphModel BuildCurrencyModel(
            string title,
            string formatterSuffix,
            (string Label, double Value)[] slices)
        {
            var sliceModels = new IPieSliceModel[slices.Length];
            for (var i = 0; i < slices.Length; i++)
            {
                sliceModels[i] = new PieSliceModel($"slice-{i}", slices[i].Label, slices[i].Value);
            }

            return new PieGraphModel(
                title,
                Units.Currency.Dollars,
                BuildCurrencyFormatter(formatterSuffix),
                sliceModels);
        }

        private static IValueFormatter BuildCurrencyFormatter(string suffix)
        {
            return new PieDoubleFormatter(
                "pie-currency-" + suffix,
                "N0",
                CultureInfo.GetCultureInfo("en-US"));
        }

        private sealed class PieDoubleFormatter : IValueFormatter
        {
            private readonly FormatterId _id;
            private readonly string _format;
            private readonly CultureInfo _culture;

            public PieDoubleFormatter(string id, string format, CultureInfo culture)
            {
                _id = new FormatterId(id);
                _format = format;
                _culture = culture;
            }

            public FormatterId Id => _id;

            public Type ValueType => typeof(double);

            public string Format(object value, IFormatProvider formatProvider = null)
            {
                var numericValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                var provider = formatProvider ?? _culture;
                return numericValue.ToString(_format, provider);
            }
        }
    }
}
