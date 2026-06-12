using System.Collections.Immutable;

namespace Funcky.Money.SourceGenerator;

/// <summary>
/// An <see cref="ImmutableArray{T}" /> wrapper that provides structural (element-wise) equality.
/// This is required so the values flowing through the incremental generator pipeline are cacheable;
/// <see cref="ImmutableArray{T}" /> itself only compares by the underlying array reference.
/// </summary>
internal readonly struct EquatableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    public ImmutableArray<T> AsImmutableArray()
        => array.IsDefault ? ImmutableArray<T>.Empty : array;

    public bool Equals(EquatableArray<T> other)
        => AsImmutableArray().SequenceEqual(other.AsImmutableArray());

    public override bool Equals(object? obj)
        => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return Enumerable.Aggregate(AsImmutableArray(), 17, (current, item) => (current * 31) + item.GetHashCode());
        }
    }
}

internal static class EquatableArrayExtension
{
    public static EquatableArray<T> AsEquatableArray<T>(this ImmutableArray<T> array)
        where T : IEquatable<T>
        => new(array);
}
