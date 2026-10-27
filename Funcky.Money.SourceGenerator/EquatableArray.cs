using System.Collections;
using System.Collections.Immutable;

namespace Funcky.Money.SourceGenerator;

/// <summary>
/// An immutable array with structural equality, so the incremental generator pipeline can cache on it.
/// </summary>
internal readonly struct EquatableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly ImmutableArray<T> _array = array;

    public static EquatableArray<T> Empty { get; } = new([]);

    public bool Equals(EquatableArray<T> other)
        => _array.AsSpan().SequenceEqual(other._array.AsSpan());

    public override bool Equals(object? obj)
        => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
        => _array.Aggregate(17, static (hash, item) => unchecked((hash * 31) + item.GetHashCode()));

    public IEnumerator<T> GetEnumerator()
        => ((IEnumerable<T>)_array).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}

internal static class EquatableArrayExtensions
{
    public static EquatableArray<T> ToEquatableArray<T>(this ImmutableArray<T> array)
        where T : IEquatable<T>
        => new(array);
}
