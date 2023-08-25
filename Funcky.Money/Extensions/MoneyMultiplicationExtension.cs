using System.Numerics;

namespace Funcky;

public static class MoneyMultiplicationExtension
{
    public static IMoneyExpression<TUnderlyingType> Multiply<TUnderlyingType>(this IMoneyExpression<TUnderlyingType> multiplicand, TUnderlyingType multiplier)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => new MoneyProduct<TUnderlyingType>(multiplicand, multiplier);
}
