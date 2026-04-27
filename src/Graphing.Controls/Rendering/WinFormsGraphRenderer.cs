using System;
using System.Collections.Generic;
using System.Drawing;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Rendering
{
    /// <summary>
    /// WinForms renderer that consumes a <see cref="GraphPresentationModel"/> and draws
    /// axes (lines and ticks) and series lines onto a <see cref="Graphics"/> surface.
    ///
    /// The renderer is a pure consumer: it does not own, mutate, or cache any presentation
    /// objects, and does not participate in snapshot or control lifecycle decisions.
    /// </summary>
    internal sealed class WinFormsGraphRenderer
    {
        private const int TickLength = 5;
        private const int TickLabelOffset = 3;
        private const int AxisTitleOffset = 6;
        private const float AxisLineWidth = 1f;
        private const float SeriesLineWidth = 1.5f;

        private static readonly Pen AxisPen = new Pen(Color.Black, AxisLineWidth);
        private static readonly Pen SeriesPen = new Pen(Color.SteelBlue, SeriesLineWidth);
        private static readonly Font TickFont = new Font("Arial", 7f);
        private static readonly Font AxisTitleFont = new Font("Arial", 8f, FontStyle.Bold);
            private static readonly Font GraphTitleFont = new Font("Arial", 12f, FontStyle.Bold);
            private static readonly Font GraphSubtitleFont = new Font("Arial", 10f);
        private static readonly Brush TickLabelBrush = Brushes.Black;

        /// <summary>
        /// Renders axes and series from <paramref name="model"/> into <paramref name="g"/>
        /// within the specified <paramref name="deviceBounds"/>.
        /// </summary>
        internal void Render(Graphics g, Rectangle deviceBounds, GraphPresentationModel model)
        {
            if (g == null || model == null || deviceBounds.Width <= 0 || deviceBounds.Height <= 0)
            {
                return;
            }

            var padding = MeasureLabelPadding(g, model);
            var paddedBounds = ApplyPadding(deviceBounds, padding);
            var plotRect = ComputeDevicePlotRect(paddedBounds, model.Layout.PlotArea);

            if (plotRect.Width <= 0 || plotRect.Height <= 0)
            {
                return;
            }

            RenderAxes(g, plotRect, paddedBounds, model);
            RenderSeries(g, plotRect, model);
            RenderAxisTitles(g, plotRect, model);
                    RenderTitles(g, paddedBounds, model.Layout);
        }

        // ── Axis rendering ────────────────────────────────────────────────────

        private static void RenderAxes(
            Graphics g,
            RectangleF plotRect,
            Rectangle deviceBounds,
            GraphPresentationModel model)
        {
            var axisEntries = model.Layout.Axes;

            for (var i = 0; i < axisEntries.Count; i++)
            {
                var entry = axisEntries[i];
                var axis = entry.Axis;
                var axisRect = ComputeAxisRect(plotRect, entry);

                if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
                {
                    continue;
                }

                switch (entry.Side)
                {
                    case AxisSide.Bottom:
                        RenderBottomAxis(g, axisRect, axis);
                        break;

                    case AxisSide.Left:
                        RenderLeftAxis(g, axisRect, axis);
                        break;

                    case AxisSide.Right:
                        RenderRightAxis(g, axisRect, axis);
                        break;

                    case AxisSide.Top:
                        RenderTopAxis(g, axisRect, axis);
                        break;
                }
            }
        }

        private static void RenderBottomAxis(
            Graphics g,
            RectangleF plotRect,
            AxisPresentationGeometry axis)
        {
            var axisY = plotRect.Bottom;
            g.DrawLine(AxisPen, plotRect.Left, axisY, plotRect.Right, axisY);

            var ticks = axis.Ticks;
            for (var i = 0; i < ticks.Count; i++)
            {
                var tick = ticks[i];
                var deviceX = DomainToDeviceX(tick.Value, axis.MinimumValue.Value, axis.MaximumValue.Value, plotRect);
                g.DrawLine(AxisPen, deviceX, axisY, deviceX, axisY + TickLength);

                if (!string.IsNullOrEmpty(tick.Label))
                {
                    var labelSize = g.MeasureString(tick.Label, TickFont);
                    g.DrawString(
                        tick.Label,
                        TickFont,
                        TickLabelBrush,
                        deviceX - labelSize.Width / 2f,
                        axisY + TickLength + TickLabelOffset);
                }
            }
        }

        private static void RenderLeftAxis(
            Graphics g,
            RectangleF plotRect,
            AxisPresentationGeometry axis)
        {
            var axisX = plotRect.Left;
            g.DrawLine(AxisPen, axisX, plotRect.Top, axisX, plotRect.Bottom);

            var ticks = axis.Ticks;
            for (var i = 0; i < ticks.Count; i++)
            {
                var tick = ticks[i];
                var deviceY = DomainToDeviceY(tick.Value, axis.MinimumValue.Value, axis.MaximumValue.Value, plotRect);
                g.DrawLine(AxisPen, axisX - TickLength, deviceY, axisX, deviceY);

                if (!string.IsNullOrEmpty(tick.Label))
                {
                    var labelSize = g.MeasureString(tick.Label, TickFont);
                    g.DrawString(
                        tick.Label,
                        TickFont,
                        TickLabelBrush,
                        axisX - TickLength - TickLabelOffset - labelSize.Width,
                        deviceY - labelSize.Height / 2f);
                }
            }
        }

        private static void RenderRightAxis(
            Graphics g,
            RectangleF plotRect,
            AxisPresentationGeometry axis)
        {
            var axisX = plotRect.Right;
            g.DrawLine(AxisPen, axisX, plotRect.Top, axisX, plotRect.Bottom);

            var ticks = axis.Ticks;
            for (var i = 0; i < ticks.Count; i++)
            {
                var tick = ticks[i];
                var deviceY = DomainToDeviceY(tick.Value, axis.MinimumValue.Value, axis.MaximumValue.Value, plotRect);
                g.DrawLine(AxisPen, axisX, deviceY, axisX + TickLength, deviceY);

                if (!string.IsNullOrEmpty(tick.Label))
                {
                    g.DrawString(
                        tick.Label,
                        TickFont,
                        TickLabelBrush,
                        axisX + TickLength + TickLabelOffset,
                        deviceY - TickFont.Height / 2f);
                }
            }
        }

        private static void RenderTopAxis(
            Graphics g,
            RectangleF plotRect,
            AxisPresentationGeometry axis)
        {
            var axisY = plotRect.Top;
            g.DrawLine(AxisPen, plotRect.Left, axisY, plotRect.Right, axisY);

            var ticks = axis.Ticks;
            for (var i = 0; i < ticks.Count; i++)
            {
                var tick = ticks[i];
                var deviceX = DomainToDeviceX(tick.Value, axis.MinimumValue.Value, axis.MaximumValue.Value, plotRect);
                g.DrawLine(AxisPen, deviceX, axisY - TickLength, deviceX, axisY);

                if (!string.IsNullOrEmpty(tick.Label))
                {
                    var labelSize = g.MeasureString(tick.Label, TickFont);
                    g.DrawString(
                        tick.Label,
                        TickFont,
                        TickLabelBrush,
                        deviceX - labelSize.Width / 2f,
                        axisY - TickLength - TickLabelOffset - TickFont.Height);
                }
            }
        }

        private static void RenderAxisTitles(
            Graphics g,
            RectangleF plotRect,
            GraphPresentationModel model)
        {
            var axisEntries = model.Layout.Axes;

            for (var i = 0; i < axisEntries.Count; i++)
            {
                var entry = axisEntries[i];
                var axis = entry.Axis;
                var axisRect = ComputeAxisRect(plotRect, entry);

                if (string.IsNullOrWhiteSpace(axis.Title))
                {
                    continue;
                }

                switch (entry.Side)
                {
                    case AxisSide.Bottom:
                        RenderBottomAxisTitle(g, axisRect, axis);
                        break;

                    case AxisSide.Left:
                        RenderLeftAxisTitle(g, axisRect, axis);
                        break;

                    case AxisSide.Right:
                        RenderRightAxisTitle(g, axisRect, axis);
                        break;

                    case AxisSide.Top:
                        RenderTopAxisTitle(g, axisRect, axis);
                        break;
                }
            }
        }

        private static void RenderBottomAxisTitle(
            Graphics g,
            RectangleF plotRect,
            AxisPresentationGeometry axis)
        {
            var tickLabelSize = MeasureMaxTickLabelSize(g, axis);
            var titleSize = g.MeasureString(axis.Title, AxisTitleFont);
            var x = plotRect.Left + (plotRect.Width - titleSize.Width) / 2f;
            var y = plotRect.Bottom + TickLength + TickLabelOffset + tickLabelSize.Height + AxisTitleOffset;

            g.DrawString(axis.Title, AxisTitleFont, TickLabelBrush, x, y);
        }

        private static void RenderTopAxisTitle(
            Graphics g,
            RectangleF plotRect,
            AxisPresentationGeometry axis)
        {
            var tickLabelSize = MeasureMaxTickLabelSize(g, axis);
            var titleSize = g.MeasureString(axis.Title, AxisTitleFont);
            var x = plotRect.Left + (plotRect.Width - titleSize.Width) / 2f;
            var y = plotRect.Top - TickLength - TickLabelOffset - tickLabelSize.Height - AxisTitleOffset - titleSize.Height;

            g.DrawString(axis.Title, AxisTitleFont, TickLabelBrush, x, y);
        }

        private static void RenderLeftAxisTitle(
            Graphics g,
            RectangleF plotRect,
            AxisPresentationGeometry axis)
        {
            var tickLabelSize = MeasureMaxTickLabelSize(g, axis);
            var titleSize = g.MeasureString(axis.Title, AxisTitleFont);
            var desiredRight = plotRect.Left - TickLength - TickLabelOffset - tickLabelSize.Width - AxisTitleOffset;
            var centerX = desiredRight - titleSize.Height / 2f;
            var centerY = plotRect.Top + plotRect.Height / 2f;

            DrawRotatedCenteredText(g, axis.Title, AxisTitleFont, TickLabelBrush, centerX, centerY, -90f);
        }

        private static void RenderRightAxisTitle(
            Graphics g,
            RectangleF plotRect,
            AxisPresentationGeometry axis)
        {
            var tickLabelSize = MeasureMaxTickLabelSize(g, axis);
            var titleSize = g.MeasureString(axis.Title, AxisTitleFont);
            var desiredLeft = plotRect.Right + TickLength + TickLabelOffset + tickLabelSize.Width + AxisTitleOffset;
            var centerX = desiredLeft + titleSize.Height / 2f;
            var centerY = plotRect.Top + plotRect.Height / 2f;

            DrawRotatedCenteredText(g, axis.Title, AxisTitleFont, TickLabelBrush, centerX, centerY, 90f);
        }

        // ── Title rendering ───────────────────────────────────────────────────

        private static void RenderTitles(Graphics g, Rectangle deviceBounds, GraphLayoutModel layout)
        {
            if (layout == null)
            {
                return;
            }

            if (layout.Title != null)
            {
                RenderGraphTitle(g, deviceBounds, layout.Title);
            }

            if (layout.Subtitle != null)
            {
                RenderGraphSubtitle(g, deviceBounds, layout.Subtitle);
            }
        }

        private static void RenderGraphTitle(Graphics g, Rectangle deviceBounds, TitlePresentationGeometry title)
        {
            var titleRect = ComputeDeviceBoundsForGeometry(deviceBounds, title.BottomLeft, title.TopRight);

            if (titleRect.Width <= 0 || titleRect.Height <= 0)
            {
                return;
            }

            var titleSize = g.MeasureString(title.Text, GraphTitleFont);
            var centerX = titleRect.Left + (titleRect.Width - titleSize.Width) / 2f;
            var centerY = titleRect.Top + (titleRect.Height - titleSize.Height) / 2f;

            g.DrawString(title.Text, GraphTitleFont, TickLabelBrush, centerX, centerY);
        }

        private static void RenderGraphSubtitle(Graphics g, Rectangle deviceBounds, SubtitlePresentationGeometry subtitle)
        {
            var subtitleRect = ComputeDeviceBoundsForGeometry(deviceBounds, subtitle.BottomLeft, subtitle.TopRight);

            if (subtitleRect.Width <= 0 || subtitleRect.Height <= 0)
            {
                return;
            }

            var subtitleSize = g.MeasureString(subtitle.Text, GraphSubtitleFont);
            var centerX = subtitleRect.Left + (subtitleRect.Width - subtitleSize.Width) / 2f;
            var centerY = subtitleRect.Top + (subtitleRect.Height - subtitleSize.Height) / 2f;

            g.DrawString(subtitle.Text, GraphSubtitleFont, TickLabelBrush, centerX, centerY);
        }

        /// <summary>
        /// Maps abstract geometry bounds to device pixel coordinates.
        /// </summary>
        private static RectangleF ComputeDeviceBoundsForGeometry(
            Rectangle deviceBounds, GeometryPoint3D bottomLeft, GeometryPoint3D topRight)
        {
            var left = deviceBounds.Left + bottomLeft.X * deviceBounds.Width;
            var right = deviceBounds.Left + topRight.X * deviceBounds.Width;
            var top = deviceBounds.Bottom - topRight.Y * deviceBounds.Height;
            var bottom = deviceBounds.Bottom - bottomLeft.Y * deviceBounds.Height;

            return RectangleF.FromLTRB((float)left, (float)top, (float)right, (float)bottom);
        }

        private static void DrawRotatedCenteredText(
            Graphics g,
            string text,
            Font font,
            Brush brush,
            float centerX,
            float centerY,
            float angle)
        {
            var textSize = g.MeasureString(text, font);
            var state = g.Save();

            try
            {
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(angle);
                g.DrawString(text, font, brush, -textSize.Width / 2f, -textSize.Height / 2f);
            }
            finally
            {
                g.Restore(state);
            }
        }

        private static SizeF MeasureMaxTickLabelSize(Graphics g, AxisPresentationGeometry axis)
        {
            var ticks = axis.Ticks;
            var maxWidth = 0f;
            var maxHeight = 0f;

            for (var i = 0; i < ticks.Count; i++)
            {
                var label = ticks[i].Label;
                if (string.IsNullOrEmpty(label))
                {
                    continue;
                }

                var size = g.MeasureString(label, TickFont);
                if (size.Width > maxWidth)
                {
                    maxWidth = size.Width;
                }

                if (size.Height > maxHeight)
                {
                    maxHeight = size.Height;
                }
            }

            return new SizeF(maxWidth, maxHeight);
        }

        // ── Series rendering ──────────────────────────────────────────────────

        private static void RenderSeries(
            Graphics g,
            RectangleF plotRect,
            GraphPresentationModel model)
        {
            var xAxis = FindHorizontalAxis(model);
            var verticalAxisEntries = FindVerticalAxisEntries(model);

            if (xAxis == null || verticalAxisEntries.Count == 0)
            {
                return;
            }

            if (!xAxis.MinimumValue.HasValue || !xAxis.MaximumValue.HasValue)
            {
                return;
            }

            double xMin = xAxis.MinimumValue.Value;
            double xMax = xAxis.MaximumValue.Value;

            if (xMin >= xMax)
            {
                return;
            }

            var series = model.Layout.Series;
            for (var i = 0; i < series.Count; i++)
            {
                var axisEntry = ResolveSeriesVerticalAxisEntry(series[i], verticalAxisEntries);
                if (axisEntry == null)
                {
                    continue;
                }

                var yAxis = axisEntry.Axis;
                if (!yAxis.MinimumValue.HasValue || !yAxis.MaximumValue.HasValue)
                {
                    continue;
                }

                var yMin = yAxis.MinimumValue.Value;
                var yMax = yAxis.MaximumValue.Value;
                if (yMin >= yMax)
                {
                    continue;
                }

                var seriesRect = ComputeAxisRect(plotRect, axisEntry);
                RenderOneSeries(g, seriesRect, series[i], xMin, xMax, yMin, yMax);
            }
        }

        private static void RenderOneSeries(
            Graphics g,
            RectangleF plotRect,
            SeriesPresentationGeometry series,
            double xMin, double xMax,
            double yMin, double yMax)
        {
            var points = series.Points;
            if (points == null || points.Count < 2)
            {
                return;
            }

            // Use a clipping region to keep lines inside the plot area.
            var clip = g.ClipBounds;
            g.SetClip(plotRect, System.Drawing.Drawing2D.CombineMode.Intersect);

            try
            {
                PointF? previous = null;

                for (var i = 0; i < points.Count; i++)
                {
                    var domainPoint = points[i];
                    var deviceX = DomainToDeviceX(domainPoint.X, xMin, xMax, plotRect);
                    var deviceY = DomainToDeviceY(domainPoint.Y, yMin, yMax, plotRect);
                    var current = new PointF(deviceX, deviceY);

                    if (previous.HasValue && series.ConnectivityIntent != SeriesConnectivityIntent.Discrete)
                    {
                        g.DrawLine(SeriesPen, previous.Value, current);
                    }

                    previous = current;
                }
            }
            finally
            {
                g.SetClip(clip);
            }
        }

        // ── Axis lookup helpers ───────────────────────────────────────────────

        private static AxisPresentationGeometry FindHorizontalAxis(GraphPresentationModel model)
        {
            var axisEntries = model.Layout.Axes;
            for (var i = 0; i < axisEntries.Count; i++)
            {
                if (axisEntries[i].Axis.Orientation == AxisOrientation.Horizontal)
                {
                    return axisEntries[i].Axis;
                }
            }
            return null;
        }

        private static List<AxisLayoutEntry> FindVerticalAxisEntries(GraphPresentationModel model)
        {
            var entries = new List<AxisLayoutEntry>();
            var axisEntries = model.Layout.Axes;

            for (var i = 0; i < axisEntries.Count; i++)
            {
                if (axisEntries[i].Axis.Orientation == AxisOrientation.Vertical)
                {
                    entries.Add(axisEntries[i]);
                }
            }

            return entries;
        }

        private static AxisLayoutEntry ResolveSeriesVerticalAxisEntry(
            SeriesPresentationGeometry series,
            IReadOnlyList<AxisLayoutEntry> verticalAxisEntries)
        {
            if (series == null || verticalAxisEntries == null || verticalAxisEntries.Count == 0)
            {
                return null;
            }

            var points = series.Points;
            if (points == null || points.Count == 0)
            {
                return verticalAxisEntries[0];
            }

            double? seriesMin = null;
            double? seriesMax = null;
            for (var i = 0; i < points.Count; i++)
            {
                var y = points[i].Y;
                if (!seriesMin.HasValue || y < seriesMin.Value)
                {
                    seriesMin = y;
                }

                if (!seriesMax.HasValue || y > seriesMax.Value)
                {
                    seriesMax = y;
                }
            }

            if (!seriesMin.HasValue || !seriesMax.HasValue)
            {
                return verticalAxisEntries[0];
            }

            const double Epsilon = 1e-12;
            AxisLayoutEntry bestContaining = null;
            double bestRange = double.MaxValue;

            for (var i = 0; i < verticalAxisEntries.Count; i++)
            {
                var candidate = verticalAxisEntries[i];
                var axis = candidate.Axis;
                if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
                {
                    continue;
                }

                var axisMin = axis.MinimumValue.Value;
                var axisMax = axis.MaximumValue.Value;
                if (axisMin > seriesMin.Value + Epsilon || axisMax < seriesMax.Value - Epsilon)
                {
                    continue;
                }

                var range = axisMax - axisMin;
                if (range < bestRange)
                {
                    bestRange = range;
                    bestContaining = candidate;
                }
            }

            if (bestContaining != null)
            {
                return bestContaining;
            }

            AxisLayoutEntry closest = verticalAxisEntries[0];
            double closestDistance = double.MaxValue;

            for (var i = 0; i < verticalAxisEntries.Count; i++)
            {
                var candidate = verticalAxisEntries[i];
                var axis = candidate.Axis;
                if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
                {
                    continue;
                }

                var axisMin = axis.MinimumValue.Value;
                var axisMax = axis.MaximumValue.Value;
                var underflow = axisMin - seriesMin.Value;
                var overflow = seriesMax.Value - axisMax;
                var distance = Math.Max(0d, underflow) + Math.Max(0d, overflow);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = candidate;
                }
            }

            return closest;
        }

        private static RectangleF ComputeAxisRect(RectangleF plotRect, AxisLayoutEntry entry)
        {
            if (entry == null || entry.Side != AxisSide.Left)
            {
                return plotRect;
            }

            var spanStart = Clamp01(entry.NormalizedSpanStart);
            var spanEnd = Clamp01(entry.NormalizedSpanEnd);
            if (spanEnd <= spanStart)
            {
                return plotRect;
            }

            var plotHeight = plotRect.Height;
            var top = plotRect.Bottom - (float)(spanEnd * plotHeight);
            var bottom = plotRect.Bottom - (float)(spanStart * plotHeight);
            return RectangleF.FromLTRB(plotRect.Left, top, plotRect.Right, bottom);
        }

        private static double Clamp01(double value)
        {
            if (value < 0d)
            {
                return 0d;
            }

            if (value > 1d)
            {
                return 1d;
            }

            return value;
        }

        // ── Padding ───────────────────────────────────────────────────────────

        private struct LabelPadding
        {
            internal float Top;
            internal float Right;
            internal float Bottom;
            internal float Left;
        }

        /// <summary>
        /// Measures renderer-only padding required to keep tick labels and axis titles visible.
        /// </summary>
        private static LabelPadding MeasureLabelPadding(Graphics g, GraphPresentationModel model)
        {
            var lineHeight = TickFont.GetHeight(g);
            var titleLineHeight = AxisTitleFont.GetHeight(g);
            var padding = new LabelPadding
            {
                Top = lineHeight / 2f + TickLabelOffset,
                Right = lineHeight + TickLabelOffset,
                Bottom = lineHeight + TickLabelOffset,
                Left = lineHeight + TickLabelOffset,
            };

            var axisEntries = model.Layout.Axes;
            for (var i = 0; i < axisEntries.Count; i++)
            {
                var entry = axisEntries[i];
                var axis = entry.Axis;
                var ticks = axis.Ticks;
                var tickLabelSize = MeasureMaxTickLabelSize(g, axis);
                var hasTitle = !string.IsNullOrWhiteSpace(axis.Title);
                var titleHeightAllowance = hasTitle ? titleLineHeight + AxisTitleOffset : 0f;
                var titleWidthAllowance = hasTitle ? titleLineHeight + AxisTitleOffset : 0f;

                if (entry.Side == AxisSide.Bottom || entry.Side == AxisSide.Top)
                {
                    for (var t = 0; t < ticks.Count; t++)
                    {
                        if (string.IsNullOrEmpty(ticks[t].Label))
                        {
                            continue;
                        }

                        var sz = g.MeasureString(ticks[t].Label, TickFont);
                        var halfWidth = sz.Width / 2f + TickLabelOffset;
                        if (halfWidth > padding.Right)
                        {
                            padding.Right = halfWidth;
                        }
                    }
                }

                switch (entry.Side)
                {
                    case AxisSide.Bottom:
                        padding.Bottom = Math.Max(
                            padding.Bottom,
                            TickLength + TickLabelOffset + tickLabelSize.Height + titleHeightAllowance);
                        break;

                    case AxisSide.Top:
                        padding.Top = Math.Max(
                            padding.Top,
                            TickLength + TickLabelOffset + tickLabelSize.Height + titleHeightAllowance);
                        break;

                    case AxisSide.Left:
                        padding.Left = Math.Max(
                            padding.Left,
                            TickLength + TickLabelOffset + tickLabelSize.Width + titleWidthAllowance);
                        padding.Top = Math.Max(padding.Top, lineHeight / 2f + TickLabelOffset);
                        break;

                    case AxisSide.Right:
                        padding.Right = Math.Max(
                            padding.Right,
                            TickLength + TickLabelOffset + tickLabelSize.Width + titleWidthAllowance);
                        padding.Top = Math.Max(padding.Top, lineHeight / 2f + TickLabelOffset);
                        break;
                }
            }

            return padding;
        }

        /// <summary>
        /// Shrinks <paramref name="deviceBounds"/> by the given <paramref name="padding"/>,
        /// reserving edge space for labels and titles drawn outside the plot area.
        /// </summary>
        private static Rectangle ApplyPadding(Rectangle deviceBounds, LabelPadding padding)
        {
            var top = deviceBounds.Top + (int)Math.Ceiling(padding.Top);
            var right = deviceBounds.Right - (int)Math.Ceiling(padding.Right);
            var bottom = deviceBounds.Bottom - (int)Math.Ceiling(padding.Bottom);
            var left = deviceBounds.Left + (int)Math.Ceiling(padding.Left);

            if (right <= left || bottom <= top)
            {
                return deviceBounds;
            }

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        // ── Coordinate transforms ─────────────────────────────────────────────

        /// <summary>
        /// Maps the abstract normalized plot area [0,1]×[0,1] to device pixel bounds.
        /// Abstract space uses math orientation (Y up); device space uses screen orientation (Y down).
        /// </summary>
        private static RectangleF ComputeDevicePlotRect(Rectangle deviceBounds, PlotAreaLayout plotArea)
        {
            var left   = deviceBounds.Left   + plotArea.BottomLeft.X * deviceBounds.Width;
            var right  = deviceBounds.Left   + plotArea.TopRight.X   * deviceBounds.Width;
            var top    = deviceBounds.Bottom - plotArea.TopRight.Y   * deviceBounds.Height;
            var bottom = deviceBounds.Bottom - plotArea.BottomLeft.Y * deviceBounds.Height;

            return RectangleF.FromLTRB((float)left, (float)top, (float)right, (float)bottom);
        }

        /// <summary>
        /// Maps a domain X value to a device X coordinate within the plot area.
        /// </summary>
        private static float DomainToDeviceX(
            double domainValue, double domainMin, double domainMax, RectangleF plotRect)
        {
            var range = domainMax - domainMin;
            if (Math.Abs(range) < double.Epsilon)
            {
                return plotRect.Left + plotRect.Width / 2f;
            }
            var t = (domainValue - domainMin) / range;
            return plotRect.Left + (float)(t * plotRect.Width);
        }

        /// <summary>
        /// Maps a domain Y value to a device Y coordinate within the plot area.
        /// Device Y is inverted relative to domain Y (screen origin at top-left).
        /// </summary>
        private static float DomainToDeviceY(
            double domainValue, double domainMin, double domainMax, RectangleF plotRect)
        {
            var range = domainMax - domainMin;
            if (Math.Abs(range) < double.Epsilon)
            {
                return plotRect.Top + plotRect.Height / 2f;
            }
            var t = (domainValue - domainMin) / range;
            return plotRect.Bottom - (float)(t * plotRect.Height);
        }
    }
}
