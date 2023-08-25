using System.Numerics;

namespace Funcky;

public interface IMoneyExpression<TUnderlyingType>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    public static IMoneyExpression<TUnderlyingType> operator *(IMoneyExpression<TUnderlyingType> multiplicand, TUnderlyingType multiplier)
        => multiplicand.Multiply(multiplier);

    public static IMoneyExpression<TUnderlyingType> operator *(TUnderlyingType multiplier, IMoneyExpression<TUnderlyingType> multiplicand)
        => multiplicand.Multiply(multiplier);

    public static IMoneyExpression<TUnderlyingType> operator /(IMoneyExpression<TUnderlyingType> dividend, TUnderlyingType divisor)
        => dividend.Divide(divisor);

    public static TUnderlyingType operator /(IMoneyExpression<TUnderlyingType> dividend, IMoneyExpression<TUnderlyingType> divisor)
        => dividend.Divide(divisor);

    public static IMoneyExpression<TUnderlyingType> operator +(IMoneyExpression<TUnderlyingType> augend, IMoneyExpression<TUnderlyingType> addend)
        => augend.Add(addend);

    public static IMoneyExpression<TUnderlyingType> operator +(IMoneyExpression<TUnderlyingType> moneyExpression)
        => moneyExpression;

    public static IMoneyExpression<TUnderlyingType> operator -(IMoneyExpression<TUnderlyingType> minuend, IMoneyExpression<TUnderlyingType> subtrahend)
        => minuend.Subtract(subtrahend);

    public static IMoneyExpression<TUnderlyingType> operator -(IMoneyExpression<TUnderlyingType> moneyExpression)
        => moneyExpression.Multiply(TUnderlyingType.NegativeOne);

    internal TState Accept<TState>(IMoneyExpressionVisitor<TUnderlyingType, TState> visitor)
        where TState : notnull;
}
