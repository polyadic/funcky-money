using System.Numerics;

namespace Funcky;

public static class RoundingStrategy<TUnderlyingType>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    public static IRoundingStrategy<TUnderlyingType> NoRounding()
        => new NoRounding<TUnderlyingType>();

    public static IRoundingStrategy<TUnderlyingType> BankersRounding(TUnderlyingType precision)
        => new BankersRounding<TUnderlyingType>(precision);

    public static IRoundingStrategy<TUnderlyingType> BankersRounding(Currency currency)
        => new BankersRounding<TUnderlyingType>(currency);

    public static IRoundingStrategy<TUnderlyingType> RoundWithAwayFromZero(TUnderlyingType precision)
        => new RoundWithAwayFromZero<TUnderlyingType>(precision);

    public static IRoundingStrategy<TUnderlyingType> RoundWithAwayFromZero(Currency currency)
        => new RoundWithAwayFromZero<TUnderlyingType>(currency);

    internal static IRoundingStrategy<TUnderlyingType> Default(TUnderlyingType precision)
        => BankersRounding(precision);

    internal static IRoundingStrategy<TUnderlyingType> Default(Currency currency)
        => BankersRounding(currency);
}
