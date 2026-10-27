namespace Funcky;

public sealed record MoneyProduct(IMoneyExpression Expression, decimal Factor) : IMoneyExpression
{
    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
