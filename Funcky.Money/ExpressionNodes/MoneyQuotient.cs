namespace Funcky;

public sealed record MoneyQuotient(IMoneyExpression Dividend, decimal Divisor) : IMoneyExpression
{
    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
