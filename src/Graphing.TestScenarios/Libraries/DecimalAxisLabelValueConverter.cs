using System;
using System.Collections.Generic;
using System.Globalization;
using Graphing.Controls.Models;

namespace Graphing.TestScenarios.Libraries
{
    /// <summary>
    /// Demonstration converter that maps numeric coordinates to decimal values for semantic currency labeling.
    /// </summary>
    internal sealed class DecimalAxisLabelValueConverter : IAxisLabelValueConverter
    {
        private readonly List<double> _receivedCoordinates = new List<double>();

        /// <summary>
        /// Gets the semantic value type produced by this converter.
        /// </summary>
        public Type TargetValueType
        {
            get { return typeof(decimal); }
        }

        /// <summary>
        /// Gets numeric coordinates received by the converter.
        /// </summary>
        public IReadOnlyList<double> ReceivedCoordinates
        {
            get { return _receivedCoordinates; }
        }

        /// <summary>
        /// Converts a numeric coordinate into decimal.
        /// </summary>
        /// <param name="coordinateValue">Numeric coordinate value.</param>
        /// <param name="formatProvider">Ignored by this converter.</param>
        /// <returns>Decimal representation of the coordinate value.</returns>
        public object Convert(double coordinateValue, IFormatProvider formatProvider = null)
        {
            _receivedCoordinates.Add(coordinateValue);
            return System.Convert.ToDecimal(coordinateValue, CultureInfo.InvariantCulture);
        }
    }
}
