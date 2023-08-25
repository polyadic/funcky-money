using System.Numerics;
using Funcky.Monads;

namespace Funcky;

public static class MoneyDivisionExtension
{
    public static IMoneyExpression<TUnderlyingType> Divide<TUnderlyingType>(this IMoneyExpression<TUnderlyingType> dividend, TUnderlyingType divisor)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => new MoneyProduct<TUnderlyingType>(dividend, TUnderlyingType.One / divisor);

    public static TUnderlyingType Divide<TUnderlyingType>(this IMoneyExpression<TUnderlyingType> dividend, IMoneyExpression<TUnderlyingType> divisor, Option<MoneyEvaluationContext<TUnderlyingType>> context = default)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => Divide(
            dividend.Evaluate(context),
            divisor.Evaluate(context));

    private static TUnderlyingType Divide<TUnderlyingType>(Money<TUnderlyingType> dividend, Money<TUnderlyingType> divisor)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => dividend.Currency == divisor.Currency
            ? dividend.Amount / divisor.Amount
            : throw new MissingExchangeRateException();
}
