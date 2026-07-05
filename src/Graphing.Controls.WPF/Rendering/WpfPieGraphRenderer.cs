using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Graphing.Core.Pie.Presentation;
using DrawingRectangle = System.Drawing.Rectangle;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfPoint = System.Windows.Point;
using WpfRect = System.Windows.Rect;

namespace Graphing.Controls.WPF.Rendering
{
    internal sealed class WpfPieGraphRenderer
    {
        private const double SliceBorderWidth = 1d;
        private const double LegendBorderWidth = 1d;
        private const double LegendSwatchToEntryHeightRatio = 0.50d;
        private const double LegendSwatchToEntryWidthRatio = 0.10d;
        private const double LegendHorizontalPadding = 2d;
        private const double LegendTextOffset = 3d;
        private const double TitleTopPadding = 4d;
        private const double FullCircleEpsilonDegrees = 1e-6;
        private const double ZeroSweepEpsilonDegrees = 1e-9;
        private const double LegendFontSize = 8d;
        private const double TitleFontSize = 12d;
        private const string FontFamily = "Arial";

        private static readonly SolidColorBrush TextBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        private static readonly Pen SliceBorderPen = CreatePen(Brushes.Black, SliceBorderWidth);
        private static readonly Pen LegendBorderPen = CreatePen(Brushes.Black, LegendBorderWidth);

        static WpfPieGraphRenderer()
        {
            TextBrush.Freeze();
        }

        public void Render(DrawingContext drawingContext, DrawingRectangle deviceBounds, PieGraphPresentationModel presentationModel)
        {
            ArgumentNullException.ThrowIfNull(drawingContext);

            if (presentationModel == null || deviceBounds.Width <= 0 || deviceBounds.Height <= 0)
            {
                return;
            }

            DrawBackground(drawingContext, deviceBounds);
            DrawSlices(drawingContext, deviceBounds, presentationModel);
            DrawLegend(drawingContext, deviceBounds, presentationModel);
            DrawTitle(drawingContext, deviceBounds, presentationModel);
        }

        internal static double NormalizedToDeviceX(double normalizedX, double width)
        {
            return normalizedX * width;
        }

        internal static double NormalizedToDeviceY(double normalizedY, double height)
        {
            return (1d - normalizedY) * height;
        }

        internal static PiePoint ComputePointOnCircle(PiePoint center, double radius, double angleDegrees)
        {
            var radians = DegreesToRadians(angleDegrees);
            var x = center.X + (radius * Math.Cos(radians));
            var y = center.Y + (radius * Math.Sin(radians));
            return new PiePoint(x, y);
        }

        internal static bool IsZeroSweep(double sweepAngle)
        {
            return Math.Abs(sweepAngle) <= ZeroSweepEpsilonDegrees;
        }

        internal static bool IsFullCircle(double sweepAngle)
        {
            return Math.Abs(Math.Abs(sweepAngle) - 360d) <= FullCircleEpsilonDegrees;
        }

        internal static int ToArgb(PieColor color)
        {
            return color.ToArgb();
        }

        private static void DrawBackground(DrawingContext dc, DrawingRectangle deviceBounds)
        {
            dc.DrawRectangle(Brushes.White, null, new WpfRect(0d, 0d, deviceBounds.Width, deviceBounds.Height));
        }

        private static void DrawSlices(DrawingContext dc, DrawingRectangle deviceBounds, PieGraphPresentationModel model)
        {
            var slices = model.Slices;
            if (slices == null || slices.Count == 0)
            {
                return;
            }

            var center = ToDevicePoint(model.Center, deviceBounds);
            var radiusX = model.Radius * deviceBounds.Width;
            var radiusY = model.Radius * deviceBounds.Height;
            var radius = Math.Min(radiusX, radiusY);

            if (radius <= 0d)
            {
                return;
            }

            for (var i = 0; i < slices.Count; i++)
            {
                var slice = slices[i];
                if (slice == null || IsZeroSweep(slice.SweepAngle))
                {
                    continue;
                }

                var fillBrush = CreateSliceBrush(slice.Color);

                if (IsFullCircle(slice.SweepAngle))
                {
                    dc.DrawEllipse(fillBrush, SliceBorderPen, center, radius, radius);
                    continue;
                }

                var wedge = CreateSliceWedgeGeometry(slice, center, radius);
                dc.DrawGeometry(fillBrush, SliceBorderPen, wedge);
            }
        }

        private static void DrawLegend(DrawingContext dc, DrawingRectangle deviceBounds, PieGraphPresentationModel model)
        {
            if (!(model.Options?.LegendVisible ?? true))
            {
                return;
            }

            var legend = model.Legend;
            if (legend == null)
            {
                return;
            }

            var legendRect = ToDeviceRect(legend.Bounds, deviceBounds);
            if (legendRect.Width <= 0d || legendRect.Height <= 0d)
            {
                return;
            }

            // Draw legend border only if the presentation option enables it
            if (model.Options?.ShowLegendBorder ?? false)
            {
                dc.DrawRectangle(null, LegendBorderPen, legendRect);
            }

            dc.PushClip(new RectangleGeometry(legendRect));
            try
            {
                var entries = legend.Entries;
                if (entries == null)
                {
                    return;
                }

                for (var i = 0; i < entries.Count; i++)
                {
                    DrawLegendEntry(dc, deviceBounds, entries[i]);
                }
            }
            finally
            {
                dc.Pop();
            }
        }

