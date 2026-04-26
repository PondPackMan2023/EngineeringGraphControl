using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    public sealed class AxisTickPresentation
    {
        public AxisTickPresentation(double value, GeometryPoint3D anchorPoint, string label)
        {
            Value = value;
            AnchorPoint = anchorPoint;
            Label = label;
        }

        public double Value { get; }
        public GeometryPoint3D AnchorPoint { get; }
        public string Label { get; }
    }
}