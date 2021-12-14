using System;
using Funcky.Monads;

namespace Funcky;

public static class MoneyEvaluationExtension
{
    public static Money Evaluate(this IMoneyExpression moneyExpression, Option<MoneyEvaluationContext> context = default)
        => Evaluate(moneyExpression, Round(context), CalculateTotal(context));

    private static Money Evaluate(IMoneyExpression moneyExpression, Func<Money, Money> round, Func<IMoneyExpression, Money> total)
        => round(total(moneyExpression));

    private static Func<IMoneyExpression, Money> CalculateTotal(Option<MoneyEvaluationContext> context)
        => moneyExpression
            => moneyExpression
                .Accept(CreateVisitor(context))
                .CalculateTotal(context);

    private static Func<Money, Money> Round(Option<MoneyEvaluationContext> context)
        => money
            => money with { Amount = FindRoundingStrategy(money, context).Round(money.Amount) };

    private static EvaluationVisitor CreateVisitor(Option<MoneyEvaluationContext> context)
        => new(CreateDistributionStrategy(context), context);

    private static IDistributionStrategy CreateDistributionStrategy(Option<MoneyEvaluationContext> context)
        => new DefaultDistributionStrategy(context);

    private static IRoundingStrategy FindRoundingStrategy(Money money, Option<MoneyEvaluationContext> context)
        => context
            .AndThen(c => c.RoundingStrategy)
            .GetOrElse(money.RoundingStrategy);
}
