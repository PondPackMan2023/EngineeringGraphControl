using System;

namespace Graphing.Controls.Presentation
{
    /// <summary>
    /// Renderer-agnostic ARGB color value used by presentation-layer contracts.
    /// </summary>
    public readonly struct GraphColor : IEquatable<GraphColor>
    {
        public static readonly GraphColor Empty = default;
        public static readonly GraphColor SteelBlue = FromArgb(255, 70, 130, 180);

        public GraphColor(byte a, byte r, byte g, byte b)
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

        public static GraphColor FromArgb(int argb)
        {
            unchecked
            {
                var a = (byte)((argb >> 24) & 0xFF);
                var r = (byte)((argb >> 16) & 0xFF);
                var g = (byte)((argb >> 8) & 0xFF);
                var b = (byte)(argb & 0xFF);
                return new GraphColor(a, r, g, b);
            }
        }

        public static GraphColor FromArgb(int a, int r, int g, int b)
        {
            return new GraphColor((byte)a, (byte)r, (byte)g, (byte)b);
        }

        public static GraphColor FromRgb(int r, int g, int b)
        {
            return FromArgb(255, r, g, b);
        }

        public bool Equals(GraphColor other)
        {
            return A == other.A && R == other.R && G == other.G && B == other.B;
        }

        public override bool Equals(object obj)
        {
            return obj is GraphColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ToArgb();
        }

        public static bool operator ==(GraphColor left, GraphColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GraphColor left, GraphColor right)
        {
            return !left.Equals(right);
        }
    }
}