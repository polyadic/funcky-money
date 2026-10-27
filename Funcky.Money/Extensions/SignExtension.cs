namespace Funcky;

public static class SignExtension
{
    extension(IMoneyExpression)
    {
        public static IMoneyExpression operator +(IMoneyExpression expression)
            => expression;

        public static IMoneyExpression operator -(IMoneyExpression expression)
            => expression.Multiply(-1);
    }
}
