using System.Numerics;

namespace Funcky;

internal sealed class MoneyDistributionPart<TUnderlyingType> : IMoneyExpression<TUnderlyingType>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    public MoneyDistributionPart(MoneyDistribution<TUnderlyingType> distribution, int index)
    {
        Distribution = distribution;
        Index = index;
    }

    public MoneyDistribution<TUnderlyingType> Distribution { get; }

    public int Index { get; }

    TState IMoneyExpression<TUnderlyingType>.Accept<TState>(IMoneyExpressionVisitor<TUnderlyingType, TState> visitor)
        => visitor.Visit(this);
}
