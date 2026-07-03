using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using Graphing.Controls.Rendering.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfColor = System.Windows.Media.Color;
using WpfPen = System.Windows.Media.Pen;
using WpfPoint = System.Windows.Point;
using WpfRect = System.Windows.Rect;
using WpfSize = System.Windows.Size;

namespace Graphing.Controls.WPF.Rendering
{
    internal sealed class WpfGraphRenderer : GraphRendererBase
    {
        private const int TickLength = 5;
        private const int TickLabelOffset = 3;
        private const int AxisTitleOffset = 6;
        private const double AxisLineWidth = 1d;
        private const double SeriesLineWidth = 2d;
        private const double GridLineWidth = 0.5d;
        private const double LegendLineWidth = 1d;
        private const double LegendTextOffset = 6d;
        private const double LegendMarkerSize = 4d;
        private const double LegendMarkerRadius = LegendMarkerSize / 2d;
        private const double LegendGlyphSampleWidth = 18d;
        private const double LegendOuterPaddingPixels = 4d;
        private const double LegendInnerPaddingPixels = 7d;
        private const double LegendEntryGapPixels = 4d;
        private const double LegendMeasurementSafetyMarginPixels = 8d;
        private const double DiscretePointMarkerSize = 4d;
        private const double DiscretePointMarkerRadius = DiscretePointMarkerSize / 2d;

        private static readonly double TickFontSize = 7d;
        private static readonly double AxisTitleFontSize = 8d;
        private static readonly double GraphTitleFontSize = 12d;
        private static readonly double GraphSubtitleFontSize = 10d;
        private static readonly double LegendFontSize = 8d;
        private static readonly string FontFamily = "Arial";

        private static readonly WpfPen AxisPen = MakePen(WpfBrushes.Black, AxisLineWidth);
        private static readonly WpfPen PlotAreaBorderPen = MakePen(WpfBrushes.Black, AxisLineWidth);
        private static readonly WpfPen OuterGraphBorderPen = MakePen(WpfBrushes.Black, AxisLineWidth);
        private static readonly WpfPen GridLinesPen = MakePen(new SolidColorBrush(WpfColor.FromRgb(0xD3, 0xD3, 0xD3)), GridLineWidth);
        private static readonly WpfPen LegendBorderPen = MakePen(WpfBrushes.Black, LegendLineWidth);
        private static readonly SolidColorBrush TextBrush = new SolidColorBrush(WpfColor.FromRgb(0, 0, 0));

        static WpfGraphRenderer()
        {
            AxisPen.Freeze();
            PlotAreaBorderPen.Freeze();
            OuterGraphBorderPen.Freeze();
            GridLinesPen.Freeze();
            LegendBorderPen.Freeze();
            TextBrush.Freeze();
        }

        protected override IGraphDrawingSurface TryCreateDrawingSurface(IGraphRenderContext context, Rectangle deviceBounds)
        {
            var dc = (context as WpfGraphRenderContext)?.DrawingContext;
            if (dc == null)
            {
                return null;
            }

            return new WpfGraphDrawingSurface(dc, deviceBounds);
        }

        protected override IGraphLayoutMeasurementInput CreateMeasurementInputCore(IGraphRenderContext context, Rectangle deviceBounds)
        {
            return new WpfLayoutMeasurementInput(deviceBounds);
        }

        protected override void RenderCore(IGraphDrawingSurface surface, RectangleF plotRect, GraphPresentationModel model, GraphPresentationOptions options)
        {
            var s = (WpfGraphDrawingSurface)surface;
            var dc = s.DrawingContext;
            var deviceBounds = s.DeviceBounds;

            RenderBackground(dc, deviceBounds);
            RenderOuterGraphBorder(dc, deviceBounds, options);
            RenderGridLines(dc, plotRect, model.Layout.GridLines, model);
            RenderAxes(dc, plotRect, deviceBounds, model);
            RenderPlotAreaBorder(dc, plotRect);
            RenderSeries(dc, plotRect, model);
            RenderAxisTitles(dc, deviceBounds, model.Layout.AxisTitleBands);
            RenderTitles(dc, deviceBounds, model.Layout);
            RenderLegend(dc, deviceBounds, model.Layout.Legend);
        }

        // ── Background ────────────────────────────────────────────────────────

        private static void RenderBackground(DrawingContext dc, Rectangle deviceBounds)
        {
            dc.DrawRectangle(WpfBrushes.White, null, ToWpfRect(deviceBounds));
        }

        // ── Border rendering ──────────────────────────────────────────────────

        private static void RenderOuterGraphBorder(DrawingContext dc, Rectangle deviceBounds, GraphPresentationOptions options)
        {
            if (!(options?.ShowGraphBorder ?? true))
            {
                return;
            }

            var r = new WpfRect(deviceBounds.X + 0.5, deviceBounds.Y + 0.5, deviceBounds.Width - 1, deviceBounds.Height - 1);
            dc.DrawRectangle(null, OuterGraphBorderPen, r);
        }

