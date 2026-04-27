using System;

namespace Graphing.Controls.Models
{
    /// <summary>
    /// Stable identity of a series.
    /// </summary>
    public sealed class SeriesId : IEquatable<SeriesId>
    {
        public SeriesId(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var normalizedValue = value.Trim();
            if (normalizedValue.Length == 0)
            {
                throw new ArgumentException("Series identifier must not be empty or whitespace.", nameof(value));
            }

            Value = normalizedValue;
        }

        public string Value { get; }

        public bool Equals(SeriesId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SeriesId);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(SeriesId left, SeriesId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SeriesId left, SeriesId right)
        {
            return !Equals(left, right);
        }
    }
}
