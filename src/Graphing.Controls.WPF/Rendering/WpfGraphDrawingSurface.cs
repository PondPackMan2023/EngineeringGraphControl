using System.Drawing;
using System.Windows.Media;
using Graphing.Controls.Rendering;

namespace Graphing.Controls.WPF.Rendering
{
    internal sealed class WpfGraphDrawingSurface : IGraphDrawingSurface
    {
        public WpfGraphDrawingSurface(DrawingContext drawingContext, Rectangle deviceBounds)
        {
            DrawingContext = drawingContext;
            DeviceBounds = deviceBounds;
        }

        public DrawingContext DrawingContext { get; }

        public Rectangle DeviceBounds { get; }
    }
}
