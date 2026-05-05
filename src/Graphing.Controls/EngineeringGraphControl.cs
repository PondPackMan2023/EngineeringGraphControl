using Graphing.Controls.Models;
using Graphing.Controls.Interaction;
using Graphing.Controls.Models.Series;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using Graphing.Controls.Rendering.Geometry;
using Graphing.Controls.Snapshot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Graphing.Controls
{
    public class EngineeringGraphControl : UserControl
    {
        private const float AnimationBarLineWidth = 3f;
        private const float AnimationBarHitTolerancePixels = 6f;
        private const float AnimationBarMarkerRadius = 5f;
        private static readonly Color DefaultAnimationBarColor = Color.OrangeRed;

        private readonly object _snapshotSync = new object();
        private readonly WinFormsGraphRenderer _winFormsRenderer = new WinFormsGraphRenderer();
        private readonly IGraphRenderer _renderer;

        private IGraphModel _graphModel;
        private IGraphSnapshot _activeSnapshot;
        private GraphPresentationModel _activePresentation;
        private GraphPresentationOptions _activePresentationOptions;
        private bool _animationBarEnabled;
        private int _animationBarXIndex;
        private bool _hasAnimationBarXIndex;
        private Color _animationBarColor = DefaultAnimationBarColor;
        private bool _isAnimationBarDragging;

        public event EventHandler<AxisInteractionMouseEventArgs> AxisMouseDown;

        public event EventHandler<AxisInteractionMouseEventArgs> AxisMouseUp;

        public event EventHandler<AxisInteractionMouseEventArgs> AxisContextRequested;

        public event EventHandler<AnimationBarIndexChangedEventArgs> AnimationBarXIndexChanged;

        public EngineeringGraphControl()
        {
            _renderer = _winFormsRenderer;
            DoubleBuffered = true;
        }

        public IGraphSnapshot ActiveSnapshot
        {
            get
            {
                lock (_snapshotSync)
                {
                    return _activeSnapshot;
                }
            }
        }

        public GraphPresentationModel ActivePresentation
        {
            get
            {
                lock (_snapshotSync)
                {
                    return _activePresentation;
                }
            }
        }

        public GraphPresentationOptions ActiveOptions
        {
            get
            {
                lock (_activePresentationOptions)
                {
                    return _activePresentationOptions;
                }
            }
        }

        public IGraphModel GraphModel => _graphModel;

        public bool AnimationBarEnabled
        {
            get => _animationBarEnabled;
            set
            {
                if (_animationBarEnabled == value)
                {
                    return;
                }

                _animationBarEnabled = value;
                Invalidate();
            }
        }

        public int AnimationBarXIndex
        {
            get => _animationBarXIndex;
            set => SetAnimationBarXIndex(value, isUserInitiated: false);
        }

        public Color AnimationBarColor
        {
            get => _animationBarColor;
            set
            {
                if (_animationBarColor.ToArgb() == value.ToArgb())
                {
                    return;
                }

                _animationBarColor = value;
                Invalidate();
            }
        }

        public void SetGraphSource(IGraphModel graphModel, GraphPresentationOptions options = null)
        {
            lock (_snapshotSync)
            {
                var snapshotBuilder = new GraphSnapshotBuilder();
                _graphModel = graphModel;
                options = GraphPresentationOptions.EnsureSeriesStyles(graphModel, options);
                var nextSnapshot = graphModel == null
                    ? null
                    : snapshotBuilder.Build(graphModel, options);

                TryInstallSnapshotAndPresentation(nextSnapshot, options);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            GraphPresentationModel presentation;
            GraphPresentationOptions options;
            IGraphSnapshot snapshot;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
                options = _activePresentationOptions;
                snapshot = _activeSnapshot;
            }

            if (snapshot != null)
            {
                var measurementInput = _renderer.CreateMeasurementInput(e.Graphics, ClientRectangle);
                presentation = CreatePresentationModel(snapshot, options, measurementInput);
                lock (_snapshotSync)
                {
                    _activePresentation = presentation;
                }
            }

            if (presentation != null)
            {
                var renderedSeriesGeometries = new List<RenderedSeriesPolyline>();
                _winFormsRenderer.SeriesGeometryRendered = renderedSeriesGeometries.Add;

                try
                {
                    _renderer.Render(e.Graphics, ClientRectangle, presentation, options);
                }
                finally
                {
                    _winFormsRenderer.SeriesGeometryRendered = null;
                }

                RenderAnimationBarOverlay(e.Graphics, ClientRectangle, presentation, renderedSeriesGeometries);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            UpdateAnimationBarCursor(e);

            if (TryHandleAnimationBarMouseMove(e))
            {
                return;
            }

            // Phase H4 intentionally does not track hover state; this probe keeps
            // the mouse-to-presentation bridge in place for future hover phases.
            _ = TryResolveAxisInteraction(e, out _, out _, out _);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (TryHandleAnimationBarMouseDown(e))
            {
                return;
            }

            if (!TryResolveAxisInteraction(e, out var descriptor, out var clientPosition, out var graphPosition) || descriptor == null)
            {
                return;
            }

            AxisMouseDown?.Invoke(
                this,
                new AxisInteractionMouseEventArgs(
                    descriptor,
                    e.Button,
                    ModifierKeys,
                    clientPosition,
                    graphPosition));
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (TryHandleAnimationBarMouseUp(e))
            {
                return;
            }

            if (!TryResolveAxisInteraction(e, out var descriptor, out var clientPosition, out var graphPosition) || descriptor == null)
            {
                return;
            }

            var args = new AxisInteractionMouseEventArgs(
                descriptor,
                e.Button,
                ModifierKeys,
                clientPosition,
                graphPosition);

            AxisMouseUp?.Invoke(this, args);

            if (e.Button == MouseButtons.Right)
            {
                AxisContextRequested?.Invoke(this, args);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor = Cursors.Default;
        }

        protected virtual GraphPresentationModel CreatePresentationModel(
            IGraphSnapshot snapshot,
            GraphPresentationOptions options = null,
            IGraphLayoutMeasurementInput measurementInput = null)
        {
            return new GraphPresentationModel(snapshot, options, measurementInput);
        }

        protected virtual AxisInteractionDescriptor ResolveAxisInteractionDescriptor(
            GraphPresentationModel presentation,
            GeometryPoint3D graphPosition)
        {
            return presentation.ResolveAxisInteraction(graphPosition);
        }

        private bool TryResolveAxisInteraction(
            MouseEventArgs mouseEvent,
            out AxisInteractionDescriptor descriptor,
            out System.Drawing.Point clientPosition,
            out GeometryPoint3D graphPosition)
        {
            descriptor = null;
            clientPosition = System.Drawing.Point.Empty;
            graphPosition = new GeometryPoint3D(0d, 0d, 0d);

            var clientBounds = ClientRectangle;
            if (clientBounds.Width <= 0 || clientBounds.Height <= 0)
            {
                return false;
            }

            clientPosition = NormalizeMouseToClientPosition(mouseEvent.Location, clientBounds);
            graphPosition = ToGraphPosition(clientBounds, clientPosition);

            GraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            if (presentation == null)
            {
                return false;
            }

            descriptor = ResolveAxisInteractionDescriptor(presentation, graphPosition);
            return true;
        }

        private System.Drawing.Point NormalizeMouseToClientPosition(
            System.Drawing.Point inputPosition,
            System.Drawing.Rectangle clientBounds)
        {
            //if (clientBounds.Contains(inputPosition))
            //{
            //    return inputPosition;
            //}

            var hostForm = FindForm();
            if (hostForm == null)
            {
                return inputPosition;
            }

            // Some input paths provide coordinates in host form client space;
            // translate those to this control's client space before normalization.
            var screenPosition = hostForm.PointToScreen(inputPosition);
            return PointToClient(screenPosition);
        }

        private static GeometryPoint3D ToGraphPosition(System.Drawing.Rectangle clientBounds, System.Drawing.Point clientPosition)
        {
            var normalizedX = (clientPosition.X - clientBounds.Left) / (double)clientBounds.Width;
            var normalizedY = (clientBounds.Bottom - clientPosition.Y) / (double)clientBounds.Height;
            return new GeometryPoint3D(normalizedX, normalizedY, 0d);
        }

        private void TryInstallSnapshotAndPresentation(IGraphSnapshot nextSnapshot,
            GraphPresentationOptions options = null)
        {
            if (ReferenceEquals(_activeSnapshot, nextSnapshot))
            {
                return;
            }

            if (nextSnapshot == null)
            {
                _activeSnapshot = null;
                _activePresentation = null;
                _activePresentationOptions = null;
                return;
            }

            GraphPresentationModel nextPresentation;
            try
            {
                nextPresentation = CreatePresentationModel(nextSnapshot, options);
            }
            catch
            {
                return;
            }

            _activeSnapshot = nextSnapshot;
            _activePresentation = nextPresentation;
            _activePresentationOptions = options;
            Invalidate();
        }

        private void SetAnimationBarXIndex(int xIndex, bool isUserInitiated)
        {
            if (xIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(xIndex));
            }

            if (_hasAnimationBarXIndex && _animationBarXIndex == xIndex)
            {
                return;
            }

            var previousXIndex = _hasAnimationBarXIndex ? (int?)_animationBarXIndex : null;
            _animationBarXIndex = xIndex;
            _hasAnimationBarXIndex = true;

            AnimationBarXIndexChanged?.Invoke(
                this,
                new AnimationBarIndexChangedEventArgs(xIndex, previousXIndex, isUserInitiated));

            Invalidate();
        }

        private bool TryHandleAnimationBarMouseDown(MouseEventArgs mouseEvent)
        {
            if (mouseEvent == null || mouseEvent.Button != MouseButtons.Left || !AnimationBarEnabled)
            {
                return false;
            }

            var clientBounds = ClientRectangle;
            if (clientBounds.Width <= 0 || clientBounds.Height <= 0)
            {
                return false;
            }

            GraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            if (presentation == null)
            {
                return false;
            }

            if (!TryComputeAnimationBarPlotRect(clientBounds, presentation, out var plotRect))
            {
                return false;
            }

            var isOnBar = HitTestAnimationBar(mouseEvent.Location, clientBounds, presentation, plotRect);
            if (isOnBar)
            {
                _isAnimationBarDragging = true;
                Capture = true;
                Cursor = Cursors.SizeAll;
                return true;
            }
            return false;
        }

        private bool TryHandleAnimationBarMouseMove(MouseEventArgs mouseEvent)
        {
            if (!_isAnimationBarDragging || mouseEvent == null || !AnimationBarEnabled)
            {
                return false;
            }

            var clientBounds = ClientRectangle;
            if (clientBounds.Width <= 0 || clientBounds.Height <= 0)
            {
                return false;
            }

            GraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            if (presentation == null)
            {
                return false;
            }

            if (TryResolveNearestAnimationBarXIndex(mouseEvent.Location.X, clientBounds, presentation, out var snappedXIndex))
            {
                SetAnimationBarXIndex(snappedXIndex, isUserInitiated: true);
            }

            Cursor = Cursors.SizeAll;

            return true;
        }

        private bool TryHandleAnimationBarMouseUp(MouseEventArgs mouseEvent)
        {
            if (mouseEvent == null || mouseEvent.Button != MouseButtons.Left)
            {
                return false;
            }

            if (!_isAnimationBarDragging)
            {
                return false;
            }

            _isAnimationBarDragging = false;
            Capture = false;
            UpdateAnimationBarCursor(mouseEvent);
            return true;
        }

        private void UpdateAnimationBarCursor(MouseEventArgs mouseEvent)
        {
            if (_isAnimationBarDragging)
            {
                Cursor = Cursors.SizeAll;
                return;
            }

            if (mouseEvent == null || !AnimationBarEnabled)
            {
                Cursor = Cursors.Default;
                return;
            }

            var clientBounds = ClientRectangle;
            if (clientBounds.Width <= 0 || clientBounds.Height <= 0)
            {
                Cursor = Cursors.Default;
                return;
            }

            GraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            if (presentation == null || !TryComputeAnimationBarPlotRect(clientBounds, presentation, out var plotRect))
            {
                Cursor = Cursors.Default;
                return;
            }

            Cursor = HitTestAnimationBar(mouseEvent.Location, clientBounds, presentation, plotRect)
                ? Cursors.SizeAll
                : Cursors.Default;
        }

        private void RenderAnimationBarOverlay(
            Graphics graphics,
            Rectangle clientBounds,
            GraphPresentationModel presentation,
            IReadOnlyList<RenderedSeriesPolyline> renderedSeriesGeometries)
        {
            if (!AnimationBarEnabled || !_hasAnimationBarXIndex || graphics == null || presentation == null)
            {
                return;
            }

            if (!TryResolveAnimationBarAbstractX(presentation, _animationBarXIndex, out var abstractX))
            {
                return;
            }

            if (!TryComputeAnimationBarPlotRect(clientBounds, presentation, out var plotRect))
            {
                return;
            }

            var deviceX = AbstractToDeviceX(clientBounds, abstractX);
            var clip = graphics.ClipBounds;
            graphics.SetClip(plotRect, CombineMode.Intersect);

            try
            {
                using (var pen = new Pen(AnimationBarColor, AnimationBarLineWidth))
                {
                    graphics.DrawLine(pen, deviceX, plotRect.Top, deviceX, plotRect.Bottom);
                }

                RenderIntersectionMarkers(graphics, plotRect, deviceX, renderedSeriesGeometries);
            }
            finally
            {
                graphics.SetClip(clip);
            }
        }

        private static void RenderIntersectionMarkers(
            Graphics graphics,
            RectangleF plotRect,
            float animationBarDeviceX,
            IReadOnlyList<RenderedSeriesPolyline> renderedSeriesGeometries)
        {
            if (renderedSeriesGeometries == null || renderedSeriesGeometries.Count == 0)
            {
                return;
            }

            var seenSeriesIds = new HashSet<SeriesId>();

            for (var i = 0; i < renderedSeriesGeometries.Count; i++)
            {
                var seriesGeometry = renderedSeriesGeometries[i];
                if (seriesGeometry == null || !IsEligibleForIntersectionMarker(seriesGeometry.SeriesType))
                {
                    continue;
                }

                if (!seenSeriesIds.Add(seriesGeometry.SeriesId))
                {
                    continue;
                }

                if (!TryResolveVerticalPolylineIntersection(animationBarDeviceX, seriesGeometry.DevicePoints, out var deviceY))
                {
                    continue;
                }

                var markerRect = new RectangleF(
                    animationBarDeviceX - AnimationBarMarkerRadius,
                    deviceY - AnimationBarMarkerRadius,
                    AnimationBarMarkerRadius * 2f,
                    AnimationBarMarkerRadius * 2f);

                using (var brush = new SolidBrush(seriesGeometry.SeriesColor))
                {
                    graphics.FillEllipse(brush, markerRect);
                }
            }
        }

        private static bool IsEligibleForIntersectionMarker(SeriesType seriesType)
        {
            return seriesType == SeriesType.Line || seriesType == SeriesType.Scatter;
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

        private bool HitTestAnimationBar(
            Point mouseClientPosition,
            Rectangle clientBounds,
            GraphPresentationModel presentation,
            RectangleF plotRect)
        {
            if (!plotRect.Contains(mouseClientPosition))
            {
                return false;
            }

            if (!TryResolveAnimationBarAbstractX(presentation, _animationBarXIndex, out var abstractX))
            {
                return false;
            }

            var barDeviceX = AbstractToDeviceX(clientBounds, abstractX);
            return Math.Abs(mouseClientPosition.X - barDeviceX) <= AnimationBarHitTolerancePixels;
        }

        private bool TryResolveNearestAnimationBarXIndex(
            int mouseClientX,
            Rectangle clientBounds,
            GraphPresentationModel presentation,
            out int nearestXIndex)
        {
            nearestXIndex = 0;

            if (!TryComputeAnimationBarPlotRect(clientBounds, presentation, out var plotRect))
            {
                return false;
            }

            if (!TryGetAnimationBarSeriesData(presentation, out var points, out var xMin, out var xMax))
            {
                return false;
            }

            var clampedMouseX = mouseClientX;
            if (clampedMouseX < plotRect.Left)
            {
                clampedMouseX = (int)Math.Round(plotRect.Left, MidpointRounding.AwayFromZero);
            }
            else if (clampedMouseX > plotRect.Right)
            {
                clampedMouseX = (int)Math.Round(plotRect.Right, MidpointRounding.AwayFromZero);
            }

            var abstractX = ToGraphPosition(clientBounds, new Point(clampedMouseX, clientBounds.Bottom)).X;
            var plotLeft = presentation.Layout.PlotArea.BottomLeft.X;
            var plotRight = presentation.Layout.PlotArea.TopRight.X;
            var plotSpan = plotRight - plotLeft;
            if (plotSpan <= 0d)
            {
                return false;
            }

            var normalizedXInPlot = (abstractX - plotLeft) / plotSpan;
            if (normalizedXInPlot < 0d)
            {
                normalizedXInPlot = 0d;
            }
            else if (normalizedXInPlot > 1d)
            {
                normalizedXInPlot = 1d;
            }

            var domainX = xMin + (normalizedXInPlot * (xMax - xMin));
            nearestXIndex = ResolveNearestXIndex(points, domainX);
            return true;
        }

        private static bool TryResolveAnimationBarAbstractX(
            GraphPresentationModel presentation,
            int xIndex,
            out double abstractX)
        {
            abstractX = 0d;

            if (!TryGetAnimationBarSeriesData(presentation, out var points, out var xMin, out var xMax))
            {
                return false;
            }

            if (xIndex < 0 || xIndex >= points.Count)
            {
                return false;
            }

            var xRange = xMax - xMin;
            if (xRange <= 0d)
            {
                return false;
            }

            var xValue = points[xIndex].X;
            var normalized = (xValue - xMin) / xRange;
            if (normalized < 0d)
            {
                normalized = 0d;
            }
            else if (normalized > 1d)
            {
                normalized = 1d;
            }

            var plotArea = presentation.Layout.PlotArea;
            abstractX = plotArea.BottomLeft.X + (normalized * (plotArea.TopRight.X - plotArea.BottomLeft.X));
            return true;
        }

        private static bool TryGetAnimationBarSeriesData(
            GraphPresentationModel presentation,
            out System.Collections.Generic.IReadOnlyList<GeometryPoint3D> points,
            out double xMin,
            out double xMax)
        {
            points = null;
            xMin = 0d;
            xMax = 0d;

            var series = presentation.Layout.Series;
            if (series == null || series.Count == 0)
            {
                return false;
            }

            for (var i = 0; i < series.Count; i++)
            {
                var candidate = series[i];
                var xAxisEntry = candidate?.XAxisEntry;
                var candidatePoints = candidate?.Points;

                if (xAxisEntry == null || candidatePoints == null || candidatePoints.Count == 0)
                {
                    continue;
                }

                var xAxis = xAxisEntry.Axis;
                if (xAxis == null || !xAxis.MinimumValue.HasValue || !xAxis.MaximumValue.HasValue)
                {
                    continue;
                }

                xMin = xAxis.MinimumValue.Value;
                xMax = xAxis.MaximumValue.Value;
                points = candidatePoints;
                return true;
            }

            return false;
        }

        private static int ResolveNearestXIndex(
            System.Collections.Generic.IReadOnlyList<GeometryPoint3D> points,
            double domainX)
        {
            var nearestIndex = 0;
            var nearestDistance = double.MaxValue;

            for (var i = 0; i < points.Count; i++)
            {
                var distance = Math.Abs(points[i].X - domainX);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        private static bool TryComputeAnimationBarPlotRect(
            Rectangle clientBounds,
            GraphPresentationModel presentation,
            out RectangleF plotRect)
        {
            plotRect = ComputeDevicePlotRect(clientBounds, presentation.Layout.PlotArea);
            return plotRect.Width > 0f && plotRect.Height > 0f;
        }

        private static RectangleF ComputeDevicePlotRect(Rectangle clientBounds, PlotAreaLayout plotArea)
        {
            var left = clientBounds.Left + plotArea.BottomLeft.X * clientBounds.Width;
            var right = clientBounds.Left + plotArea.TopRight.X * clientBounds.Width;
            var top = clientBounds.Bottom - plotArea.TopRight.Y * clientBounds.Height;
            var bottom = clientBounds.Bottom - plotArea.BottomLeft.Y * clientBounds.Height;

            return RectangleF.FromLTRB((float)left, (float)top, (float)right, (float)bottom);
        }

        private static float AbstractToDeviceX(Rectangle clientBounds, double abstractX)
        {
            return (float)(clientBounds.Left + (abstractX * clientBounds.Width));
        }

    }
}
