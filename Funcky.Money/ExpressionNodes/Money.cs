namespace Funcky;

public sealed record Money(decimal Amount, Currency Currency) : IMoneyExpression
{
    public static Money CHF(decimal amount)
        => new(amount, Currency.CHF);

    public static Money USD(decimal amount)
        => new(amount, Currency.USD);

    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
