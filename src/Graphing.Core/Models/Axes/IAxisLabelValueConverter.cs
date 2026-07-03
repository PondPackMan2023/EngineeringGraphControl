using System;

namespace Graphing.Controls.Models
{
    /// <summary>
    /// Converts a numeric axis coordinate value into a semantic value for axis label formatting.
    /// </summary>
    public interface IAxisLabelValueConverter
    {
        /// <summary>
        /// Gets the semantic value type produced by the converter.
        /// </summary>
        Type TargetValueType { get; }

        /// <summary>
        /// Converts a coordinate-domain numeric value into a semantic formatter input value.
        /// </summary>
        /// <param name="coordinateValue">Numeric coordinate value used by axis geometry.</param>
        /// <param name="formatProvider">Optional format provider for culture-aware conversion.</param>
        /// <returns>Semantic value supplied to formatter-based axis label formatting.</returns>
        object Convert(double coordinateValue, IFormatProvider formatProvider = null);
    }
}
