using System;

namespace Graphing.Core.Pie.Presentation
{
    /// <summary>
    /// Immutable identifier for a pie slice.
    /// Provides stable identity that is independent of display properties (label, value, etc.).
    /// </summary>
    public sealed class PieSliceId : IdentityBase<PieSliceId, string>
    {
        public PieSliceId(string value)
            : base(ValidateValue(value))
        {
        }

        private static string ValidateValue(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "PieSliceId cannot be null");
            }

            if (value.Length == 0)
            {
                throw new ArgumentException("PieSliceId cannot be empty", nameof(value));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("PieSliceId cannot be whitespace-only", nameof(value));
            }

            return value;
        }
    }
}
