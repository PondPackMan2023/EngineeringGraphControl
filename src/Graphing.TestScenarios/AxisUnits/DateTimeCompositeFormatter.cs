using System;
using System.Collections.Generic;
using System.Globalization;
using UnitRegistry.Formatting;

namespace Graphing.TestScenarios.AxisUnits
{
    internal sealed class DateTimeCompositeFormatter : IValueFormatter, IValueParser
    {
        private static readonly DateTime UnixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTimeFormats[] SupportedFormats = (DateTimeFormats[])Enum.GetValues(typeof(DateTimeFormats));

        public const string FormatterIdentity = "datetime";

        public DateTimeCompositeFormatter(DateTimeFormats selectedFormat = DateTimeFormats.ShortDateAndShortTime, IFormatProvider formatProvider = null)
        {
            Id = new FormatterId(FormatterIdentity);
            FormatProvider = formatProvider;
            SelectedFormat = selectedFormat;
        }

        public FormatterId Id { get; }

        public Type ValueType => typeof(double);

        public IFormatProvider FormatProvider { get; }

        public DateTimeFormats SelectedFormat { get; private set; }

        public IReadOnlyList<DateTimeFormats> GetSupportedFormats()
        {
            return SupportedFormats;
        }

        public void SetSelectedFormat(DateTimeFormats selectedFormat)
        {
            SelectedFormat = selectedFormat;
        }

        public DateTimeCompositeFormatter WithSelectedFormat(DateTimeFormats selectedFormat)
        {
            return new DateTimeCompositeFormatter(selectedFormat, FormatProvider);
        }

        public string Format(object value, IFormatProvider formatProvider = null)
        {
            if (!(value is double))
            {
                throw new ArgumentException("Value must be of type double.", nameof(value));
            }

            return Format((double)value, formatProvider);
        }

        public string Format(double value, IFormatProvider formatProvider = null)
        {
            var provider = formatProvider ?? FormatProvider ?? CultureInfo.CurrentCulture;

            switch (SelectedFormat)
            {
                case DateTimeFormats.ElapsedTimeShort:
                    return FormatElapsedShort(TimeSpan.FromSeconds(value));
                case DateTimeFormats.ElapsedTimeLong:
                    return FormatElapsedLong(TimeSpan.FromSeconds(value));
                case DateTimeFormats.ShortTime:
                    return FormatDurationShort(TimeSpan.FromSeconds(value));
                case DateTimeFormats.LongTime:
                    return FormatDurationLong(TimeSpan.FromSeconds(value));
                case DateTimeFormats.ShortDate:
                    return ToUtcDateTime(value).ToString("d", provider);
                case DateTimeFormats.LongDate:
                    return ToUtcDateTime(value).ToString("D", provider);
                case DateTimeFormats.ShortDateAndShortTime:
                    return ToUtcDateTime(value).ToString("g", provider);
                case DateTimeFormats.ShortDateAndLongTime:
                    return string.Format(
                        provider,
                        "{0} {1}",
                        ToUtcDateTime(value).ToString("d", provider),
                        ToUtcDateTime(value).ToString("T", provider));
                case DateTimeFormats.LongDateAndShortTime:
                    return ToUtcDateTime(value).ToString("f", provider);
                case DateTimeFormats.LongDateAndLongTime:
                    return ToUtcDateTime(value).ToString("F", provider);
                case DateTimeFormats.SortableDateTime:
                    return ToUtcDateTime(value).ToString("s", provider);
                case DateTimeFormats.UniversalSortableDateTime:
                    return ToUtcDateTime(value).ToUniversalTime().ToString("u", provider);
                case DateTimeFormats.UniversalFullDateAndTime:
                    return ToUtcDateTime(value).ToString("U", provider);
                default:
                    return ToUtcDateTime(value).ToString("g", provider);
            }
        }

        public bool TryParse(string text, IFormatProvider formatProvider, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var provider = formatProvider ?? FormatProvider ?? CultureInfo.CurrentCulture;
            var trimmed = text.Trim();

            switch (SelectedFormat)
            {
                case DateTimeFormats.ElapsedTimeShort:
                case DateTimeFormats.ElapsedTimeLong:
                case DateTimeFormats.ShortTime:
                case DateTimeFormats.LongTime:
                    if (!TryParseTimeSpan(trimmed, provider, out var duration))
                    {
                        return false;
                    }

                    value = duration.TotalSeconds;
                    return true;
                default:
                    if (!DateTime.TryParse(trimmed, provider, DateTimeStyles.AllowWhiteSpaces, out var dateTime))
                    {
                        return false;
                    }

                    value = dateTime.ToUniversalTime().Subtract(UnixEpochUtc).TotalSeconds;
                    return true;
            }
        }

        private static DateTime ToUtcDateTime(double seconds)
        {
            return UnixEpochUtc.AddSeconds(seconds);
        }

        private static string FormatElapsedShort(TimeSpan timeSpan)
        {
            var sign = timeSpan < TimeSpan.Zero ? "-" : string.Empty;
            var normalized = timeSpan < TimeSpan.Zero ? timeSpan.Duration() : timeSpan;
            var days = normalized.Days;

            if (days > 0)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}.{2:00}:{3:00}",
                    sign,
                    days,
                    normalized.Hours,
                    normalized.Minutes);
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1:00}:{2:00}",
                sign,
                (int)normalized.TotalHours,
                normalized.Minutes);
        }

        private static string FormatElapsedLong(TimeSpan timeSpan)
        {
            var sign = timeSpan < TimeSpan.Zero ? "-" : string.Empty;
            var normalized = timeSpan < TimeSpan.Zero ? timeSpan.Duration() : timeSpan;
            var dayToken = normalized.Days == 1 ? "day" : "days";

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1} {2} {3:00}:{4:00}:{5:00}",
                sign,
                normalized.Days,
                dayToken,
                normalized.Hours,
                normalized.Minutes,
                normalized.Seconds);
        }

        private static string FormatDurationShort(TimeSpan timeSpan)
        {
            var sign = timeSpan < TimeSpan.Zero ? "-" : string.Empty;
            var normalized = timeSpan < TimeSpan.Zero ? timeSpan.Duration() : timeSpan;

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1:00}:{2:00}",
                sign,
                (int)normalized.TotalHours,
                normalized.Minutes);
        }

        private static string FormatDurationLong(TimeSpan timeSpan)
        {
            var sign = timeSpan < TimeSpan.Zero ? "-" : string.Empty;
            var normalized = timeSpan < TimeSpan.Zero ? timeSpan.Duration() : timeSpan;

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1:00}:{2:00}:{3:00}",
                sign,
                (int)normalized.TotalHours,
                normalized.Minutes,
                normalized.Seconds);
        }

        private static bool TryParseTimeSpan(string text, IFormatProvider provider, out TimeSpan value)
        {
            var styles = TimeSpanStyles.None;
            var exactFormats = new[]
            {
                @"d\.hh\:mm",
                @"d\.hh\:mm\:ss",
                @"hh\:mm",
                @"hh\:mm\:ss",
                @"h\:mm",
                @"h\:mm\:ss"
            };

            if (TimeSpan.TryParseExact(text, exactFormats, provider, styles, out value))
            {
                return true;
            }

            return TimeSpan.TryParse(text, provider, out value);
        }
    }
}
