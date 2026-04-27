using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.TestHarness.Libraries
{
    internal static class NumericFormatterLibrary
    {
        private static NumericFormatter _timeFormatter;
        private static NumericFormatter _elevationFormatter;
        private static NumericFormatter _pressureFormatter;

        internal static NumericFormatter DefaultTimeFormatter
        {
            get
            {
                if (_timeFormatter == null)
                    _timeFormatter = GetTimeFormatter();
                return _timeFormatter;
            }
        }

        internal static NumericFormatter DefaultElevationFormatter
        {
            get
            {
                if (_elevationFormatter == null)
                    _elevationFormatter = GetElevationFormatter();
                return _elevationFormatter;
            }
        }

        internal static NumericFormatter DefaultPressureFormatter
        {
            get
            {
                if (_pressureFormatter == null)
                    _pressureFormatter = GetPressureFormatter();
                return _pressureFormatter;
            }
        }

        public static NumericFormatter GetElevationFormatter(string formatSpecifier = null)
        {
            return new NumericFormatter(new NumericFormatterId("elevation"),
                UnitsRegistry.Default, formatSpecifier == null ? "G" : formatSpecifier);
        }

        public static NumericFormatter GetPressureFormatter(string formatSpecifier = null)
        {
            return new NumericFormatter(new NumericFormatterId("pressure"),
                UnitsRegistry.Default, formatSpecifier == null ? "G" : formatSpecifier);
        }

        public static NumericFormatter GetTimeFormatter(string formatSpecifier = null)
        {
            return new NumericFormatter(new NumericFormatterId("time"),
                UnitsRegistry.Default, formatSpecifier == null ? "G" : formatSpecifier);
        }
    }
}
