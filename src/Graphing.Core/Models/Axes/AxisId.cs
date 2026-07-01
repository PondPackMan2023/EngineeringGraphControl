using System;

namespace Graphing.Controls.Models
{
    /// <summary>
    /// Stable identity of an axis.
    /// </summary>
    public sealed class AxisId : IEquatable<AxisId>
    {
        public AxisId(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var normalizedValue = value.Trim();
            if (normalizedValue.Length == 0)
            {
                throw new ArgumentException("Axis identifier must not be empty or whitespace.", nameof(value));
            }

            Value = normalizedValue;
        }

        public string Value { get; }

        public bool Equals(AxisId other)
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
            return Equals(obj as AxisId);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(AxisId left, AxisId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AxisId left, AxisId right)
        {
            return !Equals(left, right);
        }
    }
}
