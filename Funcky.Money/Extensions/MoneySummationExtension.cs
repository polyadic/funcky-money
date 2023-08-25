using System.Numerics;

namespace Funcky;

public static class MoneySummationExtension
{
    public static IMoneyExpression<TUnderlyingType> Add<TUnderlyingType>(this IMoneyExpression<TUnderlyingType> augend, IMoneyExpression<TUnderlyingType> addend)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => new MoneySum<TUnderlyingType>(augend, addend);
}
