using System;

namespace Graphing.Core.Pie.Presentation
{
    /// <summary>
    /// Performs hit testing on pie graph presentation geometry.
    /// Determines which slice (if any) is under a given normalized coordinate.
    /// </summary>
    public static class PieHitTestHelper
    {
        /// <summary>
        /// Determines the slice under the given normalized coordinate.
        /// </summary>
        /// <param name="normalizedX">X coordinate in normalized space (0.0 to 1.0).</param>
        /// <param name="normalizedY">Y coordinate in normalized space (0.0 to 1.0).</param>
        /// <param name="presentation">The presentation model containing center, radius, and slice geometry.</param>
        /// <returns>The PieSliceId if a slice is under the position, null otherwise.</returns>
        public static PieSliceId? HitTest(double normalizedX, double normalizedY, PieGraphPresentationModel presentation)
        {
            if (presentation == null)
            {
                return null;
            }

            var center = presentation.Center;
            var radius = presentation.Radius;
            var slices = presentation.Slices;

            // Calculate distance from center
            var dx = normalizedX - center.X;
            var dy = normalizedY - center.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            // If outside radius, no hit
            if (distance > radius || distance == 0)
            {
                return null;
            }

            // Calculate angle from center (in degrees, 0° is East, measured counter-clockwise)
            var angleRadians = Math.Atan2(dy, dx);
            var angleDegrees = angleRadians * 180 / Math.PI;

            // Normalize to 0-360 range
            if (angleDegrees < 0)
            {
                angleDegrees += 360;
            }

            // Check which slice contains this angle
            foreach (var slice in slices)
            {
                if (IsAngleInSlice(angleDegrees, slice.StartAngle, slice.SweepAngle))
                {
                    return slice.Id;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines if an angle falls within a slice's angular range.
        /// </summary>
        private static bool IsAngleInSlice(double angle, double startAngle, double sweepAngle)
        {
            // Handle zero-sweep slices (no area)
            if (Math.Abs(sweepAngle) < 1e-9)
            {
                return false;
            }

            // Handle full circle (360 degrees or more)
            if (Math.Abs(sweepAngle) >= 360 - 1e-9)
            {
                return true;
            }

            // Normalize angle and startAngle to 0-360
            angle = NormalizeAngle(angle);
            startAngle = NormalizeAngle(startAngle);
            
            // For positive sweep (counter-clockwise)
            if (sweepAngle > 0)
            {
                var endAngle = startAngle + sweepAngle;
                if (endAngle <= 360)
                {
                    // No wrapping: [start, end]
                    return angle >= startAngle && angle <= endAngle;
                }
                else
                {
                    // Wraps around 360: [start, 360] or [0, endAngle-360]
                    var wrappedEnd = endAngle - 360;
                    return angle >= startAngle || angle <= wrappedEnd;
                }
            }
            else
            {
                // Negative sweep (clockwise)
                var endAngle = startAngle + sweepAngle;  // This will be negative or < startAngle
                if (endAngle >= 0)
                {
                    // No wrapping: [end, start]
                    return angle >= endAngle && angle <= startAngle;
                }
                else
                {
                    // Wraps around 0: need to use [0, end+360] or [start, 360]
                    var wrappedEnd = endAngle + 360;
                    return angle <= startAngle || angle >= wrappedEnd;
                }
            }
        }

        /// <summary>
        /// Normalizes an angle to 0-360 range.
        /// </summary>
        private static double NormalizeAngle(double angle)
        {
            angle = angle % 360;
            if (angle < 0)
            {
                angle += 360;
            }

            return angle;
        }
    }
}
