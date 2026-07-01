using System;
using System.Drawing;
using Graphing.Controls.Presentation;
using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Interaction
{
    internal readonly struct AxisInteractionResolution
    {
        public AxisInteractionResolution(AxisInteractionDescriptor descriptor, Point clientPosition, GeometryPoint3D graphPosition)
        {
            Descriptor = descriptor;
            ClientPosition = clientPosition;
            GraphPosition = graphPosition;
        }

        public AxisInteractionDescriptor Descriptor { get; }

        public Point ClientPosition { get; }

        public GeometryPoint3D GraphPosition { get; }
    }

    internal static class AxisInteractionOrchestrator
    {
        internal static bool TryResolve(
            Rectangle clientBounds,
            Point rawInputPosition,
            GraphPresentationModel presentation,
            Func<Point, Point> normalizeToClient,
            Func<GraphPresentationModel, GeometryPoint3D, AxisInteractionDescriptor> descriptorResolver,
            out AxisInteractionResolution resolution)
        {
            resolution = default;

            if (clientBounds.Width <= 0 || clientBounds.Height <= 0 || presentation == null)
            {
                return false;
            }

            var clientPosition = normalizeToClient != null
                ? normalizeToClient(rawInputPosition)
                : rawInputPosition;
            var graphPosition = ToGraphPosition(clientBounds, clientPosition);

            var descriptor = descriptorResolver != null
                ? descriptorResolver(presentation, graphPosition)
                : null;

            resolution = new AxisInteractionResolution(descriptor, clientPosition, graphPosition);
            return true;
        }

        private static GeometryPoint3D ToGraphPosition(Rectangle clientBounds, Point clientPosition)
        {
            var normalizedX = (clientPosition.X - clientBounds.Left) / (double)clientBounds.Width;
            var normalizedY = (clientBounds.Bottom - clientPosition.Y) / (double)clientBounds.Height;
            return new GeometryPoint3D(normalizedX, normalizedY, 0d);
        }
    }
}
