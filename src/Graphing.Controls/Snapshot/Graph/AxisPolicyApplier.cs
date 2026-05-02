using System;
using System.Collections.Generic;
using Graphing.Controls.Models;
using UnitRegistry;

namespace Graphing.Controls.Snapshot
{
    internal sealed class AxisPolicyResolution
    {
        public AxisPolicyResolution(double? minimum, double? maximum, double? increment, int majorTickStride)
        {
            Minimum = minimum;
            Maximum = maximum;
            Increment = increment;
            MajorTickStride = majorTickStride;
        }

        public double? Minimum { get; private set; }

        public double? Maximum { get; private set; }

        public double? Increment { get; private set; }

        public int MajorTickStride { get; private set; }
    }

    internal static class AxisPolicyApplier
    {
        private const double Epsilon = 1e-12;
        private const int DenseTickCountMinimum = 9;
        private const int DenseTickCountMaximum = 13;
        internal const int MaxDenseRefinementIterations = 8;

        internal sealed class DenseNumericRefinementResult
        {
            public DenseNumericRefinementResult(double minimum, double increment, int tickCount, int iterations)
            {
                Minimum = minimum;
                Increment = increment;
                TickCount = tickCount;
                Iterations = iterations;
            }

            public double Minimum { get; private set; }

            public double Increment { get; private set; }

            public int TickCount { get; private set; }

            public int Iterations { get; private set; }
        }

        public static AxisPolicyResolution Apply(
            IAxisModel axis,
            IReadOnlyList<IFieldSnapshot> contributingFields,
            double? actualMinimum,
            double? actualMaximum,
            double? minimum,
            double? maximum,
            double? increment,
            bool hasUserRangeOverride,
            bool hasUserIncrementOverride,
            bool enforceMinimumZero,
            bool enableDenseNumericYAxisTicks,
            ISet<Dimension> denseNumericYAxisExcludedDimensions)
        {
            if (axis == null
                || axis.Orientation != AxisOrientation.Y
                || !axis.IsAutoRange
                || hasUserRangeOverride
                || hasUserIncrementOverride)
            {
                return new AxisPolicyResolution(minimum, maximum, increment, 1);
            }

            if (!actualMinimum.HasValue || !actualMaximum.HasValue)
            {
                return new AxisPolicyResolution(minimum, maximum, increment, 1);
            }

            if (IsBinaryAxis(actualMinimum.Value, actualMaximum.Value, contributingFields))
            {
                return new AxisPolicyResolution(0d, 1d, 1d, 1);
            }

            if (enforceMinimumZero && minimum.HasValue && minimum.Value < 0d)
            {
                minimum = 0d;
            }

            if (IsPercentAxis(axis.Unit))
            {
                ApplyPercentPadding(ref minimum, ref maximum, actualMinimum.Value, actualMaximum.Value, enforceMinimumZero);
            }

            if (ShouldApplyDenseNumericYAxisPolicy(
                axis,
                increment,
                minimum,
                maximum,
                enableDenseNumericYAxisTicks,
                denseNumericYAxisExcludedDimensions))
            {
                var fixedMaximum = maximum.Value;
                var baselineMinimum = minimum.Value;
                var denseResult = RefineDenseNumericIncrement(actualMinimum.Value, fixedMaximum, increment.Value);

                increment = denseResult.Increment;

                var adjustedMaximum = Math.Ceiling((actualMaximum.Value / increment.Value) - Epsilon) * increment.Value;
                if (adjustedMaximum < actualMaximum.Value)
                {
                    adjustedMaximum += increment.Value;
                }

                var provisionalMinimum = denseResult.Minimum;
                var majorStrideForAnchoring = ComputeMajorTickStride(provisionalMinimum, adjustedMaximum, increment);
                var majorInterval = increment.Value * majorStrideForAnchoring;
                var anchorSourceMinimum = increment.Value >= 1d
                    ? baselineMinimum
                    : denseResult.Minimum;

                minimum = ComputeMajorAnchoredMinimum(adjustedMaximum, anchorSourceMinimum, majorInterval);
                maximum = adjustedMaximum;
            }

            if (enforceMinimumZero && minimum.HasValue && minimum.Value < 0d)
            {
                minimum = 0d;
            }

            var stride = ComputeMajorTickStride(minimum, maximum, increment);

            return new AxisPolicyResolution(minimum, maximum, increment, stride);
        }

