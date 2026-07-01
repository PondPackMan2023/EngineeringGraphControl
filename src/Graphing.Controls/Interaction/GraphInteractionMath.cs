using System;
using System.Collections.Generic;
using System.Drawing;

namespace Graphing.Controls.Interaction
{
    internal enum ZoomGestureDirection
    {
        None,
        ZoomIn,
        ZoomReset,
    }

    internal static class GraphInteractionMath
    {
        internal static ZoomGestureDirection ClassifyZoomGesture(int deltaX, int deltaY)
        {
            if (deltaX > 0 && deltaY > 0)
            {
                return ZoomGestureDirection.ZoomIn;
            }

            if (deltaX < 0 && deltaY < 0)
            {
                return ZoomGestureDirection.ZoomReset;
            }

            return ZoomGestureDirection.None;
        }

        internal static bool TryResolveVerticalPolylineIntersection(
            float verticalX,
            IReadOnlyList<PointF> polyline,
            out float intersectionY)
        {
            intersectionY = 0f;

            if (polyline == null || polyline.Count < 2)
            {
                return false;
            }

            for (var i = 1; i < polyline.Count; i++)
            {
                var p0 = polyline[i - 1];
                var p1 = polyline[i];
                var minX = Math.Min(p0.X, p1.X);
                var maxX = Math.Max(p0.X, p1.X);

                if (verticalX < minX || verticalX > maxX)
                {
                    continue;
                }

                var dx = p1.X - p0.X;
                if (Math.Abs(dx) < float.Epsilon)
                {
                    if (Math.Abs(verticalX - p0.X) <= 0.5f)
                    {
                        intersectionY = (p0.Y + p1.Y) * 0.5f;
                        return true;
                    }

                    continue;
                }

                var t = (verticalX - p0.X) / dx;
                if (t < 0f || t > 1f)
                {
                    continue;
                }

                intersectionY = p0.Y + ((p1.Y - p0.Y) * t);
                return true;
            }

            return false;
        }

        internal static bool TryResolvePointSeriesIntersectionCenter(
            float verticalX,
            IReadOnlyList<PointF> points,
            float tolerancePixels,
            out PointF centerPoint)
        {
            centerPoint = PointF.Empty;

            if (points == null || points.Count == 0)
            {
                return false;
            }

            var bestIndex = -1;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < points.Count; i++)
            {
                var distance = Math.Abs(points[i].X - verticalX);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0 || bestDistance > tolerancePixels)
            {
                return false;
            }

            centerPoint = points[bestIndex];
            return true;
        }

        internal static int ResolveNearestRenderedXSampleIndex(IReadOnlyList<float> sortedXSamples, float deviceX)
        {
            if (sortedXSamples == null || sortedXSamples.Count == 0)
            {
                return 0;
            }

            var nearestIndex = 0;
            var nearestDistance = float.MaxValue;

            for (var i = 0; i < sortedXSamples.Count; i++)
            {
                var distance = Math.Abs(sortedXSamples[i] - deviceX);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        internal static bool TryResolveNearestPointCenterX(
            IReadOnlyList<float> pointCenterXSamples,
            float deviceX,
            float tolerancePixels,
            out float snappedCenterX)
        {
            snappedCenterX = 0f;

            if (pointCenterXSamples == null || pointCenterXSamples.Count == 0)
            {
                return false;
            }

            var nearestIndex = ResolveNearestRenderedXSampleIndex(pointCenterXSamples, deviceX);
            if (nearestIndex < 0 || nearestIndex >= pointCenterXSamples.Count)
            {
                return false;
            }

            var candidateX = pointCenterXSamples[nearestIndex];
            if (Math.Abs(candidateX - deviceX) > tolerancePixels)
            {
                return false;
            }

            snappedCenterX = candidateX;
            return true;
        }
    }
}
