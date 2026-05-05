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
        /// Renders <paramref name="model"/> into <paramref name="g"/> within
        /// <paramref name="deviceBounds"/>.
        /// </summary>
        void Render(Graphics g, Rectangle deviceBounds, GraphPresentationModel model, GraphPresentationOptions options = null);

        /// <summary>
        /// Creates a measurement input backed by <paramref name="g"/> for use during
        /// layout computation. This method exists on the renderer boundary because
        /// font/text measurements are renderer-technology-specific.
        /// </summary>
        IGraphLayoutMeasurementInput CreateMeasurementInput(Graphics g, Rectangle deviceBounds);
    }
}
