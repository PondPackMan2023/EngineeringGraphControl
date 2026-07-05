using System;

namespace Graphing.Core.Pie.Presentation
{
    public readonly struct PieColor : IEquatable<PieColor>
    {
        public static readonly PieColor Empty = default;

        public PieColor(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public byte A { get; }

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public int ToArgb()
        {
            return (A << 24) | (R << 16) | (G << 8) | B;
        }

        public static PieColor FromArgb(int a, int r, int g, int b)
        {
            return new PieColor((byte)a, (byte)r, (byte)g, (byte)b);
        }

        public bool Equals(PieColor other)
        {
            return A == other.A && R == other.R && G == other.G && B == other.B;
        }

        public override bool Equals(object? obj)
        {
            return obj is PieColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ToArgb();
        }

        public static bool operator ==(PieColor left, PieColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PieColor left, PieColor right)
        {
            return !left.Equals(right);
        }
    }
}
