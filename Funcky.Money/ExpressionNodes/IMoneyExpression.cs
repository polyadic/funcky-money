namespace Funcky;

public interface IMoneyExpression
{
#if DEFAULT_INTERFACE_IMPLEMENTATION_SUPPORTED
    public static IMoneyExpression operator *(IMoneyExpression multiplicand, decimal multiplier)
        => multiplicand.Multiply(multiplier);

    public static IMoneyExpression operator *(decimal multiplier, IMoneyExpression multiplicand)
        => multiplicand.Multiply(multiplier);

    public static IMoneyExpression operator /(IMoneyExpression dividend, decimal divisor)
        => dividend.Divide(divisor);

    public static IMoneyExpression operator +(IMoneyExpression augend, IMoneyExpression addend)
        => augend.Add(addend);

    public static IMoneyExpression operator +(IMoneyExpression moneyExpression)
        => moneyExpression;

    public static IMoneyExpression operator -(IMoneyExpression minuend, IMoneyExpression subtrahend)
        => minuend.Subtract(subtrahend);

    public static IMoneyExpression operator -(IMoneyExpression moneyExpression)
        => moneyExpression.Multiply(-1);
#endif

    internal TState Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
        where TState : notnull;
}
