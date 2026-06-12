namespace Funcky;

internal sealed class MoneyDistributionPart(MoneyDistribution distribution, int index) : IMoneyExpression
{
    public MoneyDistribution Distribution { get; } = distribution;

    public int Index { get; } = index;

    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
