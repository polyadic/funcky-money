namespace Funcky;

internal sealed record MoneyProduct(IMoneyExpression Expression, decimal Factor) : IMoneyExpression
{
    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