        private static void RenderPlotAreaBorder(DrawingContext dc, RectangleF plotRect)
        {
            dc.DrawRectangle(null, PlotAreaBorderPen, ToWpfRect(plotRect));
        }

        // ── Grid lines ────────────────────────────────────────────────────────

        private static void RenderGridLines(DrawingContext dc, RectangleF plotRect, GridLinesGeometry gridLines, GraphPresentationModel model)
        {
            if (gridLines == null || model == null)
            {
                return;
            }

            dc.PushClip(new RectangleGeometry(ToWpfRect(plotRect)));
            try
            {
                var vertical = gridLines.VerticalLines;
                for (var i = 0; i < vertical.Count; i++)
                {
                    var line = vertical[i];
                    if (line.AxisEntry == null)
                    {
                        continue;
                    }

                    var xAxis = line.AxisEntry.Axis;
                    if (!xAxis.MinimumValue.HasValue || !xAxis.MaximumValue.HasValue)
                    {
                        continue;
                    }

                    var axisRect = ComputeAxisRect(plotRect, line.AxisEntry);
                    var deviceX = DomainToDeviceX(line.Start.X, xAxis.MinimumValue.Value, xAxis.MaximumValue.Value, axisRect);
                    dc.DrawLine(GridLinesPen,
                        new WpfPoint(deviceX, plotRect.Top),
                        new WpfPoint(deviceX, plotRect.Bottom));
                }

                var horizontal = gridLines.HorizontalLines;
                for (var i = 0; i < horizontal.Count; i++)
                {
                    var line = horizontal[i];
                    if (line.AxisEntry == null)
                    {
                        continue;
                    }

                    var yAxis = line.AxisEntry.Axis;
                    if (!yAxis.MinimumValue.HasValue || !yAxis.MaximumValue.HasValue)
                    {
                        continue;
                    }

                    var xMin = double.MaxValue;
                    var xMax = double.MinValue;
                    var axes = model.Layout.Axes;
                    for (var j = 0; j < axes.Count; j++)
                    {
                        var e = axes[j];
                        if (e.Axis.Orientation == AxisOrientation.Horizontal && e.Axis.MinimumValue.HasValue && e.Axis.MaximumValue.HasValue)
                        {
                            xMin = Math.Min(xMin, e.Axis.MinimumValue.Value);
                            xMax = Math.Max(xMax, e.Axis.MaximumValue.Value);
                        }
                    }

                    if (xMin >= xMax)
                    {
                        continue;
                    }

                    var axisRect = ComputeAxisRect(plotRect, line.AxisEntry);
                    var deviceY = DomainToDeviceY(line.Start.Y, yAxis.MinimumValue.Value, yAxis.MaximumValue.Value, axisRect);
                    dc.DrawLine(GridLinesPen,
                        new WpfPoint(plotRect.Left, deviceY),
                        new WpfPoint(plotRect.Right, deviceY));
                }
            }
            finally
            {
                dc.Pop();
            }
        }

        // ── Axis rendering ────────────────────────────────────────────────────

        private static void RenderAxes(DrawingContext dc, RectangleF plotRect, Rectangle deviceBounds, GraphPresentationModel model)
        {
            var entries = model.Layout.Axes;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var axis = entry.Axis;
                if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
                {
                    continue;
                }

                var axisRect = ComputeAxisRect(plotRect, entry);
                var bandRect = FindAxisTickLabelRegionRect(deviceBounds, model.Layout.AxisTitleBands, entry.Side);

                switch (entry.Side)
                {
                    case AxisSide.Bottom:
                        RenderBottomAxis(dc, axisRect, bandRect, axis);
                        break;
                    case AxisSide.Left:
                        RenderLeftAxis(dc, axisRect, bandRect, axis);
                        break;
                    case AxisSide.Right:
                        RenderRightAxis(dc, axisRect, bandRect, axis);
                        break;
                    case AxisSide.Top:
                        RenderTopAxis(dc, axisRect, bandRect, axis);
                        break;
                }
            }
        }

