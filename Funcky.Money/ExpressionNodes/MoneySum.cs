namespace Funcky;

public sealed record MoneySum(IMoneyExpression Left, IMoneyExpression Right) : IMoneyExpression
{
    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
