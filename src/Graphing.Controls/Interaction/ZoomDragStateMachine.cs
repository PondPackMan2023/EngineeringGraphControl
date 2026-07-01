using System;
using System.Drawing;

namespace Graphing.Controls.Interaction
{
    internal sealed class ZoomDragStateMachine
    {
        private Point _anchor;
        private Point _current;

        public bool IsDragging { get; private set; }

        public RectangleF? ZoomRectangle { get; private set; }

        public bool TryBeginDrag(Point rawPoint, RectangleF plotRect)
        {
            if (plotRect.Width <= 0f || plotRect.Height <= 0f)
            {
                return false;
            }

            _anchor = ClampPointToRectangle(rawPoint, plotRect);
            _current = _anchor;
            ZoomRectangle = BuildZoomRectangle(_anchor, _current);
            IsDragging = true;
            return true;
        }

        public bool TryMoveDrag(Point rawPoint, RectangleF plotRect)
        {
            if (!IsDragging || plotRect.Width <= 0f || plotRect.Height <= 0f)
            {
                return false;
            }

            _current = ClampPointToRectangle(rawPoint, plotRect);
            ZoomRectangle = BuildZoomRectangle(_anchor, _current);
            return true;
        }

        public bool TryEndDrag(out ZoomGestureDirection gesture, out RectangleF? capturedRect)
        {
            gesture = ZoomGestureDirection.None;
            capturedRect = null;

            if (!IsDragging)
            {
                return false;
            }

            var dx = _current.X - _anchor.X;
            var dy = _current.Y - _anchor.Y;
            capturedRect = ZoomRectangle;

            IsDragging = false;
            ZoomRectangle = null;
            gesture = GraphInteractionMath.ClassifyZoomGesture(dx, dy);
            return true;
        }

        public void Clear()
        {
            IsDragging = false;
            ZoomRectangle = null;
        }

        private static Point ClampPointToRectangle(Point point, RectangleF rectangle)
        {
            var minX = (int)Math.Ceiling(rectangle.Left);
            var maxX = (int)Math.Floor(rectangle.Right);
            var minY = (int)Math.Ceiling(rectangle.Top);
            var maxY = (int)Math.Floor(rectangle.Bottom);

            var clampedX = point.X;
            if (clampedX < minX)
            {
                clampedX = minX;
            }
            else if (clampedX > maxX)
            {
                clampedX = maxX;
            }

            var clampedY = point.Y;
            if (clampedY < minY)
            {
                clampedY = minY;
            }
            else if (clampedY > maxY)
            {
                clampedY = maxY;
            }

            return new Point(clampedX, clampedY);
        }

        private static RectangleF BuildZoomRectangle(Point anchor, Point current)
        {
            var left = Math.Min(anchor.X, current.X);
            var right = Math.Max(anchor.X, current.X);
            var top = Math.Min(anchor.Y, current.Y);
            var bottom = Math.Max(anchor.Y, current.Y);
            return RectangleF.FromLTRB(left, top, right, bottom);
        }
    }
}
