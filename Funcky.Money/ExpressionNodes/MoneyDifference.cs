namespace Funcky;

internal sealed record MoneyDifference(IMoneyExpression Minuend, IMoneyExpression Subtrahend) : IMoneyExpression
{
    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
