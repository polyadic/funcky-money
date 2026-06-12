namespace Funcky;

internal sealed record MoneyQuotient(IMoneyExpression Expression, decimal Divisor) : IMoneyExpression
{
    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
