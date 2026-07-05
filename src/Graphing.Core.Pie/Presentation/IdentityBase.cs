using System;
using System.Collections.Generic;

namespace Graphing.Core.Pie.Presentation
{
    /// <summary>
    /// Immutable base class for strongly-typed identifiers.
    /// Provides equality, hashing, and string representation for identity values.
    /// </summary>
    public abstract class IdentityBase<TIdentity, TValue> : IEquatable<TIdentity>
        where TIdentity : IdentityBase<TIdentity, TValue>
        where TValue : notnull
    {
        protected IdentityBase(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; }

        public bool Equals(TIdentity? other)
        {
            return other is not null && EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is TIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(typeof(TIdentity), Value);
        }

        public override string ToString()
        {
            return Value.ToString() ?? string.Empty;
        }

        public static bool operator ==(IdentityBase<TIdentity, TValue>? left, IdentityBase<TIdentity, TValue>? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IdentityBase<TIdentity, TValue>? left, IdentityBase<TIdentity, TValue>? right)
        {
            return !(left == right);
        }
    }
}
