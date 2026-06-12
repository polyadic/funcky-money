namespace Funcky;

internal sealed record MoneySum(IMoneyExpression Left, IMoneyExpression Right) : IMoneyExpression
{
    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
