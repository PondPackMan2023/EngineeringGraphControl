using System;
using System.Collections.Generic;
using Graphing.Controls.Models;

namespace Graphing.TestScenarios.Libraries
{
    /// <summary>
    /// Demonstration converter that maps numeric day-number coordinates to DateOnly labels.
    /// </summary>
    internal sealed class DateOnlyDayNumberAxisLabelValueConverter : IAxisLabelValueConverter
    {
        private readonly List<double> _receivedCoordinates = new List<double>();

        /// <summary>
        /// Gets the semantic value type produced by this converter.
        /// </summary>
        public Type TargetValueType
        {
            get { return typeof(DateOnly); }
        }

        /// <summary>
        /// Gets numeric coordinates received by the converter.
        /// </summary>
        public IReadOnlyList<double> ReceivedCoordinates
        {
            get { return _receivedCoordinates; }
        }

        /// <summary>
        /// Converts a numeric day number into DateOnly.
        /// </summary>
        /// <param name="coordinateValue">Numeric coordinate value in day-number domain.</param>
        /// <param name="formatProvider">Ignored by this converter.</param>
        /// <returns>DateOnly corresponding to the coordinate value.</returns>
        public object Convert(double coordinateValue, IFormatProvider formatProvider = null)
        {
            _receivedCoordinates.Add(coordinateValue);
            int dayNumber = checked((int)Math.Round(coordinateValue, MidpointRounding.AwayFromZero));
            return DateOnly.FromDayNumber(dayNumber);
        }
    }
}
