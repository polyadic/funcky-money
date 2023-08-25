using System.Numerics;
using Funcky.Monads;

namespace Funcky;

public static class MoneyDistributionExtension
{
    public static IEnumerable<IMoneyExpression<TUnderlyingType>> Distribute<TUnderlyingType>(this IMoneyExpression<TUnderlyingType> moneyExpression, int numberOfSlices, Option<TUnderlyingType> precision = default)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => moneyExpression.Distribute(Enumerable.Repeat(element: 1, count: numberOfSlices), precision);

    public static IEnumerable<IMoneyExpression<TUnderlyingType>> Distribute<TUnderlyingType>(this IMoneyExpression<TUnderlyingType> moneyExpression, IEnumerable<int> factors, Option<TUnderlyingType> precision = default)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => new MoneyDistribution<TUnderlyingType>(moneyExpression, factors, precision);
}