        internal static double ComputeMajorAnchoredMinimum(double axisMaximum, double anchorMinimum, double majorInterval)
        {
            if (majorInterval <= Epsilon)
            {
                return anchorMinimum;
            }

            var span = axisMaximum - anchorMinimum;
            if (span <= Epsilon)
            {
                return axisMaximum;
            }

            var intervalCount = (int)Math.Floor(span / majorInterval);
            if (intervalCount < 0)
            {
                intervalCount = 0;
            }

            return axisMaximum - (intervalCount * majorInterval);
        }

        private static bool ShouldApplyDenseNumericYAxisPolicy(
            IAxisModel axis,
            double? increment,
            double? minimum,
            double? maximum,
            bool enableDenseNumericYAxisTicks,
            ISet<Dimension> denseNumericYAxisExcludedDimensions)
        {
            if (!enableDenseNumericYAxisTicks
                || axis == null
                || axis.Orientation != AxisOrientation.Y
                || axis.Unit == null
                    || axis.Formatter == null
                || !increment.HasValue
                || increment.Value <= 0d
                || !minimum.HasValue
                || !maximum.HasValue)
            {
                return false;
            }

            if (denseNumericYAxisExcludedDimensions != null
                && axis.Unit.Dimension != null
                && denseNumericYAxisExcludedDimensions.Contains(axis.Unit.Dimension))
            {
                return false;
            }

            var span = maximum.Value - minimum.Value;
            if (span <= Epsilon)
            {
                return false;
            }

            var baselineTicks = span / increment.Value;
            return baselineTicks <= 6d;
        }

        internal static DenseNumericRefinementResult RefineDenseNumericIncrement(double actualMinimum, double fixedMaximum, double baselineIncrement)
        {
            var span = fixedMaximum - actualMinimum;
            if (span <= Epsilon)
            {
                return new DenseNumericRefinementResult(actualMinimum, baselineIncrement, 2, 0);
            }

            // Scale-invariant refinement: normalize range to span mantissa space,
            // run the existing increment ladder there, then scale back.
            var exponent = Math.Floor(Math.Log10(span));
            var scale = Math.Pow(10d, exponent);
            if (scale <= 0d || double.IsNaN(scale) || double.IsInfinity(scale))
            {
                scale = 1d;
            }

            var normalizedActualMinimum = actualMinimum / scale;
            var normalizedFixedMaximum = fixedMaximum / scale;
            var normalizedBaselineIncrement = baselineIncrement / scale;
            if (normalizedBaselineIncrement <= 0d || double.IsNaN(normalizedBaselineIncrement) || double.IsInfinity(normalizedBaselineIncrement))
            {
                normalizedBaselineIncrement = baselineIncrement;
            }

            var targetMinimumTicks = DenseTickCountMinimum;
            var targetMaximumTicks = DenseTickCountMaximum;
            var targetCenterTicks = (targetMinimumTicks + targetMaximumTicks) / 2;

            var currentIncrement = normalizedBaselineIncrement;
            var currentOutcome = BuildDenseOutcome(normalizedActualMinimum, normalizedFixedMaximum, currentIncrement);
            var iterations = 0;

            while (iterations < MaxDenseRefinementIterations)
            {
                double candidateIncrement;

                if (currentOutcome.TickCount < targetMinimumTicks)
                {
                    candidateIncrement = GetNextSmallerIncrement(currentIncrement);
                }
                else if (currentOutcome.TickCount > targetMaximumTicks)
                {
                    candidateIncrement = GetNextLargerIncrement(currentIncrement);
                }
                else if (currentOutcome.TickCount < targetCenterTicks)
                {
                    candidateIncrement = GetNextSmallerIncrement(currentIncrement);
                }
                else if (currentOutcome.TickCount > targetCenterTicks)
                {
                    candidateIncrement = GetNextLargerIncrement(currentIncrement);
                }
                else
                {
                    break;
                }

                if (candidateIncrement <= 0d || AreClose(candidateIncrement, currentIncrement))
                {
                    break;
                }

                var candidateOutcome = BuildDenseOutcome(normalizedActualMinimum, normalizedFixedMaximum, candidateIncrement);
                if (!IsBetterDenseOutcome(candidateOutcome.TickCount, currentOutcome.TickCount, targetMinimumTicks, targetMaximumTicks, targetCenterTicks))
                {
                    break;
                }

                currentIncrement = candidateIncrement;
                currentOutcome = candidateOutcome;
                iterations++;

                if (currentOutcome.TickCount == targetCenterTicks)
                {
                    break;
                }
            }

            return new DenseNumericRefinementResult(
                currentOutcome.Minimum * scale,
                currentIncrement * scale,
                currentOutcome.TickCount,
                iterations);
        }

