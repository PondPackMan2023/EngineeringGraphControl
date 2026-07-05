namespace Graphing.Core.Pie.Presentation
{
    public readonly struct PieBounds
    {
        public PieBounds(double left, double bottom, double right, double top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        public double Left { get; }

        public double Bottom { get; }

        public double Right { get; }

        public double Top { get; }

        public double Width => Right - Left;

        public double Height => Top - Bottom;
    }
}
