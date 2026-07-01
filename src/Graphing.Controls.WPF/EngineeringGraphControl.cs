using Graphing.Controls.Interaction;
using Graphing.Controls.Models;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering;
using Graphing.Controls.Rendering.Geometry;
using Graphing.Controls.Snapshot;
using Graphing.Controls.Models.Series;
using Graphing.Controls.WPF.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DrawingRectangleF = System.Drawing.RectangleF;

namespace Graphing.Controls
{
    /// <summary>
    /// WPF scaffold control for the Engineering graph surface.
    /// This phase intentionally focuses on rendering/interaction plumbing only.
    /// </summary>
    public class EngineeringGraphControl : FrameworkElement
    {
        public static readonly DependencyProperty GraphModelProperty = DependencyProperty.Register(
            nameof(GraphModel),
            typeof(IGraphModel),
            typeof(EngineeringGraphControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnGraphModelChanged));

        public static readonly DependencyProperty GraphPresentationOptionsProperty = DependencyProperty.Register(
            nameof(GraphPresentationOptions),
            typeof(GraphPresentationOptions),
            typeof(EngineeringGraphControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnGraphPresentationOptionsChanged));

        public static readonly DependencyProperty ZoomEnabledProperty = DependencyProperty.Register(
            nameof(ZoomEnabled),
            typeof(bool),
            typeof(EngineeringGraphControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnZoomEnabledChanged));

        public static readonly DependencyProperty ZoomExtentsRequestVersionProperty = DependencyProperty.Register(
            nameof(ZoomExtentsRequestVersion),
            typeof(int),
            typeof(EngineeringGraphControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnZoomExtentsRequestVersionChanged));

        private readonly object _snapshotSync = new object();
        private readonly ZoomDragStateMachine _zoomDragState = new ZoomDragStateMachine();
        private readonly Dictionary<string, AxisExtent> _defaultAxisExtents = new Dictionary<string, AxisExtent>(StringComparer.Ordinal);
        private readonly WpfGraphRenderer _wpfRenderer = new WpfGraphRenderer();

        private IGraphModel _graphModel;
        private IGraphSnapshot _activeSnapshot;
        private GraphPresentationModel _activePresentation;
        private GraphPresentationOptions _activeOptions;

        public IGraphModel GraphModel
        {
            get => (IGraphModel)GetValue(GraphModelProperty);
            set => SetValue(GraphModelProperty, value);
        }

        public GraphPresentationOptions GraphPresentationOptions
        {
            get => (GraphPresentationOptions)GetValue(GraphPresentationOptionsProperty);
            set => SetValue(GraphPresentationOptionsProperty, value);
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
                lock (_snapshotSync)
                {
                    return _activeOptions;
                }
            }
        }

        public bool ZoomEnabled
        {
            get => (bool)GetValue(ZoomEnabledProperty);
            set => SetValue(ZoomEnabledProperty, value);
        }

        public int ZoomExtentsRequestVersion
        {
            get => (int)GetValue(ZoomExtentsRequestVersionProperty);
            set => SetValue(ZoomExtentsRequestVersionProperty, value);
        }

        public void SetGraphSource(IGraphModel graphModel, GraphPresentationOptions options = null)
        {
            GraphPresentationOptions = options;
            GraphModel = graphModel;
        }

        private void ApplyGraphSource(IGraphModel graphModel, GraphPresentationOptions options = null)
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

