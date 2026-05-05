using Graphing.Controls.Models;
using Graphing.Controls.Interaction;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using Graphing.Controls.Rendering.Geometry;
using Graphing.Controls.Snapshot;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Graphing.Controls
{
    public class EngineeringGraphControl : UserControl
    {
        private const float AnimationBarLineWidth = 1.5f;
        private static readonly Color DefaultAnimationBarColor = Color.OrangeRed;

        private readonly object _snapshotSync = new object();
        private readonly IGraphRenderer _renderer = new WinFormsGraphRenderer();

        private IGraphModel _graphModel;
        private IGraphSnapshot _activeSnapshot;
        private GraphPresentationModel _activePresentation;
        private GraphPresentationOptions _activePresentationOptions;
        private bool _animationBarEnabled;
        private int _animationBarXIndex;
        private bool _hasAnimationBarXIndex;
        private Color _animationBarColor = DefaultAnimationBarColor;

        public event EventHandler<AxisInteractionMouseEventArgs> AxisMouseDown;

        public event EventHandler<AxisInteractionMouseEventArgs> AxisMouseUp;

        public event EventHandler<AxisInteractionMouseEventArgs> AxisContextRequested;

        public event EventHandler<AnimationBarIndexChangedEventArgs> AnimationBarXIndexChanged;

        public EngineeringGraphControl()
        {
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
                _renderer.Render(e.Graphics, ClientRectangle, presentation, options);
                RenderAnimationBarOverlay(e.Graphics, ClientRectangle, presentation);
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

            // Phase H4 intentionally does not track hover state; this probe keeps
            // the mouse-to-presentation bridge in place for future hover phases.
            _ = TryResolveAxisInteraction(e, out _, out _, out _);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

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

        private void RenderAnimationBarOverlay(Graphics graphics, Rectangle clientBounds, GraphPresentationModel presentation)
        {
            if (!AnimationBarEnabled || !_hasAnimationBarXIndex || graphics == null || presentation == null)
            {
                return;
            }

            if (!TryResolveAnimationBarAbstractX(presentation, _animationBarXIndex, out var abstractX))
            {
                return;
            }

            var plotRect = ComputeDevicePlotRect(clientBounds, presentation.Layout.PlotArea);
            if (plotRect.Width <= 0f || plotRect.Height <= 0f)
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
            }
            finally
            {
                graphics.SetClip(clip);
            }
        }

        private static bool TryResolveAnimationBarAbstractX(
            GraphPresentationModel presentation,
            int xIndex,
            out double abstractX)
        {
            abstractX = 0d;

            var layout = presentation.Layout;
            var series = layout.Series;
            if (series == null || series.Count == 0)
            {
                return false;
            }

            for (var i = 0; i < series.Count; i++)
            {
                var candidate = series[i];
                var xAxisEntry = candidate?.XAxisEntry;
                var points = candidate?.Points;

                if (xAxisEntry == null || points == null || xIndex >= points.Count)
                {
                    continue;
                }

                var xAxis = xAxisEntry.Axis;
                if (xAxis == null || !xAxis.MinimumValue.HasValue || !xAxis.MaximumValue.HasValue)
                {
                    continue;
                }

                var xMin = xAxis.MinimumValue.Value;
                var xMax = xAxis.MaximumValue.Value;
                var xRange = xMax - xMin;
                if (xRange <= 0d)
                {
                    continue;
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

                var plotArea = layout.PlotArea;
                abstractX = plotArea.BottomLeft.X + (normalized * (plotArea.TopRight.X - plotArea.BottomLeft.X));
                return true;
            }

            return false;
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
