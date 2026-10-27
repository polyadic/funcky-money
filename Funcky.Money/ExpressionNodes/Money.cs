namespace Funcky;

public sealed record Money(decimal Amount, Currency Currency) : IMoneyExpression
{
    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
