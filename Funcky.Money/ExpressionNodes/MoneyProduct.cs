using System.Numerics;

namespace Funcky;

internal sealed record MoneyProduct<TUnderlyingType> : IMoneyExpression<TUnderlyingType>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    public MoneyProduct(IMoneyExpression<TUnderlyingType> moneyExpression, TUnderlyingType factor)
    {
        Expression = moneyExpression;
        Factor = factor;
    }

    public IMoneyExpression<TUnderlyingType> Expression { get; }

    public TUnderlyingType Factor { get; }

    TState IMoneyExpression<TUnderlyingType>.Accept<TState>(IMoneyExpressionVisitor<TUnderlyingType, TState> visitor)
        => visitor.Visit(this);
}
