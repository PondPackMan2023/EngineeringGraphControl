using System;
using System.Drawing;

namespace Graphing.Controls.Rendering
{
    internal sealed class WinFormsGraphRenderContext : IGraphRenderContext
    {
        public WinFormsGraphRenderContext(Graphics graphics)
        {
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        }

        public Graphics Graphics { get; }
    }
}