        private static bool IsDenseTickCountInRange(int tickCount)
        {
            return tickCount >= DenseTickCountMinimum && tickCount <= DenseTickCountMaximum;
        }

        private static bool IsTickCountInRange(int tickCount, int minimumTicks, int maximumTicks)
        {
            return tickCount >= minimumTicks && tickCount <= maximumTicks;
        }

        private static bool IsBetterDenseOutcome(int candidateTickCount, int currentTickCount, int minimumTicks, int maximumTicks, int centerTicks)
        {
            var candidateDistance = DistanceFromDenseTickTarget(candidateTickCount, minimumTicks, maximumTicks);
            var currentDistance = DistanceFromDenseTickTarget(currentTickCount, minimumTicks, maximumTicks);

            if (candidateDistance < currentDistance)
            {
                return true;
            }

            if (candidateDistance > currentDistance)
            {
                return false;
            }

            var candidateCenterDistance = Math.Abs(candidateTickCount - centerTicks);
            var currentCenterDistance = Math.Abs(currentTickCount - centerTicks);
            if (candidateCenterDistance < currentCenterDistance)
            {
                return true;
            }

            if (candidateCenterDistance > currentCenterDistance)
            {
                return false;
            }

            // Equal center distance: prefer denser in-range candidate to avoid under-ticking.
            return candidateTickCount > currentTickCount;
        }

        private static int DistanceFromDenseTickTarget(int tickCount, int minimumTicks, int maximumTicks)
        {
            if (tickCount < minimumTicks)
            {
                return minimumTicks - tickCount;
            }

            if (tickCount > maximumTicks)
            {
                return tickCount - maximumTicks;
            }

            return 0;
        }

        private sealed class DenseOutcome
        {
            public DenseOutcome(double minimum, int tickCount)
            {
                Minimum = minimum;
                TickCount = tickCount;
            }

            public double Minimum { get; private set; }

            public int TickCount { get; private set; }
        }

        private static DenseOutcome BuildDenseOutcome(double actualMinimum, double fixedMaximum, double increment)
        {
            var span = Math.Max(0d, fixedMaximum - actualMinimum);
            var intervalCount = (int)Math.Ceiling((span / increment) - Epsilon);
            if (intervalCount < 0)
            {
                intervalCount = 0;
            }

            var minimum = fixedMaximum - (intervalCount * increment);
            return new DenseOutcome(minimum, intervalCount + 1);
        }

        private static double GetNextSmallerIncrement(double increment)
        {
            if (increment <= 0d)
            {
                return 0d;
            }

            var multipliers = new[] { 1d, 1.25d, 2d, 2.5d, 5d, 10d };
            var exponent = Math.Floor(Math.Log10(increment));
            var power = Math.Pow(10d, exponent);
            var normalized = increment / power;
            var index = FindClosestMultiplierIndex(normalized, multipliers);

            if (index > 0)
            {
                return multipliers[index - 1] * power;
            }

            return 5d * Math.Pow(10d, exponent - 1d);
        }