                if (nextSnapshot == null)
                {
                    _activeSnapshot = null;
                    _activePresentation = null;
                    _activeOptions = null;
                }
                else
                {
                    _activeSnapshot = nextSnapshot;
                    _activePresentation = new GraphPresentationModel(nextSnapshot, options);
                    _activeOptions = options;
                }
            }

            InvalidateVisual();
        }

        public void ZoomExtents()
        {
            IGraphModel graphModel;
            GraphPresentationOptions activeOptions;
            Dictionary<string, AxisExtent> defaultAxisExtents;

            lock (_snapshotSync)
            {
                graphModel = _graphModel;
                activeOptions = _activeOptions;
                defaultAxisExtents = new Dictionary<string, AxisExtent>(_defaultAxisExtents, StringComparer.Ordinal);
            }

            if (graphModel == null)
            {
                InvalidateVisual();
                return;
            }

            var zoomResetOptions = CreateZoomResetOptions(activeOptions, graphModel, defaultAxisExtents);
            SetGraphSource(graphModel, zoomResetOptions);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            GraphPresentationModel presentation;
            GraphPresentationOptions options;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
                options = _activeOptions;
            }

            var w = (int)Math.Round(ActualWidth);
            var h = (int)Math.Round(ActualHeight);
            if (w <= 0 || h <= 0)
            {
                return;
            }

            var deviceBounds = new Rectangle(0, 0, w, h);

            if (presentation == null)
            {
                drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, null, new Rect(0, 0, w, h));
            }
            else
            {
                var context = new WpfGraphRenderContext(drawingContext);
                var measurementInput = _wpfRenderer.CreateMeasurementInput(context, deviceBounds);
                presentation = CreatePresentationModel(presentation, options, measurementInput);
                lock (_snapshotSync)
                {
                    _activePresentation = presentation;
                }

                _wpfRenderer.Render(context, deviceBounds, presentation, options);
            }

            if (ZoomEnabled && _zoomDragState.IsDragging && _zoomDragState.ZoomRectangle.HasValue)
            {
                var r = _zoomDragState.ZoomRectangle.Value;
                var pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Black, 1d)
                {
                    DashStyle = DashStyles.Dot
                };
                drawingContext.DrawRectangle(null, pen, new Rect(r.X, r.Y, r.Width, r.Height));
            }
        }

        private GraphPresentationModel CreatePresentationModel(
            GraphPresentationModel currentPresentation,
            GraphPresentationOptions options,
            IGraphLayoutMeasurementInput measurementInput)
        {
            IGraphSnapshot snapshot;
            lock (_snapshotSync)
            {
                snapshot = _activeSnapshot;
            }

            if (snapshot == null)
            {
                return currentPresentation;
            }

            try
            {
                return new GraphPresentationModel(snapshot, options, measurementInput);
            }
            catch
            {
                return currentPresentation;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // Keep shared axis-hit orchestration active in the WPF host scaffold.
            var mousePosition = e.GetPosition(this);
            _ = TryResolveAxisInteraction(new System.Drawing.Point((int)Math.Round(mousePosition.X), (int)Math.Round(mousePosition.Y)), out _, out _);

            if (!ZoomEnabled || e.ChangedButton != MouseButton.Left || _activePresentation == null)
            {
                return;
            }

            if (!TryGetCurrentPlotRect(out var plotRect))
            {
                return;
            }

            var point = new System.Drawing.Point((int)Math.Round(mousePosition.X), (int)Math.Round(mousePosition.Y));
            if (_zoomDragState.TryBeginDrag(point, plotRect))
            {
                CaptureMouse();
                InvalidateVisual();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!ZoomEnabled || !_zoomDragState.IsDragging)
            {
                return;
            }

            if (!TryGetCurrentPlotRect(out var plotRect))
            {
                return;
            }

            var position = e.GetPosition(this);
            var point = new System.Drawing.Point((int)Math.Round(position.X), (int)Math.Round(position.Y));
            if (_zoomDragState.TryMoveDrag(point, plotRect))
            {
                InvalidateVisual();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (!ZoomEnabled || e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (!_zoomDragState.TryEndDrag(out var gesture, out _))
            {
                return;
            }

            ReleaseMouseCapture();
            if (gesture == ZoomGestureDirection.ZoomReset)
            {
                ZoomExtents();
            }

            InvalidateVisual();
        }

        private bool TryGetCurrentPlotRect(out DrawingRectangleF plotRect)
        {
            plotRect = DrawingRectangleF.Empty;

            GraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            if (presentation == null || ActualWidth <= 0d || ActualHeight <= 0d)
            {
                return false;
            }

            var plotArea = presentation.Layout.PlotArea;
            var left = plotArea.BottomLeft.X * ActualWidth;
            var right = plotArea.TopRight.X * ActualWidth;
            var top = (1d - plotArea.TopRight.Y) * ActualHeight;
            var bottom = (1d - plotArea.BottomLeft.Y) * ActualHeight;

            if (right <= left || bottom <= top)
            {
                return false;
            }

            plotRect = DrawingRectangleF.FromLTRB((float)left, (float)top, (float)right, (float)bottom);
            return true;
        }

        private bool TryResolveAxisInteraction(
            System.Drawing.Point rawInputPosition,
            out AxisInteractionDescriptor descriptor,
            out GeometryPoint3D graphPosition)
        {
            descriptor = null;
            graphPosition = new GeometryPoint3D(0d, 0d, 0d);

            GraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            var clientBounds = new System.Drawing.Rectangle(
                0,
                0,
                (int)Math.Round(ActualWidth),
                (int)Math.Round(ActualHeight));

            if (!AxisInteractionOrchestrator.TryResolve(
                clientBounds,
                rawInputPosition,
                presentation,
                p => p,
                (model, position) => model.ResolveAxisInteraction(position),
                out var resolution))
            {
                return false;
            }

            descriptor = resolution.Descriptor;
            graphPosition = resolution.GraphPosition;
            return true;
        }

        private void CaptureDefaultAxisExtents(IGraphSnapshot snapshot)
        {
            _defaultAxisExtents.Clear();

            if (snapshot?.Axes == null)
            {
                return;
            }

            var axes = snapshot.Axes;
            for (var i = 0; i < axes.Count; i++)
            {
                var axis = axes[i];
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
                    nextAxisOverride.HasFixedIncrement = true;
                    nextAxisOverride.Minimum = extent.Minimum;
                    nextAxisOverride.Maximum = extent.Maximum;
                    nextAxisOverride.Increment = extent.Increment;
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

        private static void OnGraphModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (EngineeringGraphControl)dependencyObject;
            control.ApplyGraphSource((IGraphModel)eventArgs.NewValue, control.GraphPresentationOptions);
        }

        private static void OnGraphPresentationOptionsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (EngineeringGraphControl)dependencyObject;
            control.ApplyGraphSource(control.GraphModel, (GraphPresentationOptions)eventArgs.NewValue);
        }

        private static void OnZoomEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (EngineeringGraphControl)dependencyObject;
            if (!(bool)eventArgs.NewValue)
            {
                control._zoomDragState.Clear();
                if (Mouse.Captured == control)
                {
                    control.ReleaseMouseCapture();
                }
            }

            control.InvalidateVisual();
        }

        private static void OnZoomExtentsRequestVersionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (Equals(eventArgs.OldValue, eventArgs.NewValue))
            {
                return;
            }

            var control = (EngineeringGraphControl)dependencyObject;
            control.ZoomExtents();
        }
    }
}
