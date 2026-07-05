using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Graphing.Controls.WPF.Rendering;
using Graphing.Core.Pie.Models;
using Graphing.Core.Pie.Presentation;
using Graphing.Core.Pie.Snapshot;

namespace Graphing.Controls
{
    /// <summary>
    /// WPF host control for Pie graph rendering.
    /// Owns orchestration from model to snapshot and presentation.
    /// Owns interaction behavior including hit testing and tooltip support.
    /// </summary>
    public class EngineeringPieGraphControl : FrameworkElement
    {
        public static readonly DependencyProperty PieGraphModelProperty = DependencyProperty.Register(
            nameof(PieGraphModel),
            typeof(IPieGraphModel),
            typeof(EngineeringPieGraphControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPieGraphModelChanged));

        public static readonly DependencyProperty PieGraphPresentationOptionsProperty = DependencyProperty.Register(
            nameof(PieGraphPresentationOptions),
            typeof(PieGraphPresentationOptions),
            typeof(EngineeringPieGraphControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPieGraphPresentationOptionsChanged));

        public static readonly DependencyProperty PieSliceDoubleClickCommandProperty = DependencyProperty.Register(
            nameof(PieSliceDoubleClickCommand),
            typeof(ICommand),
            typeof(EngineeringPieGraphControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        private readonly object _snapshotSync = new object();
        private readonly PieGraphSnapshotBuilder _snapshotBuilder = new PieGraphSnapshotBuilder();
        private readonly PieGraphPresentationBuilder _presentationBuilder = new PieGraphPresentationBuilder();
        private readonly WpfPieGraphRenderer _renderer = new WpfPieGraphRenderer();

        private PieGraphSnapshot _activeSnapshot;
        private PieGraphPresentationModel _activePresentation;
        private PieGraphPresentationOptions _activeOptions;
        private ToolTip _tooltip;
        private PieSliceId _currentHoveredSliceId;
        
        private DispatcherTimer _tooltipDelayTimer;
        private PieSlicePresentationGeometry _pendingSliceForTooltip;
        private const int TooltipDelayMilliseconds = 400;
        
        // Double-click tracking
        private int _lastMouseDownClickCount;
        private System.Windows.Point _lastMouseDownPosition;

        public IPieGraphModel PieGraphModel
        {
            get => (IPieGraphModel)GetValue(PieGraphModelProperty);
            set => SetValue(PieGraphModelProperty, value);
        }

        public PieGraphPresentationOptions PieGraphPresentationOptions
        {
            get => (PieGraphPresentationOptions)GetValue(PieGraphPresentationOptionsProperty);
            set => SetValue(PieGraphPresentationOptionsProperty, value);
        }

        public ICommand? PieSliceDoubleClickCommand
        {
            get => (ICommand?)GetValue(PieSliceDoubleClickCommandProperty);
            set => SetValue(PieSliceDoubleClickCommandProperty, value);
        }

        public PieGraphSnapshot ActiveSnapshot
        {
            get
            {
                lock (_snapshotSync)
                {
                    return _activeSnapshot;
                }
            }
        }

        public PieGraphPresentationModel ActivePresentation
        {
            get
            {
                lock (_snapshotSync)
                {
                    return _activePresentation;
                }
            }
        }

        public PieGraphPresentationOptions ActiveOptions
        {
            get
            {
                lock (_snapshotSync)
                {
                    return _activeOptions;
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            ArgumentNullException.ThrowIfNull(drawingContext);

            PieGraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            var width = (int)Math.Round(ActualWidth);
            var height = (int)Math.Round(ActualHeight);
            if (width <= 0 || height <= 0)
            {
                return;
            }

            if (presentation == null)
            {
                drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, null, new Rect(0, 0, width, height));
                return;
            }

            _renderer.Render(drawingContext, new Rectangle(0, 0, width, height), presentation);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            PieGraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            if (presentation == null)
            {
                HideTooltip();
                return;
            }

            var width = (int)Math.Round(ActualWidth);
            var height = (int)Math.Round(ActualHeight);

            if (width <= 0 || height <= 0)
            {
                HideTooltip();
                return;
            }

            // Get mouse position in device coordinates
            var position = e.GetPosition(this);
            var deviceX = position.X;
            var deviceY = position.Y;

            // Normalize to 0.0-1.0 range
            var normalizedX = deviceX / width;
            var normalizedY = 1.0 - (deviceY / height); // Invert Y for normalized space

            // Clamp to valid range
            if (normalizedX < 0 || normalizedX > 1.0 || normalizedY < 0 || normalizedY > 1.0)
            {
                HideTooltip();
                return;
            }

            // Perform hit test
            var sliceId = PieHitTestHelper.HitTest(normalizedX, normalizedY, presentation);

            if (sliceId == null)
            {
                HideTooltip();
            }
            else
            {
                // Find the slice and queue it for tooltip display with delay
                foreach (var slice in presentation.Slices)
                {
                    if (slice.Id.Equals(sliceId))
                    {
                        // If this is the same slice we're already hovering, keep the tooltip
                        if (_currentHoveredSliceId != null && _currentHoveredSliceId.Equals(sliceId))
                        {
                            // Already showing tooltip for this slice, keep it visible
                            break;
                        }
                        
                        // Different slice: restart the delay timer
                        ScheduleTooltipDisplay(slice);
                        break;
                    }
                }
            }
        }

        public void SetPieGraphSource(IPieGraphModel pieGraphModel, PieGraphPresentationOptions options = null)
        {
            PieGraphPresentationOptions = options;
            PieGraphModel = pieGraphModel;
        }

        private void ScheduleTooltipDisplay(PieSlicePresentationGeometry slice)
        {
            // Store the slice and restart the timer
            _pendingSliceForTooltip = slice;
            
            if (_tooltipDelayTimer == null)
            {
                _tooltipDelayTimer = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromMilliseconds(TooltipDelayMilliseconds)
                };
                _tooltipDelayTimer.Tick += (s, e) => OnTooltipDelayExpired();
            }
            
            // Restart timer
            _tooltipDelayTimer.Stop();
            _tooltipDelayTimer.Start();
        }

        private void OnTooltipDelayExpired()
        {
            _tooltipDelayTimer.Stop();
            
            if (_pendingSliceForTooltip != null)
            {
                ShowTooltip(_pendingSliceForTooltip);
            }
        }

        private void ShowTooltip(PieSlicePresentationGeometry slice)
        {
            if (slice == null)
            {
                HideTooltip();
                return;
            }

            var content = PieTooltipContentGenerator.GenerateTooltip(slice);

            // Check if we're transitioning to a different slice
            bool isNewSlice = _currentHoveredSliceId == null || !_currentHoveredSliceId.Equals(slice.Id);

            if (_tooltip == null)
            {
                _tooltip = new ToolTip
                {
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                    HorizontalOffset = 8,
                    VerticalOffset = 8
                };
                ToolTipService.SetToolTip(this, _tooltip);
            }

            // If transitioning to a different slice, close and reopen to ensure content refresh
            if (isNewSlice && _tooltip.IsOpen)
            {
                _tooltip.IsOpen = false;
            }

            _tooltip.Content = content;
            _tooltip.IsOpen = true;
            _currentHoveredSliceId = slice.Id;
            _pendingSliceForTooltip = null;
        }

        private void HideTooltip()
        {
            if (_tooltipDelayTimer != null)
            {
                _tooltipDelayTimer.Stop();
            }
            _pendingSliceForTooltip = null;
            
            if (_tooltip != null)
            {
                _tooltip.IsOpen = false;
            }
            _currentHoveredSliceId = null;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            HideTooltip();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            
            var position = e.GetPosition(this);
            
            // Detect double-click: if click count is 2, we have a double-click
            if (e.ClickCount == 2)
            {
                HandleSliceDoubleClick(position);
                e.Handled = false;  // Allow other handlers to process
            }
        }

        private void HandleSliceDoubleClick(System.Windows.Point position)
        {
            PieGraphPresentationModel presentation;
            lock (_snapshotSync)
            {
                presentation = _activePresentation;
            }

            if (presentation == null)
            {
                return;
            }

            var width = (int)Math.Round(ActualWidth);
            var height = (int)Math.Round(ActualHeight);

            if (width <= 0 || height <= 0)
            {
                return;
            }

            var deviceX = position.X;
            var deviceY = position.Y;

            // Normalize to 0.0-1.0 range
            var normalizedX = deviceX / width;
            var normalizedY = 1.0 - (deviceY / height);

            // Clamp to valid range
            if (normalizedX < 0 || normalizedX > 1.0 || normalizedY < 0 || normalizedY > 1.0)
            {
                return;
            }

            // Perform hit test
            var sliceId = PieHitTestHelper.HitTest(normalizedX, normalizedY, presentation);

            if (sliceId == null)
            {
                return;  // No slice under cursor, do nothing
            }

            // Find the slice
            foreach (var slice in presentation.Slices)
            {
                if (slice.Id.Equals(sliceId))
                {
                    ExecuteSliceCommand(slice);
                    break;
                }
            }
        }

        private void ExecuteSliceCommand(PieSlicePresentationGeometry slice)
        {
            var command = PieSliceDoubleClickCommand;
            if (command == null)
            {
                return;  // No command registered, do nothing
            }

            // Create interaction context from presentation geometry
            var context = new PieSliceInteractionContext(
                slice.Id,
                slice.Label,
                slice.Value,
                slice.FormattedValue,
                slice.Percentage);

            // Execute command if it can handle the parameter
            if (command.CanExecute(context))
            {
                command.Execute(context);
            }
        }

        private void ApplyGraphSource(IPieGraphModel model, PieGraphPresentationOptions options)
        {
            lock (_snapshotSync)
            {
                if (model == null)
                {
                    _activeSnapshot = null;
                    _activePresentation = null;
                    _activeOptions = null;
                }
                else
                {
                    var snapshot = _snapshotBuilder.Build(model);
                    var presentation = _presentationBuilder.Build(snapshot, options);

                    _activeSnapshot = snapshot;
                    _activePresentation = presentation;
                    _activeOptions = presentation.Options;
                }
            }

            InvalidateVisual();
        }

        private static void OnPieGraphModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EngineeringPieGraphControl)d;
            control.ApplyGraphSource((IPieGraphModel)e.NewValue, control.PieGraphPresentationOptions);
        }

        private static void OnPieGraphPresentationOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EngineeringPieGraphControl)d;
            control.ApplyGraphSource(control.PieGraphModel, (PieGraphPresentationOptions)e.NewValue);
        }
    }
}