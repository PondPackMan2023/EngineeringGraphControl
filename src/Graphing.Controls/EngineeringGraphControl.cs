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
        private float[] _renderedAnimationBarXSamples = Array.Empty<float>();
        private float[] _renderedPointSeriesCenterXSamples = Array.Empty<float>();
        private float _renderedAnimationBarMinX;
        private float _renderedAnimationBarMaxX;
        private bool _hasRenderedAnimationBarXExtent;
        private readonly Dictionary<string, AxisExtent> _defaultAxisExtents = new Dictionary<string, AxisExtent>(StringComparer.Ordinal);
        private bool _zoomEnabled;
        private ZoomGestureKind _lastZoomGesture;
        private bool _isZoomDragging;
        private Point _zoomDragAnchorClient;
        private Point _zoomDragCurrentClient;
        private RectangleF? _zoomDragRectangle;

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

        public bool ZoomEnabled
        {
            get => _zoomEnabled;
            set
            {
                if (_zoomEnabled == value)
                {
                    return;
                }

                _zoomEnabled = value;

                if (!_zoomEnabled)
                {
                    ClearZoomDragOverlay();
                    Capture = false;
                }

                UpdateZoomModeCursor();
                Invalidate();
            }
        }

        internal bool ZoomDragOverlayVisible => _zoomEnabled && _isZoomDragging && _zoomDragRectangle.HasValue;

        internal RectangleF? ZoomDragOverlayBounds => _zoomDragRectangle;

        internal ZoomGestureKind LastZoomGesture => _lastZoomGesture;

        public void ZoomExtents()
        {
            IGraphModel graphModel;
            GraphPresentationOptions activeOptions;
            Dictionary<string, AxisExtent> defaultAxisExtents;

            lock (_snapshotSync)
            {
                graphModel = _graphModel;
                activeOptions = _activePresentationOptions;
                defaultAxisExtents = new Dictionary<string, AxisExtent>(_defaultAxisExtents, StringComparer.Ordinal);
            }

            if (graphModel == null)
            {
                Invalidate();
                return;
            }

            var zoomResetOptions = CreateZoomResetOptions(activeOptions, graphModel, defaultAxisExtents);
            SetGraphSource(graphModel, zoomResetOptions);
            Invalidate();
        }

        public void SetGraphSource(IGraphModel graphModel, GraphPresentationOptions options = null)
        {
            lock (_snapshotSync)
            {
                var snapshotBuilder = new GraphSnapshotBuilder();
                var isNewGraphLifecycle = !ReferenceEquals(_graphModel, graphModel);
                _graphModel = graphModel;
                options = GraphPresentationOptions.EnsureSeriesStyles(graphModel, options);
                var nextSnapshot = graphModel == null
                    ? null
                    : snapshotBuilder.Build(graphModel, options);

                if (graphModel == null)
                {
                    _defaultAxisExtents.Clear();
                }
                else if (isNewGraphLifecycle || _defaultAxisExtents.Count == 0)
                {
                    CaptureDefaultAxisExtents(nextSnapshot);
                }

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

                UpdateRenderedAnimationBarXExtent(renderedSeriesGeometries);

                RenderAnimationBarOverlay(e.Graphics, ClientRectangle, presentation, renderedSeriesGeometries);
                RenderZoomDragOverlay(e.Graphics, ClientRectangle, presentation);
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

            if (ZoomEnabled)
            {
                _ = TryHandleZoomMouseMove(e);
                return;
            }

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

            if (ZoomEnabled)
            {
                _ = TryHandleZoomMouseDown(e);
                return;
            }

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

            if (ZoomEnabled)
            {
                _ = TryHandleZoomMouseUp(e);
                return;
            }

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
            UpdateZoomModeCursor();
            if (!ZoomEnabled)
            {
                Cursor = Cursors.Default;
            }
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

        private bool TryHandleZoomMouseDown(MouseEventArgs mouseEvent)
        {
            if (mouseEvent == null || mouseEvent.Button != MouseButtons.Left)
            {
                return false;
            }

            if (!TryGetCurrentPlotRect(out var plotRect))
            {
                return false;
            }

            _zoomDragAnchorClient = ClampPointToRectangle(mouseEvent.Location, plotRect);
            _zoomDragCurrentClient = _zoomDragAnchorClient;
            _zoomDragRectangle = BuildZoomRectangle(_zoomDragAnchorClient, _zoomDragCurrentClient);
            _isZoomDragging = true;
            Capture = true;
            UpdateZoomModeCursor();
            Invalidate();
            return true;
        }

        private bool TryHandleZoomMouseMove(MouseEventArgs mouseEvent)
        {
            UpdateZoomModeCursor();

            if (!_isZoomDragging || mouseEvent == null)
            {
                return false;
            }

            if (!TryGetCurrentPlotRect(out var plotRect))
            {
                return false;
            }

            _zoomDragCurrentClient = ClampPointToRectangle(mouseEvent.Location, plotRect);
            _zoomDragRectangle = BuildZoomRectangle(_zoomDragAnchorClient, _zoomDragCurrentClient);
            Invalidate();
            return true;
        }

        private bool TryHandleZoomMouseUp(MouseEventArgs mouseEvent)
        {
            UpdateZoomModeCursor();

            if (mouseEvent == null || mouseEvent.Button != MouseButtons.Left)
            {
                return false;
            }

            if (!_isZoomDragging)
            {
                return false;
            }

            int dx = _zoomDragCurrentClient.X - _zoomDragAnchorClient.X;
            int dy = _zoomDragCurrentClient.Y - _zoomDragAnchorClient.Y;

            _isZoomDragging = false;
            _zoomDragRectangle = null;
            Capture = false;
            Invalidate();

            if (dx > 0 && dy > 0)
            {
                _lastZoomGesture = ZoomGestureKind.ZoomIn;
            }
            else if (dx < 0 && dy < 0)
            {
                _lastZoomGesture = ZoomGestureKind.ZoomReset;
                ZoomExtents();
            }
            else
            {
                _lastZoomGesture = ZoomGestureKind.None;
            }

            return true;
        }

        private void ClearZoomDragOverlay()
        {
            _isZoomDragging = false;
            _zoomDragRectangle = null;
        }

        private void UpdateZoomModeCursor()
        {
            Cursor = ZoomEnabled ? Cursors.Cross : Cursors.Default;
        }

        private bool TryGetCurrentPlotRect(out RectangleF plotRect)
        {
            plotRect = RectangleF.Empty;

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

            return TryComputeAnimationBarPlotRect(clientBounds, presentation, out plotRect);
        }

        private static Point ClampPointToRectangle(Point point, RectangleF rectangle)
        {
            if (rectangle.Width <= 0f || rectangle.Height <= 0f)
            {
                return point;
            }

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

        private void RenderZoomDragOverlay(
            Graphics graphics,
            Rectangle clientBounds,
            GraphPresentationModel presentation)
        {
            if (!ZoomEnabled || !_isZoomDragging || !_zoomDragRectangle.HasValue || graphics == null || presentation == null)
            {
                return;
            }

            if (!TryComputeAnimationBarPlotRect(clientBounds, presentation, out var plotRect))
            {
                return;
            }

            var dragRect = _zoomDragRectangle.Value;
            if (dragRect.Width <= 0f || dragRect.Height <= 0f)
            {
                return;
            }

            var clip = graphics.ClipBounds;
            graphics.SetClip(plotRect, CombineMode.Intersect);

            try
            {
                using (var pen = new Pen(Color.Black, 1f))
                {
                    pen.DashStyle = DashStyle.Dot;
                    graphics.DrawRectangle(pen, dragRect.X, dragRect.Y, dragRect.Width, dragRect.Height);
                }
            }
            finally
            {
                graphics.SetClip(clip);
            }
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

            if (!TryComputeAnimationBarPlotRect(clientBounds, presentation, out var plotRect))
            {
                return;
            }

            if (!TryResolveAnimationBarDeviceX(clientBounds, presentation, out var deviceX))
            {
                return;
            }

            deviceX = ClampAnimationBarDeviceXToRenderedExtent(deviceX);
            deviceX = ResolvePointSeriesSnappedBarX(deviceX);
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

                var markerX = animationBarDeviceX;
                var markerY = 0f;

                if (seriesGeometry.SeriesType == SeriesType.Scatter)
                {
                    if (!TryResolvePointSeriesIntersectionCenter(
                            animationBarDeviceX,
                            seriesGeometry.DevicePoints,
                            AnimationBarHitTolerancePixels,
                            out var centerPoint))
                    {
                        continue;
                    }

                    markerX = centerPoint.X;
                    markerY = centerPoint.Y;
                }
                else if (!TryResolveVerticalPolylineIntersection(animationBarDeviceX, seriesGeometry.DevicePoints, out markerY))
                {
                    continue;
                }

                var markerRect = new RectangleF(
                    markerX - AnimationBarMarkerRadius,
                    markerY - AnimationBarMarkerRadius,
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

            if (!TryResolveAnimationBarDeviceX(clientBounds, presentation, out var barDeviceX))
            {
                return false;
            }

            barDeviceX = ClampAnimationBarDeviceXToRenderedExtent(barDeviceX);
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

            if (_renderedAnimationBarXSamples == null || _renderedAnimationBarXSamples.Length == 0)
            {
                return false;
            }

            var clampedMouseX = mouseClientX;
            var minX = _hasRenderedAnimationBarXExtent ? _renderedAnimationBarMinX : plotRect.Left;
            var maxX = _hasRenderedAnimationBarXExtent ? _renderedAnimationBarMaxX : plotRect.Right;

            if (clampedMouseX < minX)
            {
                clampedMouseX = (int)Math.Round(minX, MidpointRounding.AwayFromZero);
            }
            else if (clampedMouseX > maxX)
            {
                clampedMouseX = (int)Math.Round(maxX, MidpointRounding.AwayFromZero);
            }

            var resolvedX = (float)clampedMouseX;
            if (TryResolveNearestPointCenterX(
                    _renderedPointSeriesCenterXSamples,
                    resolvedX,
                    AnimationBarHitTolerancePixels,
                    out var snappedPointCenterX))
            {
                resolvedX = snappedPointCenterX;
            }

            nearestXIndex = ResolveNearestRenderedXSampleIndex(_renderedAnimationBarXSamples, resolvedX);
            return true;
        }

        private bool TryResolveAnimationBarDeviceX(
            Rectangle clientBounds,
            GraphPresentationModel presentation,
            out float deviceX)
        {
            deviceX = 0f;

            if (_renderedAnimationBarXSamples != null && _renderedAnimationBarXSamples.Length > 0)
            {
                var clampedIndex = _animationBarXIndex;
                if (clampedIndex < 0)
                {
                    clampedIndex = 0;
                }
                else if (clampedIndex >= _renderedAnimationBarXSamples.Length)
                {
                    clampedIndex = _renderedAnimationBarXSamples.Length - 1;
                }

                deviceX = _renderedAnimationBarXSamples[clampedIndex];
                return true;
            }

            if (!TryResolveAnimationBarAbstractX(presentation, _animationBarXIndex, out var abstractX))
            {
                return false;
            }

            deviceX = AbstractToDeviceX(clientBounds, abstractX);
            return true;
        }

        private float ClampAnimationBarDeviceXToRenderedExtent(float deviceX)
        {
            if (!_hasRenderedAnimationBarXExtent)
            {
                return deviceX;
            }

            if (deviceX < _renderedAnimationBarMinX)
            {
                return _renderedAnimationBarMinX;
            }

            if (deviceX > _renderedAnimationBarMaxX)
            {
                return _renderedAnimationBarMaxX;
            }

            return deviceX;
        }

        private float ResolvePointSeriesSnappedBarX(float barDeviceX)
        {
            if (TryResolveNearestPointCenterX(
                    _renderedPointSeriesCenterXSamples,
                    barDeviceX,
                    AnimationBarHitTolerancePixels,
                    out var snappedPointCenterX))
            {
                return snappedPointCenterX;
            }

            return barDeviceX;
        }

        private void UpdateRenderedAnimationBarXExtent(IReadOnlyList<RenderedSeriesPolyline> renderedSeriesGeometries)
        {
            if (!TryBuildRenderedXSampleSet(renderedSeriesGeometries, out var sortedXSamples, out var minX, out var maxX))
            {
                _renderedAnimationBarXSamples = Array.Empty<float>();
                _renderedPointSeriesCenterXSamples = Array.Empty<float>();
                _hasRenderedAnimationBarXExtent = false;
                return;
            }

            _renderedAnimationBarXSamples = sortedXSamples;
            _renderedPointSeriesCenterXSamples = BuildPointSeriesCenterXSamples(renderedSeriesGeometries);
            _renderedAnimationBarMinX = minX;
            _renderedAnimationBarMaxX = maxX;
            _hasRenderedAnimationBarXExtent = true;
        }

        private static float[] BuildPointSeriesCenterXSamples(IReadOnlyList<RenderedSeriesPolyline> renderedSeriesGeometries)
        {
            if (renderedSeriesGeometries == null || renderedSeriesGeometries.Count == 0)
            {
                return Array.Empty<float>();
            }

            var unique = new HashSet<float>();
            for (var i = 0; i < renderedSeriesGeometries.Count; i++)
            {
                var geometry = renderedSeriesGeometries[i];
                if (geometry == null || geometry.SeriesType != SeriesType.Scatter)
                {
                    continue;
                }

                var points = geometry.DevicePoints;
                if (points == null)
                {
                    continue;
                }

                for (var p = 0; p < points.Count; p++)
                {
                    unique.Add(points[p].X);
                }
            }

            if (unique.Count == 0)
            {
                return Array.Empty<float>();
            }

            var samples = new float[unique.Count];
            var index = 0;
            foreach (var x in unique)
            {
                samples[index++] = x;
            }

            Array.Sort(samples);
            return samples;
        }

        internal static bool TryBuildRenderedXSampleSet(
            IReadOnlyList<RenderedSeriesPolyline> renderedSeriesGeometries,
            out float[] sortedXSamples,
            out float minX,
            out float maxX)
        {
            sortedXSamples = Array.Empty<float>();
            minX = 0f;
            maxX = 0f;

            if (renderedSeriesGeometries == null || renderedSeriesGeometries.Count == 0)
            {
                return false;
            }

            var unique = new HashSet<float>();
            var min = float.MaxValue;
            var max = float.MinValue;

            for (var i = 0; i < renderedSeriesGeometries.Count; i++)
            {
                var geometry = renderedSeriesGeometries[i];
                var points = geometry?.DevicePoints;
                if (points == null || points.Count == 0)
                {
                    continue;
                }

                for (var p = 0; p < points.Count; p++)
                {
                    var x = points[p].X;
                    unique.Add(x);
                    if (x < min)
                    {
                        min = x;
                    }

                    if (x > max)
                    {
                        max = x;
                    }
                }
            }

            if (unique.Count == 0)
            {
                return false;
            }

            var samples = new float[unique.Count];
            var index = 0;
            foreach (var x in unique)
            {
                samples[index++] = x;
            }

            Array.Sort(samples);

            sortedXSamples = samples;
            minX = min;
            maxX = max;
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

        private static GraphPresentationOptions CreateZoomResetOptions(
            GraphPresentationOptions activeOptions,
            IGraphModel graphModel,
            IReadOnlyDictionary<string, AxisExtent> defaultAxisExtents)
        {
            var baseOptions = activeOptions ?? new GraphPresentationOptions();

            var axisOverrides = new Dictionary<AxisId, AxisOverrides>();
            if (baseOptions.AxisOverrides != null)
            {
                foreach (var axisOverride in baseOptions.AxisOverrides)
                {
                    axisOverrides[axisOverride.Key] = CloneAxisOverrides(axisOverride.Value);
                }
            }

            var axes = graphModel?.Axes;
            if (axes != null)
            {
                for (var axisIndex = 0; axisIndex < axes.Count; axisIndex++)
                {
                    var axis = axes[axisIndex];
                    if (axis?.Id == null)
                    {
                        continue;
                    }

                    if (!defaultAxisExtents.TryGetValue(axis.Id.Value, out var extent))
                    {
                        continue;
                    }

                    axisOverrides.TryGetValue(axis.Id, out var existingOverride);
                    var nextAxisOverride = CloneAxisOverrides(existingOverride);
                    nextAxisOverride.HasFixedRange = true;
                    nextAxisOverride.Minimum = extent.Minimum;
                    nextAxisOverride.Maximum = extent.Maximum;
                    nextAxisOverride.Increment = extent.Increment;
                    nextAxisOverride.HasFixedRange = true;
                    nextAxisOverride.HasFixedIncrement = true;
                    axisOverrides[axis.Id] = nextAxisOverride;
                }
            }

            var seriesStyles = new Dictionary<SeriesId, SeriesStyle>();
            if (baseOptions.SeriesStyles != null)
            {
                foreach (var seriesStyle in baseOptions.SeriesStyles)
                {
                    if (seriesStyle.Value == null)
                    {
                        continue;
                    }

                    seriesStyles[seriesStyle.Key] = new SeriesStyle
                    {
                        HasLabelOverride = seriesStyle.Value.HasLabelOverride,
                        Label = seriesStyle.Value.Label,
                        Color = seriesStyle.Value.Color
                    };
                }
            }

            return new GraphPresentationOptions(
                hiddenSeriesIds: baseOptions.HiddenSeriesIds,
                hiddenAxisIds: baseOptions.HiddenAxisIds,
                graphTitle: baseOptions.GraphTitle,
                graphSubtitle: baseOptions.GraphSubtitle,
                annotations: baseOptions.Annotations,
                showGraphBorder: baseOptions.ShowGraphBorder,
                legendPlacement: baseOptions.LegendPlacement,
                resizeChart: baseOptions.ResizeChart,
                axisEndpointInsetMode: baseOptions.AxisEndpointInsetMode,
                axisEndpointInsetFixedValue: baseOptions.AxisEndpointInsetFixedValue,
                hiddenAxisGridLineIds: baseOptions.HiddenAxisGridLineIds,
                seriesOrder: baseOptions.SeriesOrder,
                seriesStyles: seriesStyles,
                axisOverrides: axisOverrides,
                enableDenseNumericYAxisTicks: baseOptions.EnableDenseNumericYAxisTicks,
                denseNumericYAxisExcludedDimensions: baseOptions.DenseNumericYAxisExcludedDimensions != null
                    ? new HashSet<UnitRegistry.Dimension>(baseOptions.DenseNumericYAxisExcludedDimensions)
                    : null);
        }

        private static AxisOverrides CloneAxisOverrides(AxisOverrides source)
        {
            if (source == null)
            {
                return new AxisOverrides();
            }

            return new AxisOverrides
            {
                HasTitleOverride = source.HasTitleOverride,
                Title = source.Title,
                HasFixedRange = source.HasFixedRange,
                Minimum = source.Minimum,
                Maximum = source.Maximum,
                HasFixedIncrement = source.HasFixedIncrement,
                Increment = source.Increment,
                EnforceMinimumZero = source.EnforceMinimumZero
            };
        }

        private void CaptureDefaultAxisExtents(IGraphSnapshot snapshot)
        {
            _defaultAxisExtents.Clear();

            var axes = snapshot?.Axes;
            if (axes == null)
            {
                return;
            }

            for (var axisIndex = 0; axisIndex < axes.Count; axisIndex++)
            {
                var axis = axes[axisIndex];
                if (axis == null
                    || string.IsNullOrWhiteSpace(axis.AxisId)
                    || !axis.MinimumValue.HasValue
                    || !axis.MaximumValue.HasValue
                    || !axis.Increment.HasValue)
                {
                    continue;
                }

                _defaultAxisExtents[axis.AxisId] = new AxisExtent(axis.MinimumValue.Value, axis.MaximumValue.Value, axis.Increment.Value);
            }
        }

        private readonly struct AxisExtent
        {
            public AxisExtent(double minimum, double maximum, double increment)
            {
                Minimum = minimum;
                Maximum = maximum;
                Increment = increment;
            }

            public double Minimum { get; }

            public double Maximum { get; }

            public double Increment { get; }
        }

        internal enum ZoomGestureKind
        {
            None,
            ZoomIn,
            ZoomReset,
        }

    }
}
