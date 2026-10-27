namespace Funcky;

public static class MoneyMultiplicationExtension
{
    extension(IMoneyExpression multiplicand)
    {
        public static IMoneyExpression operator *(IMoneyExpression expression, decimal factor)
            => expression.Multiply(factor);

        public static IMoneyExpression operator *(decimal factor, IMoneyExpression expression)
            => expression.Multiply(factor);

        public IMoneyExpression Multiply(decimal factor)
            => new MoneyProduct(multiplicand, factor);
    }
}
