using Graphing.Controls.Presentation;
using System.Drawing;

namespace Graphing.Controls.Rendering
{
    internal abstract class GraphRendererBase : IGraphRenderer
    {
        public void Render(IGraphRenderContext context, Rectangle deviceBounds, GraphPresentationModel model, GraphPresentationOptions options = null)
        {
            if (model == null || deviceBounds.Width <= 0 || deviceBounds.Height <= 0)
            {
                return;
            }

            var surface = TryCreateDrawingSurface(context, deviceBounds);
            if (surface == null)
            {
                return;
            }

            var plotRect = ComputeDevicePlotRect(deviceBounds, model.Layout.PlotArea);
            if (plotRect.Width <= 0 || plotRect.Height <= 0)
            {
                return;
            }

            RenderCore(surface, plotRect, model, options);
        }

        public IGraphLayoutMeasurementInput CreateMeasurementInput(IGraphRenderContext context, Rectangle deviceBounds)
        {
            return CreateMeasurementInputCore(context, deviceBounds);
        }

        protected static RectangleF ComputeDevicePlotRect(Rectangle deviceBounds, PlotAreaLayout plotArea)
        {
            var left = deviceBounds.Left + plotArea.BottomLeft.X * deviceBounds.Width;
            var right = deviceBounds.Left + plotArea.TopRight.X * deviceBounds.Width;
            var top = deviceBounds.Bottom - plotArea.TopRight.Y * deviceBounds.Height;
            var bottom = deviceBounds.Bottom - plotArea.BottomLeft.Y * deviceBounds.Height;
            return RectangleF.FromLTRB((float)left, (float)top, (float)right, (float)bottom);
        }

        protected abstract IGraphDrawingSurface TryCreateDrawingSurface(IGraphRenderContext context, Rectangle deviceBounds);

        protected abstract IGraphLayoutMeasurementInput CreateMeasurementInputCore(IGraphRenderContext context, Rectangle deviceBounds);

        protected abstract void RenderCore(IGraphDrawingSurface surface, RectangleF plotRect, GraphPresentationModel model, GraphPresentationOptions options);
    }
}