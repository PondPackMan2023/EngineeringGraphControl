using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Graphing.Controls.Rendering
{
    /// <summary>
    /// Headless, off-screen renderer that independently implements <see cref="IGraphRenderer"/>
    /// using GDI+ metafile primitives only.
    ///
    /// This renderer is a sibling of <see cref="WinFormsGraphRenderer"/> and
    /// <see cref="BitmapGraphRenderer"/>: it does not compose, subclass, or delegate to either.
    /// All rasterization and measurement behavior is implemented locally against a
    /// metafile-backed <see cref="Graphics"/> surface.
    /// </summary>
    internal sealed class MetafileGraphRenderer : GraphRendererBase
    {
        private const int TickLength = 5;
        private const int TickLabelOffset = 3;
        private const int AxisTitleOffset = 6;
        private const float AxisLineWidth = 1f;
        private const float SeriesLineWidth = 2.0f;
        private const float GridLineWidth = 0.5f;
        private const float LegendLineWidth = 1f;
        private const float LegendTextOffset = 6f;
        private const float LegendMarkerSize = 4f;
        private const float LegendGlyphSampleWidth = 18f;
        private const float LegendOuterPaddingPixels = 4f;
        private const float LegendInnerPaddingPixels = 7f;
        private const float LegendEntryGapPixels = 4f;
        private const float LegendMeasurementSafetyMarginPixels = 8f;
        private const float DiscretePointMarkerSize = 4f;
        private const float DiscretePointMarkerRadius = DiscretePointMarkerSize / 2f;

        private static readonly Pen AxisPen = new Pen(Color.Black, AxisLineWidth);
        private static readonly Pen PlotAreaBorderPen = new Pen(Color.Black, AxisLineWidth);
        private static readonly Pen OuterGraphBorderPen = new Pen(Color.Black, AxisLineWidth);
        private static readonly Pen GridLinesPen = new Pen(Color.LightGray, GridLineWidth);
        private static readonly Pen LegendBorderPen = new Pen(Color.Black, LegendLineWidth);
        private static readonly Font TickFont = new Font("Arial", 7f);
        private static readonly Font AxisTitleFont = new Font("Arial", 8f, FontStyle.Bold);
        private static readonly Font GraphTitleFont = new Font("Arial", 12f, FontStyle.Bold);
        private static readonly Font GraphSubtitleFont = new Font("Arial", 10f);
        private static readonly Font LegendFont = new Font("Arial", 8f);
        private static readonly Brush TickLabelBrush = Brushes.Black;

        private readonly float _dpiX;
        private readonly float _dpiY;
        private readonly EmfType _emfType;

        internal MetafileGraphRenderer(float dpiX = 96f, float dpiY = 96f, EmfType emfType = EmfType.EmfPlusDual)
        {
            _dpiX = dpiX;
            _dpiY = dpiY;
            _emfType = emfType;
        }

        protected override void RenderCore(IGraphDrawingSurface surface, RectangleF plotRect, GraphPresentationModel model, GraphPresentationOptions options)
        {
            var winFormsSurface = surface as WinFormsGraphDrawingSurface;
            var g = winFormsSurface?.Graphics;
            var deviceBounds = winFormsSurface != null ? winFormsSurface.DeviceBounds : Rectangle.Empty;
            if (g == null)
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

        internal void Render(Graphics g, Rectangle deviceBounds, GraphPresentationModel model, GraphPresentationOptions options = null)
        {
            Render(new WinFormsGraphRenderContext(g), deviceBounds, model, options);
        }

        protected override IGraphLayoutMeasurementInput CreateMeasurementInputCore(IGraphRenderContext context, Rectangle deviceBounds)
        {
            var g = TryResolveGraphics(context);
            return new MetafileLayoutMeasurementInput(g, deviceBounds);
        }

        internal IGraphLayoutMeasurementInput CreateMeasurementInput(Graphics g, Rectangle deviceBounds)
        {
            return CreateMeasurementInput(new WinFormsGraphRenderContext(g), deviceBounds);
        }

        internal void RenderToMetafile(
            int width,
            int height,
            Stream output,
            GraphPresentationModel model,
            GraphPresentationOptions options = null)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            using (var referenceBitmap = new Bitmap(Math.Max(1, width), Math.Max(1, height)))
            {
                referenceBitmap.SetResolution(_dpiX, _dpiY);

                using (var referenceGraphics = Graphics.FromImage(referenceBitmap))
                {
                    var hdc = referenceGraphics.GetHdc();
                    try
                    {
                        using (var metafile = new Metafile(
                            output,
                            hdc,
                            new Rectangle(0, 0, Math.Max(1, width), Math.Max(1, height)),
                            MetafileFrameUnit.Pixel,
                            _emfType,
                            "EngineeringGraphControl"))
                        using (var metafileGraphics = Graphics.FromImage(metafile))
                        {
                            metafileGraphics.PageUnit = GraphicsUnit.Pixel;
                            metafileGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                            metafileGraphics.PixelOffsetMode = PixelOffsetMode.Half;
                            metafileGraphics.Clear(Color.White);
                            Render(metafileGraphics, new Rectangle(0, 0, width, height), model, options);
                            metafileGraphics.Flush();
                        }
                    }
                    finally
                    {
                        referenceGraphics.ReleaseHdc(hdc);
                    }
                }
            }
        }

        private sealed class MetafileLayoutMeasurementInput : IGraphLayoutMeasurementInput
        {
            private readonly Graphics _graphics;
            private readonly Rectangle _deviceBounds;

            internal MetafileLayoutMeasurementInput(Graphics graphics, Rectangle deviceBounds)
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
                var pixelThickness = AxisTitleOffset + size.Height;
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
                    var size = _graphics.MeasureString(label, LegendFont);
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
                var contentWidthPixels = availableWidthPixels - (2f * LegendOuterPaddingPixels) - (2f * LegendInnerPaddingPixels);
                var itemsPerRow = 1;
                if (availableWidthPixels > 0f)
                {
                    itemsPerRow = Math.Max(1, (int)Math.Floor((availableWidthPixels + LegendEntryGapPixels) / (maxItemWidth + LegendEntryGapPixels)));
                }

                if (maxItemWidth > contentWidthPixels)
                {
                    itemsPerRow = 1;
                }

                if (itemsPerRow > 1)
                {
                    var packedWidthPixels = (itemsPerRow * maxItemWidth) + ((itemsPerRow - 1) * LegendEntryGapPixels);
                    if (packedWidthPixels > contentWidthPixels)
                    {
                        itemsPerRow = Math.Max(1, itemsPerRow - 1);
                    }
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

        private static void RenderOuterGraphBorder(Graphics g, Rectangle deviceBounds, GraphPresentationOptions options)
        {
            var showBorder = options?.ShowGraphBorder ?? true;
            if (!showBorder)
            {
                return;
            }

            g.DrawRectangle(OuterGraphBorderPen, deviceBounds.X, deviceBounds.Y, deviceBounds.Width - 1, deviceBounds.Height - 1);
        }

        private static void RenderPlotAreaBorder(Graphics g, RectangleF plotRect)
        {
            g.DrawRectangle(PlotAreaBorderPen, plotRect.X, plotRect.Y, plotRect.Width, plotRect.Height);
        }

        private static void RenderGridLines(Graphics g, RectangleF plotRect, GridLinesGeometry gridLines, GraphPresentationModel model)
        {
            if (gridLines == null || model == null)
            {
                return;
            }

            var clip = g.ClipBounds;
            g.SetClip(plotRect, CombineMode.Intersect);

            try
            {
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

                    var startDeviceX = DomainToDeviceX(line.Start.X, xMin, xMax, axisRect);
                    var endDeviceX = DomainToDeviceX(line.End.X, xMin, xMax, axisRect);
                    var startDeviceY = plotRect.Top;
                    var endDeviceY = plotRect.Bottom;

                    g.DrawLine(GridLinesPen, startDeviceX, startDeviceY, endDeviceX, endDeviceY);
                }

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

                    var xEntries = model.Layout.Axes;
                    var xMin = double.MaxValue;
                    var xMax = double.MinValue;

                    for (var j = 0; j < xEntries.Count; j++)
                    {
                        var entry = xEntries[j];
                        if (entry.Axis.Orientation == AxisOrientation.Horizontal
                            && entry.Axis.MinimumValue.HasValue
                            && entry.Axis.MaximumValue.HasValue)
                        {
                            xMin = Math.Min(xMin, entry.Axis.MinimumValue.Value);
                            xMax = Math.Max(xMax, entry.Axis.MaximumValue.Value);
                        }
                    }

                    if (xMin >= xMax)
                    {
                        continue;
                    }

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

        private static void RenderAxes(Graphics g, RectangleF plotRect, Rectangle deviceBounds, GraphPresentationModel model)
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

        private static void RenderBottomAxis(Graphics g, RectangleF plotRect, RectangleF? sideBandRect, AxisPresentationGeometry axis, double endpointInset)
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
                var relaxed = new RectangleF(clip.Left, sideBandRect.Value.Top, clip.Width, sideBandRect.Value.Height);
                g.SetClip(relaxed, CombineMode.Intersect);
            }

            var ticks = axis.Ticks;
            var step = ComputeTickLabelStep(g, ticks, sideBandRect, AxisSide.Bottom);
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var start = MapHorizontalTickPointToDevice(tick.Start, domainMin, domainMax, plotRect, axisY);
                    var end = MapHorizontalTickPointToDevice(tick.End, domainMin, domainMax, plotRect, axisY);
                    var deviceX = DomainToDeviceX(tick.Value, domainMin, domainMax, plotRect);
                    g.DrawLine(AxisPen, start, end);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, step))
                    {
                        if (i == ticks.Count - 1)
                        {
                            continue;
                        }

                        var labelSize = g.MeasureString(tick.Label, TickFont);
                        var x = deviceX - labelSize.Width / 2f;
                        var y = Math.Max(start.Y, end.Y) + TickLabelOffset;
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

        private static void RenderLeftAxis(Graphics g, RectangleF plotRect, RectangleF? sideBandRect, AxisPresentationGeometry axis, double endpointInset)
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
                g.SetClip(sideBandRect.Value, CombineMode.Intersect);
            }

            var ticks = axis.Ticks;
            const int Step = 1;
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var deviceY = DomainToDeviceY(tick.Value, domainMin, domainMax, plotRect);
                    var start = MapVerticalTickPointToDevice(tick.Start, domainMin, domainMax, plotRect, axisX);
                    var end = MapVerticalTickPointToDevice(tick.End, domainMin, domainMax, plotRect, axisX);
                    g.DrawLine(AxisPen, start, end);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, Step))
                    {
                        var label = FitLabelToWidth(g, tick.Label, TickFont, sideBandRect.HasValue ? sideBandRect.Value.Width : float.MaxValue);
                        if (string.IsNullOrEmpty(label))
                        {
                            continue;
                        }

                        var labelSize = g.MeasureString(label, TickFont);
                        var x = Math.Min(start.X, end.X) - TickLabelOffset - labelSize.Width;
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

        private static void RenderRightAxis(Graphics g, RectangleF plotRect, RectangleF? sideBandRect, AxisPresentationGeometry axis, double endpointInset)
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
                g.SetClip(sideBandRect.Value, CombineMode.Intersect);
            }

            var ticks = axis.Ticks;
            const int Step = 1;
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var deviceY = DomainToDeviceY(tick.Value, domainMin, domainMax, plotRect);
                    var start = MapVerticalTickPointToDevice(tick.Start, domainMin, domainMax, plotRect, axisX);
                    var end = MapVerticalTickPointToDevice(tick.End, domainMin, domainMax, plotRect, axisX);
                    g.DrawLine(AxisPen, start, end);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, Step))
                    {
                        var label = FitLabelToWidth(g, tick.Label, TickFont, sideBandRect.HasValue ? sideBandRect.Value.Width : float.MaxValue);
                        if (string.IsNullOrEmpty(label))
                        {
                            continue;
                        }

                        var labelSize = g.MeasureString(label, TickFont);
                        var x = Math.Max(start.X, end.X) + TickLabelOffset;
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

        private static void RenderTopAxis(Graphics g, RectangleF plotRect, RectangleF? sideBandRect, AxisPresentationGeometry axis, double endpointInset)
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
                g.SetClip(sideBandRect.Value, CombineMode.Intersect);
            }

            var ticks = axis.Ticks;
            var step = ComputeTickLabelStep(g, ticks, sideBandRect, AxisSide.Top);
            try
            {
                for (var i = 0; i < ticks.Count; i++)
                {
                    var tick = ticks[i];
                    var start = MapHorizontalTickPointToDevice(tick.Start, domainMin, domainMax, plotRect, axisY);
                    var end = MapHorizontalTickPointToDevice(tick.End, domainMin, domainMax, plotRect, axisY);
                    var deviceX = DomainToDeviceX(tick.Value, domainMin, domainMax, plotRect);
                    g.DrawLine(AxisPen, start, end);

                    if (!string.IsNullOrEmpty(tick.Label) && ShouldRenderTickLabel(i, step))
                    {
                        if (i == ticks.Count - 1)
                        {
                            continue;
                        }

                        var labelSize = g.MeasureString(tick.Label, TickFont);
                        var x = deviceX - labelSize.Width / 2f;
                        var y = Math.Min(start.Y, end.Y) - TickLabelOffset - TickFont.Height;
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

        private static RectangleF? FindAxisTickLabelRegionRect(Rectangle deviceBounds, IReadOnlyList<AxisTitleBandGeometry> bands, AxisSide side)
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

        private static void RenderAxisTitles(Graphics g, Rectangle deviceBounds, IReadOnlyList<AxisTitleBandGeometry> bands)
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

                    var itemRect = ComputeDeviceBoundsForGeometry(deviceBounds, item.AxisTitleRegion.BottomLeft, item.AxisTitleRegion.TopRight);

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
            g.SetClip(contentRect, CombineMode.Intersect);
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

            using (var glyphPen = new Pen(ToDrawingColor(entry.SeriesColor), LegendLineWidth))
            {
                var markerRadius = LegendMarkerSize / 2f;
                var markerCenterX = glyphLeft + (glyphWidth / 2f);

                if (entry.GlyphKind == LegendGlyphKind.Line || entry.GlyphKind == LegendGlyphKind.LineAndPoint)
                {
                    g.DrawLine(glyphPen, glyphLeft, glyphCenterY, glyphRight, glyphCenterY);
                }

                if (entry.GlyphKind == LegendGlyphKind.Point || entry.GlyphKind == LegendGlyphKind.LineAndPoint)
                {
                    g.DrawEllipse(glyphPen, markerCenterX - markerRadius, glyphCenterY - markerRadius, LegendMarkerSize, LegendMarkerSize);
                }
            }

            if (string.IsNullOrWhiteSpace(entry.DisplayText))
            {
                return;
            }

            var textSize = g.MeasureString(entry.DisplayText, LegendFont);
            var textX = glyphRight + LegendTextOffset;
            var textY = entryRect.Top + ((entryRect.Height - textSize.Height) / 2f);
            g.DrawString(entry.DisplayText, LegendFont, TickLabelBrush, textX, textY);
        }

        private static void RenderSeries(Graphics g, RectangleF plotRect, GraphPresentationModel model)
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

                var xMin = xAxis.MinimumValue.Value;
                var xMax = xAxis.MaximumValue.Value;
                if (xMin >= xMax)
                {
                    continue;
                }

                var yMin = yAxis.MinimumValue.Value;
                var yMax = yAxis.MaximumValue.Value;
                if (yMin >= yMax)
                {
                    continue;
                }

                var seriesRect = ComputeSeriesRect(plotRect, xAxisEntry, yAxisEntry);
                RenderOneSeries(g, seriesRect, s, xMin, xMax, yMin, yMax);
            }
        }

        private static void RenderOneSeries(Graphics g, RectangleF seriesRect, SeriesPresentationGeometry series, double xMin, double xMax, double yMin, double yMax)
        {
            var points = series.Points;
            if (points == null || points.Count < 1)
            {
                return;
            }

            var clip = g.ClipBounds;
            g.SetClip(seriesRect, CombineMode.Intersect);

            using (var seriesPen = new Pen(ToDrawingColor(series.SeriesColor), SeriesLineWidth))
            using (var seriesBrush = new SolidBrush(ToDrawingColor(series.SeriesColor)))
            {
                try
                {
                    if (series.ConnectivityIntent == SeriesConnectivityIntent.Discrete)
                    {
                        for (var i = 0; i < points.Count; i++)
                        {
                            var domainPoint = points[i];
                            var deviceX = DomainToDeviceX(domainPoint.X, xMin, xMax, seriesRect);
                            var deviceY = DomainToDeviceY(domainPoint.Y, yMin, yMax, seriesRect);
                            g.FillEllipse(seriesBrush, deviceX - DiscretePointMarkerRadius, deviceY - DiscretePointMarkerRadius, DiscretePointMarkerSize, DiscretePointMarkerSize);
                        }
                    }
                    else
                    {
                        PointF? previous = null;

                        for (var i = 0; i < points.Count; i++)
                        {
                            var domainPoint = points[i];
                            var deviceX = DomainToDeviceX(domainPoint.X, xMin, xMax, seriesRect);
                            var deviceY = DomainToDeviceY(domainPoint.Y, yMin, yMax, seriesRect);
                            var current = new PointF(deviceX, deviceY);

                            if (previous.HasValue)
                            {
                                g.DrawLine(seriesPen, previous.Value, current);
                            }

                            previous = current;
                        }
                    }
                }
                finally
                {
                    g.SetClip(clip);
                }
            }
        }

        private static Color ToDrawingColor(GraphColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private static Graphics TryResolveGraphics(IGraphRenderContext context)
        {
            return (context as WinFormsGraphRenderContext)?.Graphics;
        }

        protected override IGraphDrawingSurface TryCreateDrawingSurface(IGraphRenderContext context, Rectangle deviceBounds)
        {
            var g = TryResolveGraphics(context);
            if (g == null)
            {
                return null;
            }

            return new WinFormsGraphDrawingSurface(g, deviceBounds);
        }

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

            var plotHeight = insetRect.Height;
            var top = insetRect.Bottom - (float)(spanEnd * plotHeight);
            var bottom = insetRect.Bottom - (float)(spanStart * plotHeight);
            return RectangleF.FromLTRB(insetRect.Left, top, insetRect.Right, bottom);
        }

        private static RectangleF ComputeSeriesRect(RectangleF plotRect, AxisLayoutEntry xAxisEntry, AxisLayoutEntry yAxisEntry)
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

        private static RectangleF ComputeDeviceBoundsForGeometry(Rectangle deviceBounds, GeometryPoint3D bottomLeft, GeometryPoint3D topRight)
        {
            var left = deviceBounds.Left + bottomLeft.X * deviceBounds.Width;
            var right = deviceBounds.Left + topRight.X * deviceBounds.Width;
            var top = deviceBounds.Bottom - topRight.Y * deviceBounds.Height;
            var bottom = deviceBounds.Bottom - bottomLeft.Y * deviceBounds.Height;
            return RectangleF.FromLTRB((float)left, (float)top, (float)right, (float)bottom);
        }

        private static float DomainToDeviceX(double domainValue, double domainMin, double domainMax, RectangleF plotRect)
        {
            var range = domainMax - domainMin;
            if (Math.Abs(range) < double.Epsilon)
            {
                return plotRect.Left + plotRect.Width / 2f;
            }

            var t = (domainValue - domainMin) / range;
            return plotRect.Left + (float)(t * plotRect.Width);
        }

        private static float DomainToDeviceY(double domainValue, double domainMin, double domainMax, RectangleF plotRect)
        {
            var range = domainMax - domainMin;
            if (Math.Abs(range) < double.Epsilon)
            {
                return plotRect.Top + plotRect.Height / 2f;
            }

            var t = (domainValue - domainMin) / range;
            return plotRect.Bottom - (float)(t * plotRect.Height);
        }

        private static PointF MapHorizontalTickPointToDevice(GeometryPoint3D point, double domainMin, double domainMax, RectangleF axisRect, float axisY)
        {
            var x = DomainToDeviceX(point.X, domainMin, domainMax, axisRect);
            var y = axisY + (float)(point.Y * axisRect.Height);
            return new PointF(x, y);
        }

        private static PointF MapVerticalTickPointToDevice(GeometryPoint3D point, double domainMin, double domainMax, RectangleF axisRect, float axisX)
        {
            var x = axisX + (float)(point.X * axisRect.Width);
            var y = DomainToDeviceY(point.Y, domainMin, domainMax, axisRect);
            return new PointF(x, y);
        }

        private static int ComputeTickLabelStep(Graphics g, IReadOnlyList<AxisTickPresentation> ticks, RectangleF? tickLabelRegion, AxisSide side)
        {
            if (!tickLabelRegion.HasValue)
            {
                return 1;
            }

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

        private static void DrawRotatedCenteredText(Graphics g, string text, Font font, Brush brush, float centerX, float centerY, float angle)
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
    }
}