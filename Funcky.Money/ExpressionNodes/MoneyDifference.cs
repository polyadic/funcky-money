namespace Funcky;

internal sealed record MoneyDifference : IMoneyExpression
{
    public MoneyDifference(IMoneyExpression minuend, IMoneyExpression subtrahend)
    {
        Minuend = minuend;
        Subtrahend = subtrahend;
    }

    public IMoneyExpression Minuend { get; }

    public IMoneyExpression Subtrahend { get; }

    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
