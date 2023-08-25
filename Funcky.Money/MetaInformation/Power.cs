using System.Numerics;

namespace Funcky;

internal static class Power<TUnderlyingType>
    where TUnderlyingType : INumberBase<TUnderlyingType>
{
    public static TUnderlyingType OfATenth(int exponent)
        => Exp(TUnderlyingType.CreateChecked(0.1m), exponent);

    private static TUnderlyingType Exp(TUnderlyingType @base, int exponent)
        => Enumerable.Repeat(@base, exponent)
            .Aggregate(TUnderlyingType.One, Multiply);

    private static TUnderlyingType Multiply(TUnderlyingType multiplicand, TUnderlyingType multiplier)
        => multiplicand * multiplier;
}
