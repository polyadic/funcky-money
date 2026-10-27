namespace Funcky;

public sealed record MoneyDistributionPart(MoneyDistribution Distribution, int Index) : IMoneyExpression
{
    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
