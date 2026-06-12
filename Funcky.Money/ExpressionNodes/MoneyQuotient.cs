namespace Funcky;

internal sealed record MoneyQuotient : IMoneyExpression
{
    public MoneyQuotient(IMoneyExpression moneyExpression, decimal divisor)
    {
        Expression = moneyExpression;
        Divisor = divisor;
    }

    public IMoneyExpression Expression { get; }

    public decimal Divisor { get; }

    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
