using System.Numerics;

namespace Funcky;

public static class MoneySubtractionExtension
{
    public static IMoneyExpression<TUnderlyingType> Subtract<TUnderlyingType>(this IMoneyExpression<TUnderlyingType> minuend, IMoneyExpression<TUnderlyingType> subtrahend)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => new MoneySum<TUnderlyingType>(minuend, new MoneyProduct<TUnderlyingType>(subtrahend, TUnderlyingType.NegativeOne));
}
