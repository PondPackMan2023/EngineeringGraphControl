using Graphing.Controls.Rendering.Geometry;

namespace Graphing.Controls.Presentation
{
    public sealed class AxisTickPresentation
    {
        public AxisTickPresentation(double value, string label, GeometryPoint3D start, GeometryPoint3D end)
        {
            Value = value;
            Label = label;
            Start = start;
            End = end;
        }

        public double Value { get; }
        public string Label { get; }
        public GeometryPoint3D Start { get; }
        public GeometryPoint3D End { get; }
    }
}