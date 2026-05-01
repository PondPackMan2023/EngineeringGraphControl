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
            _formatterRegistry.Register(new NumericFormatter(FormatterId.Time_Extended, UnitsRegistry.Default, "Time", NumericFormat.Fixed(2)));
            _formatterRegistry.Register(new NumericFormatter(FormatterId.Elevation, UnitsRegistry.Default, "Elevation", NumericFormat.Fixed(2)));
            _formatterRegistry.Register(new NumericFormatter(FormatterId.Pressure, UnitsRegistry.Default, "Pressure", NumericFormat.Fixed(2)));
        }

        internal static void ChangeFormat(NumericFormatterId id, string formatSpecifier)
        {
            _formatterRegistry.ChangeFormat(id, formatSpecifier);
        }

        internal static NumericFormatter TimeFormatter => _formatterRegistry.Get(FormatterId.Time_Extended);

        internal static NumericFormatter ElevationFormatter => _formatterRegistry.Get(FormatterId.Elevation);

        internal static NumericFormatter PressureFormatter => _formatterRegistry.Get(FormatterId.Pressure);
    }
}