        private static void RenderBottomAxis(DrawingContext dc, RectangleF plotRect, WpfRect? bandRect, AxisPresentationGeometry axis)
        {
            var domainMin = axis.MinimumValue.Value;
            var domainMax = axis.MaximumValue.Value;
            var axisY = plotRect.Bottom;
            dc.DrawLine(AxisPen, new WpfPoint(plotRect.Left, axisY), new WpfPoint(plotRect.Right, axisY));

            var step = ComputeHorizontalTickStep(axis.Ticks, bandRect);
            var ticks = axis.Ticks;

            if (bandRect.HasValue)
            {
                dc.PushClip(new RectangleGeometry(new WpfRect(
                    plotRect.Left - 1000.0,
                    bandRect.Value.Top,
                    plotRect.Width + 1000.0,
                    bandRect.Value.Height)));
            }

            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var start = MapHorizontalTickPoint(tick.Start, domainMin, domainMax, plotRect, axisY);
                    var end = MapHorizontalTickPoint(tick.End, domainMin, domainMax, plotRect, axisY);
                    dc.DrawLine(AxisPen, start, end);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, step) && i != ticks.Count - 1)
                    {
                        var ft = MakeFormattedText(tick.Label, TickFontSize);
                        var deviceX = DomainToDeviceX(tick.Value, domainMin, domainMax, plotRect);
                        var x = deviceX - ft.Width / 2d;
                        var y = Math.Max(start.Y, end.Y) + TickLabelOffset;
                        dc.DrawText(ft, new WpfPoint(x, y));
                    }
                }
            }
            finally
            {
                if (bandRect.HasValue)
                {
                    dc.Pop();
                }
            }
        }

        private static void RenderLeftAxis(DrawingContext dc, RectangleF plotRect, WpfRect? bandRect, AxisPresentationGeometry axis)
        {
            var domainMin = axis.MinimumValue.Value;
            var domainMax = axis.MaximumValue.Value;
            var axisX = plotRect.Left;
            dc.DrawLine(AxisPen, new WpfPoint(axisX, plotRect.Top), new WpfPoint(axisX, plotRect.Bottom));

            if (bandRect.HasValue)
            {
                dc.PushClip(new RectangleGeometry(bandRect.Value));
            }

            var ticks = axis.Ticks;
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var start = MapVerticalTickPoint(tick.Start, domainMin, domainMax, plotRect, axisX);
                    var end = MapVerticalTickPoint(tick.End, domainMin, domainMax, plotRect, axisX);
                    dc.DrawLine(AxisPen, start, end);

                    if (!string.IsNullOrEmpty(tick.Label))
                    {
                        var ft = MakeFormattedText(tick.Label, TickFontSize);
                        var deviceY = DomainToDeviceY(tick.Value, domainMin, domainMax, plotRect);
                        var x = Math.Min(start.X, end.X) - TickLabelOffset - ft.Width;
                        var y = deviceY - ft.Height / 2d;
                        dc.DrawText(ft, new WpfPoint(x, y));
                    }
                }
            }
            finally
            {
                if (bandRect.HasValue)
                {
                    dc.Pop();
                }
            }
        }

        private static void RenderRightAxis(DrawingContext dc, RectangleF plotRect, WpfRect? bandRect, AxisPresentationGeometry axis)
        {
            var domainMin = axis.MinimumValue.Value;
            var domainMax = axis.MaximumValue.Value;
            var axisX = plotRect.Right;
            dc.DrawLine(AxisPen, new WpfPoint(axisX, plotRect.Top), new WpfPoint(axisX, plotRect.Bottom));

            if (bandRect.HasValue)
            {
                dc.PushClip(new RectangleGeometry(bandRect.Value));
            }

            var ticks = axis.Ticks;
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var start = MapVerticalTickPoint(tick.Start, domainMin, domainMax, plotRect, axisX);
                    var end = MapVerticalTickPoint(tick.End, domainMin, domainMax, plotRect, axisX);
                    dc.DrawLine(AxisPen, start, end);

                    if (!string.IsNullOrEmpty(tick.Label))
                    {
                        var ft = MakeFormattedText(tick.Label, TickFontSize);
                        var deviceY = DomainToDeviceY(tick.Value, domainMin, domainMax, plotRect);
                        var x = Math.Max(start.X, end.X) + TickLabelOffset;
                        var y = deviceY - ft.Height / 2d;
                        dc.DrawText(ft, new WpfPoint(x, y));
                    }
                }
            }
            finally
            {
                if (bandRect.HasValue)
                {
                    dc.Pop();
                }
            }
        }

        private static void RenderTopAxis(DrawingContext dc, RectangleF plotRect, WpfRect? bandRect, AxisPresentationGeometry axis)
        {
            var domainMin = axis.MinimumValue.Value;
            var domainMax = axis.MaximumValue.Value;
            var axisY = plotRect.Top;
            dc.DrawLine(AxisPen, new WpfPoint(plotRect.Left, axisY), new WpfPoint(plotRect.Right, axisY));

            var step = ComputeHorizontalTickStep(axis.Ticks, bandRect);
            var ticks = axis.Ticks;

            if (bandRect.HasValue)
            {
                dc.PushClip(new RectangleGeometry(bandRect.Value));
            }

            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var start = MapHorizontalTickPoint(tick.Start, domainMin, domainMax, plotRect, axisY);
                    var end = MapHorizontalTickPoint(tick.End, domainMin, domainMax, plotRect, axisY);
                    dc.DrawLine(AxisPen, start, end);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, step) && i != ticks.Count - 1)
                    {
                        var ft = MakeFormattedText(tick.Label, TickFontSize);
                        var deviceX = DomainToDeviceX(tick.Value, domainMin, domainMax, plotRect);
                        var x = deviceX - ft.Width / 2d;
                        var y = Math.Min(start.Y, end.Y) - TickLabelOffset - ft.Height;
                        dc.DrawText(ft, new WpfPoint(x, y));
                    }
                }
            }
            finally
            {
                if (bandRect.HasValue)
                {
                    dc.Pop();
                }
            }
        }

        // ── Axis titles ───────────────────────────────────────────────────────

        private static void RenderAxisTitles(DrawingContext dc, Rectangle deviceBounds, IReadOnlyList<AxisTitleBandGeometry> bands)
        {
            if (bands == null)
            {
                return;
            }

            for (var bi = 0; bi < bands.Count; bi++)
            {
                var items = bands[bi].Items;
                for (var ii = 0; ii < items.Count; ii++)
                {
                    var item = items[ii];
                    if (string.IsNullOrWhiteSpace(item.Title))
                    {
                        continue;
                    }

                    var itemRect = ComputeDeviceBoundsForGeometry(deviceBounds, item.AxisTitleRegion.BottomLeft, item.AxisTitleRegion.TopRight);
                    if (itemRect.Width <= 0 || itemRect.Height <= 0)
                    {
                        continue;
                    }

                    var ft = MakeFormattedText(item.Title, AxisTitleFontSize, bold: true);
                    var centerX = itemRect.Left + itemRect.Width / 2d;
                    var centerY = itemRect.Top + itemRect.Height / 2d;

                    if (item.Side == AxisSide.Left)
                    {
                        DrawRotatedCenteredText(dc, ft, centerX, centerY, -90d);
                    }
                    else if (item.Side == AxisSide.Right)
                    {
                        DrawRotatedCenteredText(dc, ft, centerX, centerY, 90d);
                    }
                    else
                    {
                        dc.DrawText(ft, new WpfPoint(centerX - ft.Width / 2d, centerY - ft.Height / 2d));
                    }
                }
            }
        }

        // ── Graph titles ──────────────────────────────────────────────────────

        private static void RenderTitles(DrawingContext dc, Rectangle deviceBounds, GraphLayoutModel layout)
        {
            if (layout?.Title != null)
            {
                var r = ComputeDeviceBoundsForGeometry(deviceBounds, layout.Title.BottomLeft, layout.Title.TopRight);
                if (r.Width > 0 && r.Height > 0)
                {
                    var ft = MakeFormattedText(layout.Title.Text, GraphTitleFontSize, bold: true);
                    dc.DrawText(ft, new WpfPoint(r.Left + (r.Width - ft.Width) / 2d, r.Top + (r.Height - ft.Height) / 2d));
                }
            }

            if (layout?.Subtitle != null)
            {
                var r = ComputeDeviceBoundsForGeometry(deviceBounds, layout.Subtitle.BottomLeft, layout.Subtitle.TopRight);
                if (r.Width > 0 && r.Height > 0)
                {
                    var ft = MakeFormattedText(layout.Subtitle.Text, GraphSubtitleFontSize);
                    dc.DrawText(ft, new WpfPoint(r.Left + (r.Width - ft.Width) / 2d, r.Top + (r.Height - ft.Height) / 2d));
                }
            }
        }

        // ── Legend ────────────────────────────────────────────────────────────

        private static void RenderLegend(DrawingContext dc, Rectangle deviceBounds, LegendPresentationGeometry legend)
        {
            if (legend == null)
            {
                return;
            }

            var legendRect = ComputeDeviceBoundsForGeometry(deviceBounds, legend.BottomLeft, legend.TopRight);
            if (legendRect.Width <= 0 || legendRect.Height <= 0)
            {
                return;
            }

            if (legend.ShowBorder)
            {
                dc.DrawRectangle(null, LegendBorderPen, legendRect);
            }

            var contentRect = ComputeDeviceBoundsForGeometry(deviceBounds, legend.ContentBottomLeft, legend.ContentTopRight);
            dc.PushClip(new RectangleGeometry(contentRect));
            try
            {
                var entries = legend.Entries;
                for (var i = 0; i < entries.Count; i++)
                {
                    RenderLegendEntry(dc, deviceBounds, entries[i]);
                }
            }
            finally
            {
                dc.Pop();
            }
        }

        private static void RenderLegendEntry(DrawingContext dc, Rectangle deviceBounds, LegendEntryPresentationGeometry entry)
        {
            if (entry == null)
            {
                return;
            }

            var entryRect = ComputeDeviceBoundsForGeometry(deviceBounds, entry.BottomLeft, entry.TopRight);
            if (entryRect.Width <= 0 || entryRect.Height <= 0)
            {
                return;
            }

            var glyphRect = ComputeDeviceBoundsForGeometry(deviceBounds, entry.GlyphBottomLeft, entry.GlyphTopRight);
            var glyphLeft = Math.Max(entryRect.Left, glyphRect.Left);
            var glyphWidth = Math.Min(LegendGlyphSampleWidth, Math.Max(0d, entryRect.Right - glyphLeft));
            var glyphRight = glyphLeft + glyphWidth;
            var glyphCenterY = glyphRect.Top + glyphRect.Height / 2d;

            var seriesColor = ToWpfColor(entry.SeriesColor);
            var glyphBrush = new SolidColorBrush(seriesColor);
            var glyphPen = new WpfPen(glyphBrush, LegendLineWidth);

            if (entry.GlyphKind == LegendGlyphKind.Line || entry.GlyphKind == LegendGlyphKind.LineAndPoint)
            {
                dc.DrawLine(glyphPen, new WpfPoint(glyphLeft, glyphCenterY), new WpfPoint(glyphRight, glyphCenterY));
            }

            if (entry.GlyphKind == LegendGlyphKind.Point || entry.GlyphKind == LegendGlyphKind.LineAndPoint)
            {
                var markerCenterX = glyphLeft + glyphWidth / 2d;
                dc.DrawEllipse(null, glyphPen, new WpfPoint(markerCenterX, glyphCenterY), LegendMarkerRadius, LegendMarkerRadius);
            }

            if (!string.IsNullOrWhiteSpace(entry.DisplayText))
            {
                var ft = MakeFormattedText(entry.DisplayText, LegendFontSize);
                var textX = glyphRight + LegendTextOffset;
                var textY = entryRect.Top + (entryRect.Height - ft.Height) / 2d;
                dc.DrawText(ft, new WpfPoint(textX, textY));
            }
        }

        // ── Series ────────────────────────────────────────────────────────────

        private static void RenderSeries(DrawingContext dc, RectangleF plotRect, GraphPresentationModel model)
        {
            var series = model.Layout.Series;
            for (var i = 0; i < series.Count; i++)
            {
                var s = series[i];
                if (s.XAxisEntry == null || s.YAxisEntry == null)
                {
                    continue;
                }

                var xAxis = s.XAxisEntry.Axis;
                var yAxis = s.YAxisEntry.Axis;
                if (!xAxis.MinimumValue.HasValue || !xAxis.MaximumValue.HasValue ||
                    !yAxis.MinimumValue.HasValue || !yAxis.MaximumValue.HasValue)
                {
                    continue;
                }

                var xMin = xAxis.MinimumValue.Value;
                var xMax = xAxis.MaximumValue.Value;
                var yMin = yAxis.MinimumValue.Value;
                var yMax = yAxis.MaximumValue.Value;
                if (xMin >= xMax || yMin >= yMax)
                {
                    continue;
                }

                var seriesRect = ComputeSeriesRect(plotRect, s.XAxisEntry, s.YAxisEntry);
                RenderOneSeries(dc, seriesRect, s, xMin, xMax, yMin, yMax);
            }
        }

        private static void RenderOneSeries(DrawingContext dc, RectangleF seriesRect, SeriesPresentationGeometry series,
            double xMin, double xMax, double yMin, double yMax)
        {
            var points = series.Points;
            if (points == null || points.Count < 1)
            {
                return;
            }

            var color = ToWpfColor(series.SeriesColor);
            var brush = new SolidColorBrush(color);
            var pen = new WpfPen(brush, SeriesLineWidth);

            dc.PushClip(new RectangleGeometry(ToWpfRect(seriesRect)));
            try
            {
                if (series.ConnectivityIntent == SeriesConnectivityIntent.Discrete)
                {
                    for (var i = 0; i < points.Count; i++)
                    {
                        var px = DomainToDeviceX(points[i].X, xMin, xMax, seriesRect);
                        var py = DomainToDeviceY(points[i].Y, yMin, yMax, seriesRect);
                        dc.DrawEllipse(brush, null,
                            new WpfPoint(px, py), DiscretePointMarkerRadius, DiscretePointMarkerRadius);
                    }
                }
                else
                {
                    WpfPoint? prev = null;
                    for (var i = 0; i < points.Count; i++)
                    {
                        var px = DomainToDeviceX(points[i].X, xMin, xMax, seriesRect);
                        var py = DomainToDeviceY(points[i].Y, yMin, yMax, seriesRect);
                        var curr = new WpfPoint(px, py);
                        if (prev.HasValue)
                        {
                            dc.DrawLine(pen, prev.Value, curr);
                        }

                        prev = curr;
                    }
                }
            }
            finally
            {
                dc.Pop();
            }
        }

        // ── Layout helpers (mirrored from WinForms renderer) ──────────────────

        private static RectangleF ComputeAxisRect(RectangleF plotRect, AxisLayoutEntry entry)
        {
            if (entry == null)
            {
                return plotRect;
            }

            var inset = ApplyAxisInset(plotRect, entry);
            if (entry.Side != AxisSide.Left)
            {
                return inset;
            }

            var spanStart = Clamp01(entry.NormalizedSpanStart);
            var spanEnd = Clamp01(entry.NormalizedSpanEnd);
            if (spanEnd <= spanStart)
            {
                return inset;
            }

            var plotHeight = inset.Height;
            var top = (float)(inset.Bottom - spanEnd * plotHeight);
            var bottom = (float)(inset.Bottom - spanStart * plotHeight);
            return RectangleF.FromLTRB(inset.Left, top, inset.Right, bottom);
        }

        private static RectangleF ComputeSeriesRect(RectangleF plotRect, AxisLayoutEntry xEntry, AxisLayoutEntry yEntry)
        {
            var rect = plotRect;
            if (xEntry != null)
            {
                rect = ApplyAxisInset(rect, xEntry);
            }

            if (yEntry != null)
            {
                rect = ComputeAxisRect(rect, yEntry);
            }

            return rect;
        }

        private static RectangleF ApplyAxisInset(RectangleF plotRect, AxisLayoutEntry entry)
        {
            if (entry == null)
            {
                return plotRect;
            }

            var inset = Math.Min(0.49d, Clamp01(entry.TickEndpointInset));
            if (inset <= 0d)
            {
                return plotRect;
            }

            if (entry.Side == AxisSide.Left || entry.Side == AxisSide.Right)
            {
                var delta = (float)(inset * plotRect.Height);
                var top = plotRect.Top + delta;
                var bottom = plotRect.Bottom - delta;
                return bottom > top ? RectangleF.FromLTRB(plotRect.Left, top, plotRect.Right, bottom) : plotRect;
            }

            var hDelta = (float)(inset * plotRect.Width);
            var left = plotRect.Left + hDelta;
            var right = plotRect.Right - hDelta;
            return right > left ? RectangleF.FromLTRB(left, plotRect.Top, right, plotRect.Bottom) : plotRect;
        }

        private static WpfRect? FindAxisTickLabelRegionRect(Rectangle deviceBounds, IReadOnlyList<AxisTitleBandGeometry> bands, AxisSide side)
        {
            if (bands == null)
            {
                return null;
            }

            for (var i = 0; i < bands.Count; i++)
            {
                if (bands[i].Side != side)
                {
                    continue;
                }

                return ComputeDeviceBoundsForGeometry(deviceBounds, bands[i].AxisTickLabelRegion.BottomLeft, bands[i].AxisTickLabelRegion.TopRight);
            }

            return null;
        }

        private static WpfRect ComputeDeviceBoundsForGeometry(Rectangle deviceBounds, GeometryPoint3D bottomLeft, GeometryPoint3D topRight)
        {
            var left = deviceBounds.Left + bottomLeft.X * deviceBounds.Width;
            var right = deviceBounds.Left + topRight.X * deviceBounds.Width;
            var top = deviceBounds.Bottom - topRight.Y * deviceBounds.Height;
            var bottom = deviceBounds.Bottom - bottomLeft.Y * deviceBounds.Height;
            return new WpfRect(left, top, Math.Max(0d, right - left), Math.Max(0d, bottom - top));
        }

        // ── Coordinate mapping ────────────────────────────────────────────────

        private static double DomainToDeviceX(double value, double domainMin, double domainMax, RectangleF rect)
        {
            if (domainMax <= domainMin)
            {
                return rect.Left;
            }

            var t = (value - domainMin) / (domainMax - domainMin);
            return rect.Left + t * rect.Width;
        }

        private static double DomainToDeviceY(double value, double domainMin, double domainMax, RectangleF rect)
        {
            if (domainMax <= domainMin)
            {
                return rect.Bottom;
            }

            var t = (value - domainMin) / (domainMax - domainMin);
            return rect.Bottom - t * rect.Height;
        }

        private static WpfPoint MapHorizontalTickPoint(GeometryPoint3D point, double domainMin, double domainMax, RectangleF axisRect, double axisY)
        {
            var x = DomainToDeviceX(point.X, domainMin, domainMax, axisRect);
            var y = axisY + point.Y * axisRect.Height;
            return new WpfPoint(x, y);
        }

        private static WpfPoint MapVerticalTickPoint(GeometryPoint3D point, double domainMin, double domainMax, RectangleF axisRect, double axisX)
        {
            var x = axisX + point.X * axisRect.Width;
            var y = DomainToDeviceY(point.Y, domainMin, domainMax, axisRect);
            return new WpfPoint(x, y);
        }

        // ── Text helpers ──────────────────────────────────────────────────────

        private static FormattedText MakeFormattedText(string text, double emSize, bool bold = false)
        {
            var typeface = new Typeface(new WpfFontFamily(FontFamily),
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

        private static void DrawRotatedCenteredText(DrawingContext dc, FormattedText ft, double centerX, double centerY, double angle)
        {
            dc.PushTransform(new TranslateTransform(centerX, centerY));
            dc.PushTransform(new RotateTransform(angle));
            try
            {
                dc.DrawText(ft, new WpfPoint(-ft.Width / 2d, -ft.Height / 2d));
            }
            finally
            {
                dc.Pop();
                dc.Pop();
            }
        }

        // ── Color + rect conversion ───────────────────────────────────────────

        private static WpfColor ToWpfColor(GraphColor c)
        {
            return WpfColor.FromArgb(c.A, c.R, c.G, c.B);
        }

        private static WpfRect ToWpfRect(RectangleF r)
        {
            return new WpfRect(r.X, r.Y, r.Width, r.Height);
        }

        private static WpfRect ToWpfRect(Rectangle r)
        {
            return new WpfRect(r.X, r.Y, r.Width, r.Height);
        }

        // ── Tick step ─────────────────────────────────────────────────────────

        private static int ComputeHorizontalTickStep(IReadOnlyList<AxisTickPresentation> ticks, WpfRect? bandRect)
        {
            if (!bandRect.HasValue || bandRect.Value.Width <= 1d)
            {
                return 1;
            }

            var maxLabelWidth = 0d;
            for (var i = 0; i < ticks.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(ticks[i].Label))
                {
                    var ft = MakeFormattedText(ticks[i].Label, TickFontSize);
                    if (ft.Width > maxLabelWidth)
                    {
                        maxLabelWidth = ft.Width;
                    }
                }
            }

            if (maxLabelWidth <= 0d)
            {
                return 1;
            }

            var allowed = (int)Math.Floor(bandRect.Value.Width / (maxLabelWidth + 2d));
            if (allowed <= 0)
            {
                return int.MaxValue;
            }

            return Math.Max(1, (int)Math.Ceiling(ticks.Count / (double)allowed));
        }

        private static bool ShouldRenderTickLabel(int index, int step)
        {
            return step != int.MaxValue && index % Math.Max(1, step) == 0;
        }

        // ── Misc helpers ──────────────────────────────────────────────────────

        private static double Clamp01(double value)
        {
            return value < 0d ? 0d : value > 1d ? 1d : value;
        }

        private static WpfPen MakePen(System.Windows.Media.Brush brush, double thickness)
        {
            var pen = new WpfPen(brush, thickness);
            pen.Freeze();
            return pen;
        }

        // ── WPF measurement input ─────────────────────────────────────────────

        private sealed class WpfLayoutMeasurementInput : IGraphLayoutMeasurementInput
        {
            private readonly Rectangle _deviceBounds;

            internal WpfLayoutMeasurementInput(Rectangle deviceBounds)
            {
                _deviceBounds = deviceBounds;
            }

            public double MeasureAxisTickThickness(AxisSide side, IReadOnlyList<AxisTickPresentation> ticks)
            {
                var maxWidth = 0d;
                var maxHeight = 0d;
                for (var i = 0; i < ticks.Count; i++)
                {
                    var label = ticks[i].Label;
                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }

                    var ft = MakeFormattedText(label, TickFontSize);
                    if (ft.Width > maxWidth)
                    {
                        maxWidth = ft.Width;
                    }

                    if (ft.Height > maxHeight)
                    {
                        maxHeight = ft.Height;
                    }
                }

                var pixelThickness = side == AxisSide.Left || side == AxisSide.Right
                    ? TickLength + TickLabelOffset + maxWidth
                    : TickLength + TickLabelOffset + maxHeight;

                return NormalizeThickness(pixelThickness, side);
            }

            public double MeasureAxisTitleThickness(AxisSide side, string title)
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return 0d;
                }

                var ft = MakeFormattedText(title, AxisTitleFontSize, bold: true);
                return NormalizeThickness(AxisTitleOffset + ft.Height, side);
            }

            public double MeasureAxisEndpointLabelExtent(AxisSide side, IReadOnlyList<AxisTickPresentation> ticks)
            {
                var maxWidth = 0d;
                var maxHeight = 0d;
                for (var i = 0; i < ticks.Count; i++)
                {
                    var label = ticks[i].Label;
                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }

                    var ft = MakeFormattedText(label, TickFontSize);
                    if (ft.Width > maxWidth)
                    {
                        maxWidth = ft.Width;
                    }

                    if (ft.Height > maxHeight)
                    {
                        maxHeight = ft.Height;
                    }
                }

                if (side == AxisSide.Left || side == AxisSide.Right)
                {
                    return _deviceBounds.Height > 0 ? maxHeight / _deviceBounds.Height : 0d;
                }

                return _deviceBounds.Width > 0 ? maxWidth / _deviceBounds.Width : 0d;
            }

            public LegendMeasurementAdvice MeasureLegend(
                LegendPlacement placement,
                IReadOnlyList<SeriesPresentationGeometry> series,
                double availablePrimarySpan)
            {
                if (series == null || series.Count == 0)
                {
                    return new LegendMeasurementAdvice(0d, 0d, 0d, 0d, 0, 0);
                }

                var maxItemWidth = 0d;
                var itemHeight = MakeFormattedText("Ay", LegendFontSize).Height + 4d;

                for (var i = 0; i < series.Count; i++)
                {
                    var label = string.IsNullOrWhiteSpace(series[i].Label)
                        ? series[i].SeriesId?.ToString() ?? string.Empty
                        : series[i].Label;
                    var ft = MakeFormattedText(label, LegendFontSize);
                    var w = LegendGlyphSampleWidth + LegendTextOffset + ft.Width;
                    if (w > maxItemWidth)
                    {
                        maxItemWidth = w;
                    }
                }

                var itemWidthNorm = _deviceBounds.Width > 0 ? maxItemWidth / _deviceBounds.Width : 0d;
                var itemHeightNorm = _deviceBounds.Height > 0 ? itemHeight / _deviceBounds.Height : 0d;

                if (placement == LegendPlacement.Left || placement == LegendPlacement.Right)
                {
                    var availableHeightPx = _deviceBounds.Height > 0 ? availablePrimarySpan * _deviceBounds.Height : 0d;
                    var rowsPerCol = availableHeightPx > 0d
                        ? Math.Max(1, (int)Math.Floor((availableHeightPx + LegendEntryGapPixels) / (itemHeight + LegendEntryGapPixels)))
                        : 1;
                    var colCount = (int)Math.Ceiling(series.Count / (double)rowsPerCol);
                    var pixelWidth = 2d * LegendOuterPaddingPixels + 2d * LegendInnerPaddingPixels
                        + colCount * maxItemWidth + Math.Max(0, colCount - 1) * LegendEntryGapPixels
                        + LegendMeasurementSafetyMarginPixels;
                    return new LegendMeasurementAdvice(
                        _deviceBounds.Width > 0 ? pixelWidth / _deviceBounds.Width : 0d,
                        itemWidthNorm, itemHeightNorm, availablePrimarySpan, rowsPerCol, colCount);
                }

                var availableWidthPx = _deviceBounds.Width > 0 ? availablePrimarySpan * _deviceBounds.Width : 0d;
                var contentWidthPx = availableWidthPx - 2d * LegendOuterPaddingPixels - 2d * LegendInnerPaddingPixels;
                var itemsPerRow = availableWidthPx > 0d
                    ? Math.Max(1, (int)Math.Floor((availableWidthPx + LegendEntryGapPixels) / (maxItemWidth + LegendEntryGapPixels)))
                    : 1;
                if (maxItemWidth > contentWidthPx)
                {
                    itemsPerRow = 1;
                }
                else if (itemsPerRow > 1)
                {
                    var packed = itemsPerRow * maxItemWidth + (itemsPerRow - 1) * LegendEntryGapPixels;
                    if (packed > contentWidthPx)
                    {
                        itemsPerRow = Math.Max(1, itemsPerRow - 1);
                    }
                }

                var rowCount = (int)Math.Ceiling(series.Count / (double)itemsPerRow);
                var pixelHeight = 2d * LegendOuterPaddingPixels + 2d * LegendInnerPaddingPixels
                    + rowCount * itemHeight + Math.Max(0, rowCount - 1) * LegendEntryGapPixels
                    + LegendMeasurementSafetyMarginPixels;
                return new LegendMeasurementAdvice(
                    _deviceBounds.Height > 0 ? pixelHeight / _deviceBounds.Height : 0d,
                    itemWidthNorm, itemHeightNorm, availablePrimarySpan, itemsPerRow, rowCount);
            }

            public double MeasureTitleThickness(string text, bool isSubtitle)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return 0d;
                }

                var ft = MakeFormattedText(text, isSubtitle ? GraphSubtitleFontSize : GraphTitleFontSize, bold: !isSubtitle);
                return _deviceBounds.Height > 0 ? ft.Height / _deviceBounds.Height : 0d;
            }

            private double NormalizeThickness(double pixelThickness, AxisSide side)
            {
                if (side == AxisSide.Left || side == AxisSide.Right)
                {
                    return _deviceBounds.Width > 0 ? pixelThickness / _deviceBounds.Width : 0d;
                }

                return _deviceBounds.Height > 0 ? pixelThickness / _deviceBounds.Height : 0d;
            }
        }
    }
}
