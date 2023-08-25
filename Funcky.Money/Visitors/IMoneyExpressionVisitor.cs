using System.Numerics;

namespace Funcky;

internal interface IMoneyExpressionVisitor<TUnderlyingType, out TState>
    where TState : notnull
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    TState Visit(Money<TUnderlyingType> money);

    TState Visit(MoneySum<TUnderlyingType> sum);

    TState Visit(MoneyProduct<TUnderlyingType> product);

    TState Visit(MoneyDistributionPart<TUnderlyingType> part);
}
