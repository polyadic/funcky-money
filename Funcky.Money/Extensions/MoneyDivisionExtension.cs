namespace Funcky;

public static class MoneyDivisionExtension
{
    extension(IMoneyExpression dividend)
    {
        public static IMoneyExpression operator /(IMoneyExpression expression, decimal divisor)
            => expression.Divide(divisor);

        public IMoneyExpression Divide(decimal divisor)
            => new MoneyQuotient(dividend, divisor);
    }
}
