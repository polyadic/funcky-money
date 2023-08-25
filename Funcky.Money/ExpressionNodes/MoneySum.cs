using System.Numerics;

namespace Funcky;

internal sealed record MoneySum<TUnderlyingType> : IMoneyExpression<TUnderlyingType>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    public MoneySum(IMoneyExpression<TUnderlyingType> leftMoneyExpression, IMoneyExpression<TUnderlyingType> rightMoneyExpression)
    {
        Left = leftMoneyExpression;
        Right = rightMoneyExpression;
    }

    public IMoneyExpression<TUnderlyingType> Left { get; }

    public IMoneyExpression<TUnderlyingType> Right { get; }

    TState IMoneyExpression<TUnderlyingType>.Accept<TState>(IMoneyExpressionVisitor<TUnderlyingType, TState> visitor)
        => visitor.Visit(this);
}
