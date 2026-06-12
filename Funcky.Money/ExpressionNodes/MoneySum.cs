namespace Funcky;

internal sealed record MoneySum : IMoneyExpression
{
    public MoneySum(IMoneyExpression augend, IMoneyExpression addend)
    {
        Augend = augend;
        Addend = addend;
    }

    public IMoneyExpression Augend { get; }

    public IMoneyExpression Addend { get; }

    TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        => visitor.Visit(this);
}
