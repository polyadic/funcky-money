using Funcky.Monads;

namespace Funcky;

public static class MoneyDivisionExtension
{
    public static IMoneyExpression Divide(this IMoneyExpression dividend, decimal divisor)
        => divisor is 0.0m
            ? throw new DivideByZeroException()
            : new MoneyQuotient(dividend, divisor);

    public static decimal Divide(this IMoneyExpression dividend, IMoneyExpression divisor, Option<MoneyEvaluationContext> context = default)
        => Divide(
            dividend.Evaluate(context),
            divisor.Evaluate(context));

    private static decimal Divide(Money dividend, Money divisor)
        => dividend.Currency == divisor.Currency
            ? dividend.Amount / divisor.Amount
            : throw new MissingExchangeRateException();
}
