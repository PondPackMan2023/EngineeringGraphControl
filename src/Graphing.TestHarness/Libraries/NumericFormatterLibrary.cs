using System.Runtime.InteropServices;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.TestHarness.Libraries
{
    internal static class NumericFormatterLibrary
    {
        private static FormatterRegistry _formatterRegistry = new FormatterRegistry();

        static NumericFormatterLibrary()
        {
            _formatterRegistry.Register(new NumericFormatter(FormatterIds.Time_Extended, UnitsRegistry.Default, "Time", NumericFormat.Fixed(2)));
            _formatterRegistry.Register(new NumericFormatter(FormatterIds.Elevation, UnitsRegistry.Default, "Elevation", NumericFormat.Fixed(2)));
            _formatterRegistry.Register(new NumericFormatter(FormatterIds.Pressure, UnitsRegistry.Default, "Pressure", NumericFormat.Fixed(2)));
        }

        internal static void ChangeFormat(FormatterId id, string formatSpecifier)
        {
            _formatterRegistry.ChangeFormat(id, formatSpecifier);
        }

        internal static NumericFormatter TimeFormatter => _formatterRegistry.Get(FormatterIds.Time_Extended);

        internal static NumericFormatter ElevationFormatter => _formatterRegistry.Get(FormatterIds.Elevation);

        internal static NumericFormatter PressureFormatter => _formatterRegistry.Get(FormatterIds.Pressure);
    }
}
