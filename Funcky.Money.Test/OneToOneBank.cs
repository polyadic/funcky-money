using System.Numerics;

namespace Funcky.Test;

// Bank which returns a 1:1 exchange rate for every pair of currencies.
internal sealed class OneToOneBank<TUnderlyingType> : IBank<TUnderlyingType>
    where TUnderlyingType : INumberBase<TUnderlyingType>
{
    public static readonly IBank<TUnderlyingType> Instance = new OneToOneBank<TUnderlyingType>();

    public TUnderlyingType ExchangeRate(Currency source, Currency target)
        => TUnderlyingType.One;
}
