using System.Numerics;
using Funcky.Monads;

namespace Funcky;

public static class MoneyEvaluationExtension
{
    public static Money<TUnderlyingType> Evaluate<TUnderlyingType>(this IMoneyExpression<TUnderlyingType> moneyExpression, Option<MoneyEvaluationContext<TUnderlyingType>> context = default)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => Evaluate(moneyExpression, Round(context), CalculateTotal(context));

    private static Money<TUnderlyingType> Evaluate<TUnderlyingType>(IMoneyExpression<TUnderlyingType> moneyExpression, Func<Money<TUnderlyingType>, Money<TUnderlyingType>> round, Func<IMoneyExpression<TUnderlyingType>, Money<TUnderlyingType>> total)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => round(total(moneyExpression));

    private static Func<IMoneyExpression<TUnderlyingType>, Money<TUnderlyingType>> CalculateTotal<TUnderlyingType>(Option<MoneyEvaluationContext<TUnderlyingType>> context)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => moneyExpression
            => moneyExpression
                .Accept(CreateVisitor(context))
                .CalculateTotal(context);

    private static Func<Money<TUnderlyingType>, Money<TUnderlyingType>> Round<TUnderlyingType>(Option<MoneyEvaluationContext<TUnderlyingType>> context)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType> => money
        => money with { Amount = FindRoundingStrategy(money, context).Round(money.Amount) };

    private static EvaluationVisitor<TUnderlyingType> CreateVisitor<TUnderlyingType>(Option<MoneyEvaluationContext<TUnderlyingType>> context)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => new(CreateDistributionStrategy(context), context);

    private static IDistributionStrategy<TUnderlyingType> CreateDistributionStrategy<TUnderlyingType>(Option<MoneyEvaluationContext<TUnderlyingType>> context)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => new DefaultDistributionStrategy<TUnderlyingType>(context);

    private static IRoundingStrategy<TUnderlyingType> FindRoundingStrategy<TUnderlyingType>(Money<TUnderlyingType> money, Option<MoneyEvaluationContext<TUnderlyingType>> context)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => context
            .AndThen(c => c.RoundingStrategy)
            .GetOrElse(money.RoundingStrategy);
}
