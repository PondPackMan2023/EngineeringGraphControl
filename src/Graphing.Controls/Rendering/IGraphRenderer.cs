using Graphing.Controls.Presentation;
using System.Drawing;

namespace Graphing.Controls.Rendering
{
    /// <summary>
    /// Abstraction for the graph rasterizer. Consumes fully-resolved presentation output
    /// and draws it onto a device surface. Implementations must not participate in snapshot
    /// construction, layout calculation, or model lifecycle.
    /// </summary>
    internal interface IGraphRenderer
    {
        /// <summary>
        /// Renders <paramref name="model"/> into <paramref name="context"/> within
        /// <paramref name="deviceBounds"/>.
        /// </summary>
        void Render(IGraphRenderContext context, Rectangle deviceBounds, GraphPresentationModel model, GraphPresentationOptions options = null);

        /// <summary>
        /// Creates a measurement input backed by <paramref name="context"/> for use during
        /// layout computation. This method exists on the renderer boundary because
        /// font/text measurements are renderer-technology-specific.
        /// </summary>
        IGraphLayoutMeasurementInput CreateMeasurementInput(IGraphRenderContext context, Rectangle deviceBounds);
    }
}