        private static void DrawLegendEntry(DrawingContext dc, DrawingRectangle deviceBounds, PieLegendEntryPresentationGeometry entry)
        {
            if (entry == null)
            {
                return;
            }

            var entryRect = ToDeviceRect(entry.Bounds, deviceBounds);
            if (entryRect.Width <= 0d || entryRect.Height <= 0d)
            {
                return;
            }

            // Check if this is a "More..." indicator (transparent color)
            var isMoreIndicator = entry.Color.A == 0;
            
            WpfRect? swatchRect = null;
            if (!isMoreIndicator)
            {
                var swatchSize = Math.Min(entryRect.Height * LegendSwatchToEntryHeightRatio, entryRect.Width * LegendSwatchToEntryWidthRatio);
                var swatchLeft = entryRect.Left + LegendHorizontalPadding;
                var swatchTop = entryRect.Top + ((entryRect.Height - swatchSize) / 2d);
                swatchRect = new WpfRect(swatchLeft, swatchTop, swatchSize, swatchSize);

                var swatchBrush = CreateSliceBrush(entry.Color);
                dc.DrawRectangle(swatchBrush, SliceBorderPen, swatchRect.Value);
            }

            if (string.IsNullOrWhiteSpace(entry.Label))
            {
                return;
            }

            var text = MakeFormattedText(entry.Label, LegendFontSize);
            
            // Position text after swatch or at left margin for "More..." entries
            double textX;
            if (isMoreIndicator)
            {
                textX = entryRect.Left + LegendHorizontalPadding;
            }
            else
            {
                textX = swatchRect!.Value.Right + LegendTextOffset;
            }
            
            var textY = entryRect.Top + ((entryRect.Height - text.Height) / 2d);
            dc.DrawText(text, new WpfPoint(textX, textY));
        }

        private static void DrawTitle(DrawingContext dc, DrawingRectangle deviceBounds, PieGraphPresentationModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return;
            }

            var text = MakeFormattedText(model.Title, TitleFontSize, bold: true);
            var textX = (deviceBounds.Width - text.Width) / 2d;
            dc.DrawText(text, new WpfPoint(textX, TitleTopPadding));
        }

        private static StreamGeometry CreateSliceWedgeGeometry(
            PieSlicePresentationGeometry slice,
            WpfPoint deviceCenter,
            double radius)
        {
            var startPoint = ComputeDevicePointOnCircle(deviceCenter, radius, slice.StartAngle);
            var endPoint = ComputeDevicePointOnCircle(deviceCenter, radius, slice.StartAngle + slice.SweepAngle);

            var isLargeArc = Math.Abs(slice.SweepAngle) > 180d;
            var direction = slice.SweepAngle >= 0d ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(deviceCenter, isFilled: true, isClosed: true);
                context.LineTo(startPoint, isStroked: true, isSmoothJoin: true);
                context.ArcTo(
                    endPoint,
                    new Size(radius, radius),
                    rotationAngle: 0d,
                    isLargeArc: isLargeArc,
                    sweepDirection: direction,
                    isStroked: true,
                    isSmoothJoin: true);
                context.LineTo(deviceCenter, isStroked: true, isSmoothJoin: true);
            }

            geometry.Freeze();
            return geometry;
        }

        private static WpfPoint ComputeDevicePointOnCircle(WpfPoint center, double radius, double angleDegrees)
        {
            var radians = DegreesToRadians(angleDegrees);
            var x = center.X + (radius * Math.Cos(radians));
            var y = center.Y - (radius * Math.Sin(radians));
            return new WpfPoint(x, y);
        }

        private static WpfPoint ToDevicePoint(PiePoint point, DrawingRectangle deviceBounds)
        {
            var x = NormalizedToDeviceX(point.X, deviceBounds.Width);
            var y = NormalizedToDeviceY(point.Y, deviceBounds.Height);
            return new WpfPoint(x, y);
        }

        private static WpfRect ToDeviceRect(PieBounds bounds, DrawingRectangle deviceBounds)
        {
            var left = NormalizedToDeviceX(bounds.Left, deviceBounds.Width);
            var right = NormalizedToDeviceX(bounds.Right, deviceBounds.Width);
            var top = NormalizedToDeviceY(bounds.Top, deviceBounds.Height);
            var bottom = NormalizedToDeviceY(bounds.Bottom, deviceBounds.Height);
            return new WpfRect(left, top, Math.Max(0d, right - left), Math.Max(0d, bottom - top));
        }

        private static double DegreesToRadians(double degrees)
        {
            return Math.PI * degrees / 180d;
        }

        private static SolidColorBrush CreateSliceBrush(PieColor color)
        {
            var brush = new SolidColorBrush(ToWpfColor(color));
            brush.Freeze();
            return brush;
        }

        private static Color ToWpfColor(PieColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private static FormattedText MakeFormattedText(string text, double emSize, bool bold = false)
        {
            var typeface = new Typeface(
                new WpfFontFamily(FontFamily),
                FontStyles.Normal,
                bold ? FontWeights.Bold : FontWeights.Normal,
                FontStretches.Normal);

            return new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                emSize,
                TextBrush,
                96d);
        }

        private static Pen CreatePen(Brush brush, double thickness)
        {
            var pen = new Pen(brush, thickness);
            pen.Freeze();
            return pen;
        }
    }
}