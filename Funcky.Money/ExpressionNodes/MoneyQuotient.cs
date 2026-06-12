namespace Funcky;

internal sealed record MoneyQuotient(IMoneyExpression Expression, decimal Factor) : IMoneyExpression
{
    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
