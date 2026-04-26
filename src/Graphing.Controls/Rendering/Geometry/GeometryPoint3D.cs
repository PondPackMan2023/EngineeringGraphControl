namespace Graphing.Controls.Rendering.Geometry
{
    public sealed class GeometryPoint3D
    {
        public GeometryPoint3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }
    }
}