        private static double GetNextLargerIncrement(double increment)
        {
            if (increment <= 0d)
            {
                return 0d;
            }

            var multipliers = new[] { 1d, 1.25d, 2d, 2.5d, 5d, 10d };
            var exponent = Math.Floor(Math.Log10(increment));
            var power = Math.Pow(10d, exponent);
            var normalized = increment / power;
            var index = FindClosestMultiplierIndex(normalized, multipliers);

            if (index < multipliers.Length - 1)
            {
                return multipliers[index + 1] * power;
            }

            return Math.Pow(10d, exponent + 1d);
        }

        private static int FindClosestMultiplierIndex(double normalized, double[] multipliers)
        {
            var bestIndex = 0;
            var bestDistance = double.MaxValue;

            for (var index = 0; index < multipliers.Length; index++)
            {
                var distance = Math.Abs(normalized - multipliers[index]);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        private static bool IsBinaryAxis(double actualMinimum, double actualMaximum, IReadOnlyList<IFieldSnapshot> contributingFields)
        {
            return AreClose(actualMinimum, 0d)
                && AreClose(actualMaximum, 1d)
                && AreAllValuesIntegral(contributingFields);
        }

        private static bool IsPercentAxis(UnitRegistry.Unit unit)
        {
            return unit != null && unit.Dimension == UnitRegistry.Dimensions.Percent;
        }

        private static void ApplyPercentPadding(
            ref double? minimum,
            ref double? maximum,
            double actualMinimum,
            double actualMaximum,
            bool enforceMinimumZero)
        {
            var dataMinimum = actualMinimum;
            var dataMaximum = actualMaximum;
            var dataSpan = dataMaximum - dataMinimum;

            if (dataSpan < Epsilon)
            {
                dataSpan = Math.Max(Math.Abs(dataMaximum), 1d) * 0.20d;
                dataMinimum = actualMinimum - (dataSpan / 2d);
                dataMaximum = actualMaximum + (dataSpan / 2d);
            }

            var padding = dataSpan * 0.125d;
            var paddedMinimum = dataMinimum - padding;
            var paddedMaximum = dataMaximum + padding;

            if (enforceMinimumZero && paddedMinimum < 0d)
            {
                paddedMinimum = 0d;
            }

            minimum = paddedMinimum;
            maximum = paddedMaximum;
        }

        private static bool AreAllValuesIntegral(IReadOnlyList<IFieldSnapshot> fields)
        {
            if (fields == null || fields.Count == 0)
            {
                return false;
            }

            for (var fieldIndex = 0; fieldIndex < fields.Count; fieldIndex++)
            {
                var field = fields[fieldIndex];
                if (field == null || field.Values == null)
                {
                    continue;
                }

                var hasAnyValue = false;

                foreach (var value in field.Values)
                {
                    if (value == null)
                    {
                        continue;
                    }

                    hasAnyValue = true;

                    var numericValue = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                    var rounded = Math.Round(numericValue);
                    if (!AreClose(numericValue, rounded))
                    {
                        return false;
                    }
                }

                if (!hasAnyValue)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Chooses a major tick label stride from {1, 2, 5} such that
        /// majorTickCount = (minorIntervals / stride) + 1 falls within 9–13.
        /// Prefers the largest stride that satisfies the window.
        /// Falls back to 1 when no stride fits.
        /// </summary>
        internal static int ComputeMajorTickStride(double? minimum, double? maximum, double? increment)
        {
            if (!minimum.HasValue || !maximum.HasValue || !increment.HasValue || increment.Value <= 0d)
            {
                return 1;
            }

            var span = maximum.Value - minimum.Value;
            if (span <= Epsilon)
            {
                return 1;
            }

            var minorIntervals = (int)Math.Round(span / increment.Value);

            var candidates = new[] { 5, 2, 1 };
            foreach (var stride in candidates)
            {
                var majorCount = (minorIntervals / stride) + 1;
                if (majorCount >= DenseTickCountMinimum && majorCount <= DenseTickCountMaximum)
                {
                    return stride;
                }
            }

            return 1;
        }

        private static bool AreClose(double left, double right)
        {
            return Math.Abs(left - right) <= Epsilon;
        }
    }
}
