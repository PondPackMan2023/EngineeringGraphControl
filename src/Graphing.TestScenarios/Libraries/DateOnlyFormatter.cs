using System;
using System.Globalization;
using UnitRegistry.Formatting;

namespace Graphing.TestScenarios.Libraries
{
    /// <summary>
    /// Demonstration formatter that formats DateOnly values for semantic axis labels.
    /// </summary>
    internal sealed class DateOnlyFormatter : IValueFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyFormatter"/> class.
        /// </summary>
        /// <param name="id">Stable identity for the formatter.</param>
        /// <param name="formatString">Date format string used for label rendering.</param>
        /// <param name="formatProvider">Default format provider. If null, current culture is used.</param>
        public DateOnlyFormatter(FormatterId id, string formatString = "yyyy-MM-dd", IFormatProvider formatProvider = null)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(formatString))
            {
                throw new ArgumentException("Format string must not be null, empty, or whitespace.", nameof(formatString));
            }

            Id = id;
            FormatString = formatString;
            FormatProvider = formatProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyFormatter"/> class.
        /// </summary>
        /// <param name="id">Stable identity value for the formatter.</param>
        /// <param name="formatString">Date format string used for label rendering.</param>
        /// <param name="formatProvider">Default format provider. If null, current culture is used.</param>
        public DateOnlyFormatter(string id, string formatString = "yyyy-MM-dd", IFormatProvider formatProvider = null)
            : this(new FormatterId(id), formatString, formatProvider)
        {
        }

        /// <summary>
        /// Gets the stable identity of this formatter.
        /// </summary>
        public FormatterId Id { get; }

        /// <summary>
        /// Gets the supported value type.
        /// </summary>
        public Type ValueType
        {
            get { return typeof(DateOnly); }
        }

        /// <summary>
        /// Gets the DateOnly format string used by this formatter.
        /// </summary>
        public string FormatString { get; }

        /// <summary>
        /// Gets the default format provider used when none is provided at call time.
        /// </summary>
        public IFormatProvider FormatProvider { get; }

        /// <summary>
        /// Formats a DateOnly value.
        /// </summary>
        /// <param name="value">DateOnly value to format.</param>
        /// <param name="formatProvider">Optional format provider override.</param>
        /// <returns>A formatted string representation of the value.</returns>
        public string Format(DateOnly value, IFormatProvider formatProvider = null)
        {
            IFormatProvider provider = formatProvider ?? FormatProvider ?? CultureInfo.CurrentCulture;
            return value.ToString(FormatString, provider);
        }

        /// <summary>
        /// Formats an object value as DateOnly.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <param name="formatProvider">Optional format provider override.</param>
        /// <returns>A formatted string representation of the value.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a DateOnly.</exception>
        public string Format(object value, IFormatProvider formatProvider = null)
        {
            if (!(value is DateOnly))
            {
                throw new ArgumentException("Value must be of type DateOnly.", nameof(value));
            }

            return Format((DateOnly)value, formatProvider);
        }
    }
}
