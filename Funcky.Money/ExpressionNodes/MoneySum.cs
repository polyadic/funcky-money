namespace Funcky;

internal sealed record MoneySum(IMoneyExpression Augend, IMoneyExpression Addend) : IMoneyExpression
{
    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
