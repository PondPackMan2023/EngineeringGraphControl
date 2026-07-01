using System.Windows.Media;
using Graphing.Controls.Rendering;

namespace Graphing.Controls.WPF.Rendering
{
    internal sealed class WpfGraphRenderContext : IGraphRenderContext
    {
        public WpfGraphRenderContext(DrawingContext drawingContext)
        {
            DrawingContext = drawingContext;
        }

        public DrawingContext DrawingContext { get; }
    }
}
