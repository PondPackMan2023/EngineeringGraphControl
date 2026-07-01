using System;
using System.Drawing;

namespace Graphing.Controls.Rendering
{
    internal sealed class WinFormsGraphDrawingSurface : IGraphDrawingSurface
    {
        public WinFormsGraphDrawingSurface(Graphics graphics, Rectangle deviceBounds)
        {
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            DeviceBounds = deviceBounds;
        }

        public Graphics Graphics { get; }

        public Rectangle DeviceBounds { get; }
    }
}