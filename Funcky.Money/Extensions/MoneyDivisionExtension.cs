namespace Funcky;

public static class MoneyDivisionExtension
{
    extension(IMoneyExpression dividend)
    {
        public static IMoneyExpression operator /(IMoneyExpression expression, decimal divisor)
            => expression.Divide(divisor);

        public static decimal operator /(IMoneyExpression expression, IMoneyExpression divisor)
            => expression.Divide(divisor);

        public IMoneyExpression Divide(decimal divisor)
            => new MoneyQuotient(dividend, divisor);

        /// <summary>
        /// Divides two moneys into a dimensionless factor. Both expressions are evaluated eagerly; a context is
        /// required when they are not of the same currency.
        /// </summary>
        /// <exception cref="MissingExchangeRateException">The moneys have different currencies and no context converts them.</exception>
        /// <exception cref="DivideByZeroException">The divisor evaluates to zero.</exception>
        public decimal Divide(IMoneyExpression divisor, Option<MoneyEvaluationContext> context = default)
            => Divide(dividend.Evaluate(context), divisor.Evaluate(context));
    }

    private static decimal Divide(Money dividend, Money divisor)
        => dividend.Currency == divisor.Currency
            ? dividend.Amount / divisor.Amount
            : throw new MissingExchangeRateException(divisor.Currency, dividend.Currency);
}
