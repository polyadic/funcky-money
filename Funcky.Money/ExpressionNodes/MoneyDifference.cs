namespace Funcky;

public sealed record MoneyDifference(IMoneyExpression Minuend, IMoneyExpression Subtrahend) : IMoneyExpression
{
    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
