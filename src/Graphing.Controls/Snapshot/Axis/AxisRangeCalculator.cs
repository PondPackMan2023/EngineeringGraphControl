using System;

namespace Graphing.Controls.Snapshot
{
    public sealed class AxisRange
    {
        public AxisRange(double minimum, double maximum, double increment)
        {
            Minimum = minimum;
            Maximum = maximum;
            Increment = increment;
        }

        public double Minimum { get; }

        public double Maximum { get; }

        public double Increment { get; }
    }

    public static class AxisRangeCalculator
    {
        private const int MinimumDesiredTickCount = 2;

        public static AxisRange Calculate(double actualMin, double actualMax, int desiredTickCount)
        {
            if (double.IsNaN(actualMin) || double.IsInfinity(actualMin))
            {
                throw new ArgumentOutOfRangeException(nameof(actualMin));
            }

            if (double.IsNaN(actualMax) || double.IsInfinity(actualMax))
            {
                throw new ArgumentOutOfRangeException(nameof(actualMax));
            }

            if (desiredTickCount < MinimumDesiredTickCount)
            {
                throw new ArgumentOutOfRangeException(nameof(desiredTickCount));
            }

            if (actualMin > actualMax)
            {
                var tmp = actualMin;
                actualMin = actualMax;
                actualMax = tmp;
            }

            if (actualMin == actualMax)
            {
                var baseline = actualMin == 0d ? 1d : Math.Abs(actualMin);
                var zeroSpanIncrement = NiceNumber(baseline, true);
                var halfSpan = zeroSpanIncrement * ((desiredTickCount - 1) / 2.0);
                var min = actualMin - halfSpan;
                var max = actualMin + halfSpan;
                return new AxisRange(min, max, zeroSpanIncrement);
            }

            var rawRange = actualMax - actualMin;
            var increment = NiceNumber(rawRange / (desiredTickCount - 1), true);

            if (increment <= 0d || double.IsNaN(increment) || double.IsInfinity(increment))
            {
                increment = 1d;
            }

            var minimum = Math.Floor(actualMin / increment) * increment;
            var maximum = Math.Ceiling(actualMax / increment) * increment;

            if (maximum <= minimum)
            {
                maximum = minimum + increment;
            }

            return new AxisRange(minimum, maximum, increment);
        }

        private static double NiceNumber(double value, bool round)
        {
            if (value <= 0d)
            {
                return 1d;
            }

            var exponent = Math.Floor(Math.Log10(value));
            var power = Math.Pow(10d, exponent);
            var fraction = value / power;

            double niceFraction;
            if (round)
            {
                if (fraction < 1.5d)
                {
                    niceFraction = 1d;
                }
                else if (fraction < 3d)
                {
                    niceFraction = 2d;
                }
                else if (fraction < 7d)
                {
                    niceFraction = 5d;
                }
                else
                {
                    niceFraction = 10d;
                }
            }
            else
            {
                if (fraction <= 1d)
                {
                    niceFraction = 1d;
                }
                else if (fraction <= 2d)
                {
                    niceFraction = 2d;
                }
                else if (fraction <= 5d)
                {
                    niceFraction = 5d;
                }
                else
                {
                    niceFraction = 10d;
                }
            }

            return niceFraction * power;
        }
    }
}
