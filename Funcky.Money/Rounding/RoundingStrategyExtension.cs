using System.Numerics;

namespace Funcky;

internal static class RoundingStrategyExtension
{
    public static bool IsSameAfterRounding<TUnderlyingType>(this IRoundingStrategy<TUnderlyingType> roundingStrategy, TUnderlyingType amount)
        where TUnderlyingType : INumberBase<TUnderlyingType>
        => roundingStrategy.Round(amount) == amount;
}
