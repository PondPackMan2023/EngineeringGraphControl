using System;
using System.Drawing;
using System.Windows.Forms;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Interaction
{
    /// <summary>
    /// Immutable payload for semantic axis interaction mouse events.
    /// </summary>
    public sealed class AxisInteractionMouseEventArgs : EventArgs
    {
        public AxisInteractionMouseEventArgs(
            AxisInteractionDescriptor descriptor,
            MouseButtons button,
            Keys modifiers,
            Point clientPosition,
            GeometryPoint3D graphPosition)
        {
            Descriptor = descriptor;
            Button = button;
            Modifiers = modifiers;
            ClientPosition = clientPosition;
            GraphPosition = graphPosition;
        }

        public AxisInteractionDescriptor Descriptor { get; }

        public MouseButtons Button { get; }

        public Keys Modifiers { get; }

        public Point ClientPosition { get; }

        public GeometryPoint3D GraphPosition { get; }
    }
}
