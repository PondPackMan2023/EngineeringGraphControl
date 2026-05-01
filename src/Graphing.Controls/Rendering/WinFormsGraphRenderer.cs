using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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
        private const float GridLineWidth = 0.5f;
        private const float LegendLineWidth = 1f;
        private const float LegendTextOffset = 6f;
        private const float LegendMarkerSize = 4f;
        private const float LegendGlyphSampleWidth = 18f;
        private const float LegendOuterPaddingPixels = 4f;
        private const float LegendInnerPaddingPixels = 7f;
        private const float LegendEntryGapPixels = 4f;
        private const float LegendMeasurementSafetyMarginPixels = 8f;
        private const TextFormatFlags LegendTextFormatFlags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

        private static readonly Pen AxisPen = new Pen(Color.Black, AxisLineWidth);
        private static readonly Pen PlotAreaBorderPen = new Pen(Color.Black, AxisLineWidth);
        private static readonly Pen OuterGraphBorderPen = new Pen(Color.Black, AxisLineWidth);
        private static readonly Pen GridLinesPen = new Pen(Color.LightGray, GridLineWidth);
        private static readonly Pen SeriesPen = new Pen(Color.SteelBlue, SeriesLineWidth);
        private static readonly Pen LegendBorderPen = new Pen(Color.Black, LegendLineWidth);
        private static readonly Pen LegendGlyphPen = new Pen(Color.DimGray, LegendLineWidth);
        private static readonly Font TickFont = new Font("Arial", 7f);
        private static readonly Font AxisTitleFont = new Font("Arial", 8f, FontStyle.Bold);
        private static readonly Font GraphTitleFont = new Font("Arial", 12f, FontStyle.Bold);
        private static readonly Font GraphSubtitleFont = new Font("Arial", 10f);
        private static readonly Font LegendFont = new Font("Arial", 8f);
        private static readonly Brush TickLabelBrush = Brushes.Black;

        /// <summary>
        /// Renders axes and series from <paramref name="model"/> into <paramref name="g"/>
        /// within the specified <paramref name="deviceBounds"/>.
        /// </summary>
        internal void Render(Graphics g, Rectangle deviceBounds, GraphPresentationModel model, GraphPresentationOptions options = null)
        {
            if (g == null || model == null || deviceBounds.Width <= 0 || deviceBounds.Height <= 0)
            {
                return;
            }

            var plotRect = ComputeDevicePlotRect(deviceBounds, model.Layout.PlotArea);

            if (plotRect.Width <= 0 || plotRect.Height <= 0)
            {
                return;
            }

            RenderOuterGraphBorder(g, deviceBounds, options);
            RenderGridLines(g, plotRect, model.Layout.GridLines, model);
            RenderAxes(g, plotRect, deviceBounds, model);
            RenderPlotAreaBorder(g, plotRect);
            RenderSeries(g, plotRect, model);
            RenderAxisTitles(g, deviceBounds, model.Layout.AxisTitleBands);
            RenderTitles(g, deviceBounds, model.Layout);
            RenderLegend(g, deviceBounds, model.Layout.Legend);
        }

        internal IGraphLayoutMeasurementInput CreateMeasurementInput(Graphics g, Rectangle deviceBounds)
        {
            return new WinFormsLayoutMeasurementInput(g, deviceBounds);
        }

        private sealed class WinFormsLayoutMeasurementInput : IGraphLayoutMeasurementInput
        {
            private readonly Graphics _graphics;
            private readonly Rectangle _deviceBounds;

            internal WinFormsLayoutMeasurementInput(Graphics graphics, Rectangle deviceBounds)
            {
                _graphics = graphics;
                _deviceBounds = deviceBounds;
            }

            public double MeasureAxisTickThickness(AxisSide side, IReadOnlyList<AxisTickPresentation> ticks)
            {
                var maxWidth = 0f;
                var maxHeight = 0f;
                for (var i = 0; i < ticks.Count; i++)
                {
                    var label = ticks[i].Label;
                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }

                    var size = _graphics.MeasureString(label, TickFont);
                    if (size.Width > maxWidth)
                    {
                        maxWidth = size.Width;
                    }

                    if (size.Height > maxHeight)
                    {
                        maxHeight = size.Height;
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

                var size = _graphics.MeasureString(title, AxisTitleFont);
                var pixelThickness = side == AxisSide.Left || side == AxisSide.Right
                    ? AxisTitleOffset + size.Height
                    : AxisTitleOffset + size.Height;

                return NormalizeThickness(pixelThickness, side);
            }

            public double MeasureAxisEndpointLabelExtent(AxisSide side, IReadOnlyList<AxisTickPresentation> ticks)
            {
                var maxWidth = 0f;
                var maxHeight = 0f;
                for (var i = 0; i < ticks.Count; i++)
                {
                    var label = ticks[i].Label;
                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }

                    var size = _graphics.MeasureString(label, TickFont);
                    if (size.Width > maxWidth)
                    {
                        maxWidth = size.Width;
                    }

                    if (size.Height > maxHeight)
                    {
                        maxHeight = size.Height;
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

                var maxItemWidth = 0f;
                var itemHeight = LegendFont.GetHeight(_graphics) + 4f;
                for (var i = 0; i < series.Count; i++)
                {
                    var label = string.IsNullOrWhiteSpace(series[i].Label)
                        ? series[i].SeriesId != null ? series[i].SeriesId.ToString() : string.Empty
                        : series[i].Label;
                    var size = MeasureLegendText(_graphics, label);
                    var width = LegendGlyphSampleWidth + LegendTextOffset + size.Width;
                    if (width > maxItemWidth)
                    {
                        maxItemWidth = width;
                    }
                }

                var itemWidthNormalized = _deviceBounds.Width > 0 ? maxItemWidth / _deviceBounds.Width : 0d;
                var itemHeightNormalized = _deviceBounds.Height > 0 ? itemHeight / _deviceBounds.Height : 0d;

                if (placement == LegendPlacement.Left || placement == LegendPlacement.Right)
                {
                    var availableHeightPixels = _deviceBounds.Height > 0 ? (float)(availablePrimarySpan * _deviceBounds.Height) : 0f;
                    var rowsPerColumn = 1;
                    if (availableHeightPixels > 0f)
                    {
                        rowsPerColumn = Math.Max(1, (int)Math.Floor((availableHeightPixels + LegendEntryGapPixels) / (itemHeight + LegendEntryGapPixels)));
                    }

                    var columnCount = (int)Math.Ceiling(series.Count / (double)rowsPerColumn);
                    var pixelWidth = (2f * LegendOuterPaddingPixels)
                        + (2f * LegendInnerPaddingPixels)
                        + (columnCount * maxItemWidth)
                        + ((columnCount > 1 ? columnCount - 1 : 0) * LegendEntryGapPixels)
                        + LegendMeasurementSafetyMarginPixels;
                    var normalizedWidth = _deviceBounds.Width > 0 ? pixelWidth / _deviceBounds.Width : 0d;

                    return new LegendMeasurementAdvice(
                        normalizedWidth,
                        itemWidthNormalized,
                        itemHeightNormalized,
                        availablePrimarySpan,
                        rowsPerColumn,
                        columnCount);
                }

                var availableWidthPixels = _deviceBounds.Width > 0 ? (float)(availablePrimarySpan * _deviceBounds.Width) : 0f;
                var itemsPerRow = 1;
                if (availableWidthPixels > 0f)
                {
                    itemsPerRow = Math.Max(1, (int)Math.Floor((availableWidthPixels + LegendEntryGapPixels) / (maxItemWidth + LegendEntryGapPixels)));
                }

                var rowCount = (int)Math.Ceiling(series.Count / (double)itemsPerRow);
                var pixelHeight = (2f * LegendOuterPaddingPixels)
                    + (2f * LegendInnerPaddingPixels)
                    + (rowCount * itemHeight)
                    + ((rowCount > 1 ? rowCount - 1 : 0) * LegendEntryGapPixels)
                    + LegendMeasurementSafetyMarginPixels;
                var normalizedHeight = _deviceBounds.Height > 0 ? pixelHeight / _deviceBounds.Height : 0d;
                return new LegendMeasurementAdvice(
                    normalizedHeight,
                    itemWidthNormalized,
                    itemHeightNormalized,
                    availablePrimarySpan,
                    itemsPerRow,
                    rowCount);
            }

            public double MeasureTitleThickness(string text, bool isSubtitle)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return 0d;
                }

                var font = isSubtitle ? GraphSubtitleFont : GraphTitleFont;
                var size = _graphics.MeasureString(text, font);
                return _deviceBounds.Height > 0 ? size.Height / _deviceBounds.Height : 0d;
            }

            private double NormalizeThickness(float pixelThickness, AxisSide side)
            {
                if (side == AxisSide.Left || side == AxisSide.Right)
                {
                    return _deviceBounds.Width > 0 ? pixelThickness / _deviceBounds.Width : 0d;
                }

                return _deviceBounds.Height > 0 ? pixelThickness / _deviceBounds.Height : 0d;
            }
        }

        // ── Plot area border rendering ─────────────────────────────────────────

        /// <summary>
        /// Renders a decorative border around the entire control bounds.
        /// This is a renderer-level concern, independent of presentation geometry.
        /// Controlled by <see cref="GraphPresentationOptions.ShowGraphBorder"/> (defaults to true).
        /// </summary>
        private static void RenderOuterGraphBorder(Graphics g, Rectangle deviceBounds, GraphPresentationOptions options)
        {
            // Default to showing the border if options are not supplied
            var showBorder = options?.ShowGraphBorder ?? true;
            if (!showBorder)
            {
                return;
            }

            g.DrawRectangle(OuterGraphBorderPen, deviceBounds.X, deviceBounds.Y, deviceBounds.Width - 1, deviceBounds.Height - 1);
        }

        /// <summary>
        /// Renders the plot area border as a simple rectangular frame.
        /// The border is drawn behind series but above the background.
        /// Geometry is consumed directly from the supplied device-space <paramref name="plotRect"/>,
        /// which is derived from the abstract <see cref="PlotAreaLayout"/> by the caller.
        /// </summary>
        private static void RenderPlotAreaBorder(Graphics g, RectangleF plotRect)
        {
            g.DrawRectangle(PlotAreaBorderPen, plotRect.X, plotRect.Y, plotRect.Width, plotRect.Height);
        }

        /// <summary>
        /// Renders grid lines derived from axis ticks.
        /// Grid lines are drawn inside the plot area, behind series.
        /// Each grid line is normalized using its bound AxisLayoutEntry, exactly like series geometry.
        /// </summary>
        private static void RenderGridLines(Graphics g, RectangleF plotRect, GridLinesGeometry gridLines, GraphPresentationModel model)
        {
            if (gridLines == null || model == null)
            {
                return;
            }

            // Clipping ensures grid lines stay inside the plot area
            var clip = g.ClipBounds;
            g.SetClip(plotRect, System.Drawing.Drawing2D.CombineMode.Intersect);

            try
            {
                // Render vertical grid lines (from X-axis ticks)
                // Vertical grid lines span the full plot height using the full plotRect.
                var verticalLines = gridLines.VerticalLines;
                for (var i = 0; i < verticalLines.Count; i++)
                {
                    var line = verticalLines[i];
                    if (line.AxisEntry == null)
                    {
                        continue;
                    }

                    var xAxis = line.AxisEntry.Axis;
                    if (!xAxis.MinimumValue.HasValue || !xAxis.MaximumValue.HasValue)
                    {
                        continue;
                    }

                    var xMin = xAxis.MinimumValue.Value;
                    var xMax = xAxis.MaximumValue.Value;
                    var axisRect = ComputeAxisRect(plotRect, line.AxisEntry);

                    // Render vertical line spanning full plot height.
                    var startDeviceX = DomainToDeviceX(line.Start.X, xMin, xMax, axisRect);
                    var endDeviceX = DomainToDeviceX(line.End.X, xMin, xMax, axisRect);
                    var startDeviceY = plotRect.Top;
                    var endDeviceY = plotRect.Bottom;

                    g.DrawLine(GridLinesPen, startDeviceX, startDeviceY, endDeviceX, endDeviceY);
                }

                // Render horizontal grid lines (from Y-axis ticks)
                // Each horizontal grid line must be mapped using its bound Y-axis rectangle,
                // exactly like series geometry. This ensures correct positioning with stacked axes.
                var horizontalLines = gridLines.HorizontalLines;
                for (var i = 0; i < horizontalLines.Count; i++)
                {
                    var line = horizontalLines[i];
                    if (line.AxisEntry == null)
                    {
                        continue;
                    }

                    var yAxis = line.AxisEntry.Axis;
                    if (!yAxis.MinimumValue.HasValue || !yAxis.MaximumValue.HasValue)
                    {
                        continue;
                    }

                    var yMin = yAxis.MinimumValue.Value;
                    var yMax = yAxis.MaximumValue.Value;

                    // Aggregate all X-axis domains to span the full plot width.
                    var xEntries = model.Layout.Axes;
                    var xMin = double.MaxValue;
                    var xMax = double.MinValue;

                    for (var j = 0; j < xEntries.Count; j++)
                    {
                        var entry = xEntries[j];
                        if (entry.Axis.Orientation == AxisOrientation.Horizontal && entry.Axis.MinimumValue.HasValue && entry.Axis.MaximumValue.HasValue)
                        {
                            xMin = Math.Min(xMin, entry.Axis.MinimumValue.Value);
                            xMax = Math.Max(xMax, entry.Axis.MaximumValue.Value);
                        }
                    }

                    if (xMin >= xMax)
                    {
                        continue;
                    }

                    // Map using the Y-axis-specific rectangle to respect stacked layout.
                    var axisRect = ComputeAxisRect(plotRect, line.AxisEntry);

                    var startDeviceX = DomainToDeviceX(line.Start.X, xMin, xMax, plotRect);
                    var endDeviceX = DomainToDeviceX(line.End.X, xMin, xMax, plotRect);
                    var startDeviceY = DomainToDeviceY(line.Start.Y, yMin, yMax, axisRect);
                    var endDeviceY = DomainToDeviceY(line.End.Y, yMin, yMax, axisRect);

                    g.DrawLine(GridLinesPen, startDeviceX, startDeviceY, endDeviceX, endDeviceY);
                }
            }
            finally
            {
                g.SetClip(clip);
            }
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
                var tickRegionRect = FindAxisTickLabelRegionRect(deviceBounds, model.Layout.AxisTitleBands, entry.Side);

                if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
                {
                    continue;
                }

                switch (entry.Side)
                {
                    case AxisSide.Bottom:
                        RenderBottomAxis(g, axisRect, tickRegionRect, axis, entry.TickEndpointInset);
                        break;

                    case AxisSide.Left:
                        RenderLeftAxis(g, axisRect, tickRegionRect, axis, entry.TickEndpointInset);
                        break;

                    case AxisSide.Right:
                        RenderRightAxis(g, axisRect, tickRegionRect, axis, entry.TickEndpointInset);
                        break;

                    case AxisSide.Top:
                        RenderTopAxis(g, axisRect, tickRegionRect, axis, entry.TickEndpointInset);
                        break;
                }
            }
        }

        private static void RenderBottomAxis(
            Graphics g,
            RectangleF plotRect,
            RectangleF? sideBandRect,
            AxisPresentationGeometry axis,
            double endpointInset)
        {
            _ = endpointInset;
            if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
            {
                return;
            }

            var domainMin = axis.MinimumValue.Value;
            var domainMax = axis.MaximumValue.Value;
            var axisY = plotRect.Bottom;
            g.DrawLine(AxisPen, plotRect.Left, axisY, plotRect.Right, axisY);

            var clip = g.ClipBounds;
            if (sideBandRect.HasValue)
            {
                // Allow labels to extend left/right, but still clip vertically
                var relaxed = new RectangleF(
                    clip.Left,                    // allow full horizontal extent
                    sideBandRect.Value.Top,       // keep vertical bounds
                    clip.Width,
                    sideBandRect.Value.Height);

                g.SetClip(relaxed, CombineMode.Intersect);
            }

            var ticks = axis.Ticks;
            var step = ComputeTickLabelStep(g, ticks, sideBandRect, AxisSide.Bottom);
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var deviceX = DomainToDeviceX(tick.Value, domainMin, domainMax, plotRect);
                    g.DrawLine(AxisPen, deviceX, axisY, deviceX, axisY + TickLength);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, step))
                    {
                        // X-axis note:
                        // The first tick label is allowed to render beyond the axis origin (to the left of the Y-axis).
                        // The last tick label is intentionally always suppressed (WaterGEMS-consistent).
                        // X-axis geometry is never inset or adjusted to accommodate tick labels.
                        if (i == ticks.Count - 1)
                        {
                            continue;
                        }

                        var labelSize = g.MeasureString(tick.Label, TickFont);
                        var x = deviceX - labelSize.Width / 2f;
                        var y = axisY + TickLength + TickLabelOffset;
                        if (sideBandRect.HasValue)
                        {
                            y = Math.Min(y, sideBandRect.Value.Bottom - labelSize.Height);
                        }

                        g.DrawString(tick.Label, TickFont, TickLabelBrush, x, y);
                    }
                }
            }
            finally
            {
                g.SetClip(clip);
            }
        }

        private static void RenderLeftAxis(
            Graphics g,
            RectangleF plotRect,
            RectangleF? sideBandRect,
            AxisPresentationGeometry axis,
            double endpointInset)
        {
            _ = endpointInset;
            if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
            {
                return;
            }

            var domainMin = axis.MinimumValue.Value;
            var domainMax = axis.MaximumValue.Value;
            var axisX = plotRect.Left;
            g.DrawLine(AxisPen, axisX, plotRect.Top, axisX, plotRect.Bottom);

            var clip = g.ClipBounds;
            if (sideBandRect.HasValue)
            {
                g.SetClip(sideBandRect.Value, System.Drawing.Drawing2D.CombineMode.Intersect);
            }

            var ticks = axis.Ticks;
            const int step = 1;
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var deviceY = DomainToDeviceY(tick.Value, domainMin, domainMax, plotRect);
                    g.DrawLine(AxisPen, axisX, deviceY, axisX + TickLength, deviceY);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, step))
                    {
                        var label = FitLabelToWidth(g, tick.Label, TickFont, sideBandRect.HasValue ? sideBandRect.Value.Width : float.MaxValue);
                        if (string.IsNullOrEmpty(label))
                        {
                            continue;
                        }

                        var labelSize = g.MeasureString(label, TickFont);
                        var x = axisX - TickLength - TickLabelOffset - labelSize.Width;
                        var y = deviceY - labelSize.Height / 2f;
                        if (sideBandRect.HasValue)
                        {
                            x = Math.Max(x, sideBandRect.Value.Left);
                        }

                        g.DrawString(label, TickFont, TickLabelBrush, x, y);
                    }
                }
            }
            finally
            {
                g.SetClip(clip);
            }
        }

        private static void RenderRightAxis(
            Graphics g,
            RectangleF plotRect,
            RectangleF? sideBandRect,
            AxisPresentationGeometry axis,
            double endpointInset)
        {
            _ = endpointInset;
            if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
            {
                return;
            }

            var domainMin = axis.MinimumValue.Value;
            var domainMax = axis.MaximumValue.Value;
            var axisX = plotRect.Right;
            g.DrawLine(AxisPen, axisX, plotRect.Top, axisX, plotRect.Bottom);

            var clip = g.ClipBounds;
            if (sideBandRect.HasValue)
            {
                g.SetClip(sideBandRect.Value, System.Drawing.Drawing2D.CombineMode.Intersect);
            }

            var ticks = axis.Ticks;
            const int step = 1;
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var deviceY = DomainToDeviceY(tick.Value, domainMin, domainMax, plotRect);
                    g.DrawLine(AxisPen, axisX - TickLength, deviceY, axisX, deviceY);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, step))
                    {
                        var label = FitLabelToWidth(g, tick.Label, TickFont, sideBandRect.HasValue ? sideBandRect.Value.Width : float.MaxValue);
                        if (string.IsNullOrEmpty(label))
                        {
                            continue;
                        }

                        var labelSize = g.MeasureString(label, TickFont);
                        var x = axisX + TickLength + TickLabelOffset;
                        var y = deviceY - TickFont.Height / 2f;
                        if (sideBandRect.HasValue)
                        {
                            x = Math.Min(x, sideBandRect.Value.Right - labelSize.Width);
                        }

                        g.DrawString(label, TickFont, TickLabelBrush, x, y);
                    }
                }
            }
            finally
            {
                g.SetClip(clip);
            }
        }

        private static void RenderTopAxis(
            Graphics g,
            RectangleF plotRect,
            RectangleF? sideBandRect,
            AxisPresentationGeometry axis,
            double endpointInset)
        {
            _ = endpointInset;
            if (!axis.MinimumValue.HasValue || !axis.MaximumValue.HasValue)
            {
                return;
            }

            var domainMin = axis.MinimumValue.Value;
            var domainMax = axis.MaximumValue.Value;
            var axisY = plotRect.Top;
            g.DrawLine(AxisPen, plotRect.Left, axisY, plotRect.Right, axisY);

            var clip = g.ClipBounds;
            if (sideBandRect.HasValue)
            {
                g.SetClip(sideBandRect.Value, System.Drawing.Drawing2D.CombineMode.Intersect);
            }

            var ticks = axis.Ticks;
            var step = ComputeTickLabelStep(g, ticks, sideBandRect, AxisSide.Top);
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var deviceX = DomainToDeviceX(tick.Value, domainMin, domainMax, plotRect);
                    g.DrawLine(AxisPen, deviceX, axisY - TickLength, deviceX, axisY);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, step))
                    {
                        if (i == ticks.Count - 1)
                        {
                            continue;
                        }

                        var labelSize = g.MeasureString(tick.Label, TickFont);
                        var x = deviceX - labelSize.Width / 2f;
                        var y = axisY - TickLength - TickLabelOffset - TickFont.Height;
                        if (sideBandRect.HasValue)
                        {
                            y = Math.Max(y, sideBandRect.Value.Top);
                        }

                        g.DrawString(tick.Label, TickFont, TickLabelBrush, x, y);
                    }
                }
            }
            finally
            {
                g.SetClip(clip);
            }
        }

        private static RectangleF? FindAxisTickLabelRegionRect(
            Rectangle deviceBounds,
            IReadOnlyList<AxisTitleBandGeometry> bands,
            AxisSide side)
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

                return ComputeDeviceBoundsForGeometry(
                    deviceBounds,
                    bands[i].AxisTickLabelRegion.BottomLeft,
                    bands[i].AxisTickLabelRegion.TopRight);
            }

            return null;
        }

        private static void RenderAxisTitles(
            Graphics g,
            Rectangle deviceBounds,
            IReadOnlyList<AxisTitleBandGeometry> bands)
        {
            if (bands == null)
            {
                return;
            }

            for (var bandIndex = 0; bandIndex < bands.Count; bandIndex++)
            {
                var items = bands[bandIndex].Items;
                for (var itemIndex = 0; itemIndex < items.Count; itemIndex++)
                {
                    var item = items[itemIndex];
                    if (string.IsNullOrWhiteSpace(item.Title))
                    {
                        continue;
                    }

                    var itemRect = ComputeDeviceBoundsForGeometry(
                        deviceBounds,
                        item.AxisTitleRegion.BottomLeft,
                        item.AxisTitleRegion.TopRight);
                    if (itemRect.Width <= 0 || itemRect.Height <= 0)
                    {
                        continue;
                    }

                    var centerX = itemRect.Left + (itemRect.Width / 2f);
                    var centerY = itemRect.Top + (itemRect.Height / 2f);

                    if (item.Side == AxisSide.Left)
                    {
                        DrawRotatedCenteredText(g, item.Title, AxisTitleFont, TickLabelBrush, centerX, centerY, -90f);
                    }
                    else if (item.Side == AxisSide.Right)
                    {
                        DrawRotatedCenteredText(g, item.Title, AxisTitleFont, TickLabelBrush, centerX, centerY, 90f);
                    }
                    else
                    {
                        var titleSize = g.MeasureString(item.Title, AxisTitleFont);
                        var x = itemRect.Left + (itemRect.Width - titleSize.Width) / 2f;
                        var y = itemRect.Top + (itemRect.Height - titleSize.Height) / 2f;
                        g.DrawString(item.Title, AxisTitleFont, TickLabelBrush, x, y);
                    }
                }
            }
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

        private static void RenderLegend(Graphics g, Rectangle deviceBounds, LegendPresentationGeometry legend)
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
                g.DrawRectangle(LegendBorderPen, legendRect.X, legendRect.Y, legendRect.Width, legendRect.Height);
            }

            var contentRect = ComputeDeviceBoundsForGeometry(deviceBounds, legend.ContentBottomLeft, legend.ContentTopRight);
            var clip = g.ClipBounds;
            g.SetClip(contentRect, System.Drawing.Drawing2D.CombineMode.Intersect);
            try
            {
                var entries = legend.Entries;
                for (var i = 0; i < entries.Count; i++)
                {
                    RenderLegendEntry(g, deviceBounds, entries[i]);
                }
            }
            finally
            {
                g.SetClip(clip);
            }
        }

        private static void RenderLegendEntry(Graphics g, Rectangle deviceBounds, LegendEntryPresentationGeometry entry)
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
            var glyphWidth = Math.Min(LegendGlyphSampleWidth, Math.Max(0f, entryRect.Right - glyphLeft));
            var glyphRight = glyphLeft + glyphWidth;
            var glyphCenterY = glyphRect.Top + (glyphRect.Height / 2f);

            using (var glyphPen = new Pen(entry.SeriesColor, LegendLineWidth))
            {
                g.DrawLine(glyphPen, glyphLeft, glyphCenterY, glyphRight, glyphCenterY);

                var markerRadius = LegendMarkerSize / 2f;
                var markerCenterX = glyphLeft + (glyphWidth / 2f);
                g.DrawEllipse(
                    glyphPen,
                    markerCenterX - markerRadius,
                    glyphCenterY - markerRadius,
                    LegendMarkerSize,
                    LegendMarkerSize);
            }

            if (string.IsNullOrWhiteSpace(entry.DisplayText))
            {
                return;
            }

            var textX = glyphRight + LegendTextOffset;
            var textSize = MeasureLegendText(g, entry.DisplayText);
            var textY = entryRect.Top + ((entryRect.Height - textSize.Height) / 2f);
            DrawLegendText(g, entry.DisplayText, textX, textY);
        }

        private static Size MeasureLegendText(Graphics g, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }

            return TextRenderer.MeasureText(g, text, LegendFont, new Size(int.MaxValue, int.MaxValue), LegendTextFormatFlags);
        }

        private static void DrawLegendText(Graphics g, string text, float x, float y)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            TextRenderer.DrawText(g, text, LegendFont, new Point((int)Math.Round(x), (int)Math.Round(y)), Color.Black, LegendTextFormatFlags);
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

        private static int ComputeTickLabelStep(
            Graphics g,
            IReadOnlyList<AxisTickPresentation> ticks,
            RectangleF? tickLabelRegion,
            AxisSide side)
        {
            var region = tickLabelRegion.Value;
            if (region.Width <= 1f || region.Height <= 1f)
            {
                return int.MaxValue;
            }

            var maxLabelWidth = 0f;
            for (var i = 0; i < ticks.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(ticks[i].Label))
                {
                    continue;
                }

                var size = g.MeasureString(ticks[i].Label, TickFont);
                if (size.Width > maxLabelWidth)
                {
                    maxLabelWidth = size.Width;
                }
            }

            if (maxLabelWidth <= 0f)
            {
                return 1;
            }

            int allowedLabels;
            if (side == AxisSide.Left || side == AxisSide.Right)
            {
                var slotHeight = TickFont.GetHeight(g) + 2f;
                allowedLabels = slotHeight > 0f ? (int)Math.Floor(region.Height / slotHeight) : 1;
            }
            else
            {
                allowedLabels = (int)Math.Floor(region.Width / (maxLabelWidth + 2f));
            }

            if (allowedLabels <= 0)
            {
                return int.MaxValue;
            }

            return Math.Max(1, (int)Math.Ceiling(ticks.Count / (double)allowedLabels));
        }

        private static bool ShouldRenderTickLabel(int index, int step)
        {
            if (step == int.MaxValue)
            {
                return false;
            }

            return index % Math.Max(1, step) == 0;
        }

        private static string FitLabelToWidth(Graphics g, string text, Font font, float maxWidth)
        {
            if (string.IsNullOrEmpty(text) || maxWidth <= 1f)
            {
                return string.Empty;
            }

            if (g.MeasureString(text, font).Width <= maxWidth)
            {
                return text;
            }

            const string Ellipsis = "...";
            for (var len = text.Length - 1; len > 0; len--)
            {
                var candidate = text.Substring(0, len) + Ellipsis;
                if (g.MeasureString(candidate, font).Width <= maxWidth)
                {
                    return candidate;
                }
            }

            return string.Empty;
        }

        // ── Series rendering ──────────────────────────────────────────────────

        private static void RenderSeries(
            Graphics g,
            RectangleF plotRect,
            GraphPresentationModel model)
        {
            var series = model.Layout.Series;

            for (var i = 0; i < series.Count; i++)
            {
                var s = series[i];
                var xAxisEntry = s.XAxisEntry;
                var yAxisEntry = s.YAxisEntry;

                if (xAxisEntry == null || yAxisEntry == null)
                {
                    continue;
                }

                var xAxis = xAxisEntry.Axis;
                var yAxis = yAxisEntry.Axis;

                if (!xAxis.MinimumValue.HasValue || !xAxis.MaximumValue.HasValue)
                {
                    continue;
                }

                if (!yAxis.MinimumValue.HasValue || !yAxis.MaximumValue.HasValue)
                {
                    continue;
                }

                double xMin = xAxis.MinimumValue.Value;
                double xMax = xAxis.MaximumValue.Value;
                if (xMin >= xMax)
                {
                    continue;
                }

                double yMin = yAxis.MinimumValue.Value;
                double yMax = yAxis.MaximumValue.Value;
                if (yMin >= yMax)
                {
                    continue;
                }

                var seriesRect = ComputeSeriesRect(plotRect, xAxisEntry, yAxisEntry);
                RenderOneSeries(g, seriesRect, s, xMin, xMax, yMin, yMax);
            }
        }

        private static void RenderOneSeries(
            Graphics g,
            RectangleF seriesRect,
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
            // Use a clipping region to keep lines inside the plot area.
            var clip = g.ClipBounds;
            g.SetClip(seriesRect, System.Drawing.Drawing2D.CombineMode.Intersect);

            using (var seriesPen = new Pen(series.SeriesColor, SeriesLineWidth))
            {
                try
                {
                    PointF? previous = null;

                    for (var i = 0; i < points.Count; i++)
                    {
                        var domainPoint = points[i];
                        var deviceX = DomainToDeviceX(domainPoint.X, xMin, xMax, seriesRect);
                        var deviceY = DomainToDeviceY(domainPoint.Y, yMin, yMax, seriesRect);
                        var current = new PointF(deviceX, deviceY);

                        if (previous.HasValue && series.ConnectivityIntent != SeriesConnectivityIntent.Discrete)
                        {
                            g.DrawLine(seriesPen, previous.Value, current);
                        }

                        previous = current;
                    }
                }
                finally
                {
                    g.SetClip(clip);
                }
            }
        }

        // ── Axis rect helpers ─────────────────────────────────────────────────

        private static RectangleF ComputeAxisRect(RectangleF plotRect, AxisLayoutEntry entry)
        {
            if (entry == null)
            {
                return plotRect;
            }

            var insetRect = ApplyAxisInset(plotRect, entry);
            if (entry.Side != AxisSide.Left)
            {
                return insetRect;
            }

            var spanStart = Clamp01(entry.NormalizedSpanStart);
            var spanEnd = Clamp01(entry.NormalizedSpanEnd);
            if (spanEnd <= spanStart)
            {
                return insetRect;
            }

            // Left-axis stacked spans map to a vertical sub-rectangle of the plot.
            var plotHeight = insetRect.Height;
            var top = insetRect.Bottom - (float)(spanEnd * plotHeight);
            var bottom = insetRect.Bottom - (float)(spanStart * plotHeight);
            return RectangleF.FromLTRB(insetRect.Left, top, insetRect.Right, bottom);
        }

        private static RectangleF ComputeSeriesRect(
            RectangleF plotRect,
            AxisLayoutEntry xAxisEntry,
            AxisLayoutEntry yAxisEntry)
        {
            var rect = plotRect;
            if (xAxisEntry != null)
            {
                rect = ApplyAxisInset(rect, xAxisEntry);
            }

            if (yAxisEntry != null)
            {
                rect = ComputeAxisRect(rect, yAxisEntry);
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
                if (bottom <= top)
                {
                    return plotRect;
                }

                return RectangleF.FromLTRB(plotRect.Left, top, plotRect.Right, bottom);
            }

            var horizontalDelta = (float)(inset * plotRect.Width);
            var left = plotRect.Left + horizontalDelta;
            var right = plotRect.Right - horizontalDelta;
            if (right <= left)
            {
                return plotRect;
            }

            return RectangleF.FromLTRB(left, plotRect.Top, right, plotRect.Bottom);
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

        // ── Coordinate transforms ─────────────────────────────────────────────

        /// <summary>
        /// Maps the abstract normalized plot area [0,1]×[0,1] to device pixel bounds.
        /// Abstract space uses math orientation (Y up); device space uses screen orientation (Y down).
        /// </summary>
        private static RectangleF ComputeDevicePlotRect(Rectangle deviceBounds, PlotAreaLayout plotArea)
        {
            var left = deviceBounds.Left + plotArea.BottomLeft.X * deviceBounds.Width;
            var right = deviceBounds.Left + plotArea.TopRight.X * deviceBounds.Width;
            var top = deviceBounds.Bottom - plotArea.TopRight.Y * deviceBounds.Height;
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
