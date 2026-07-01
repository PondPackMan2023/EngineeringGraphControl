using System.Drawing;

namespace Graphing.Controls.Rendering
{
    /// <summary>
    /// Platform-neutral drawing surface descriptor for graph rendering.
    /// Concrete surfaces may expose platform-specific handles (Graphics, DrawingContext, etc.)
    /// while this contract keeps renderer orchestration platform-agnostic.
    /// </summary>
    internal interface IGraphDrawingSurface
    {
        Rectangle DeviceBounds { get; }
    }
}