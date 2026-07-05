namespace Graphing.Core.Pie.Presentation
{
    public readonly struct PiePoint
    {
        public PiePoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }

        public double Y { get; }
    }
}
