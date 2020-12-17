using System.Collections.Generic;
using Funcky.Monads;

namespace Funcky
{
    public static class MoneyEvaluationExtension
    {
        public static Money Evaluate(this IMoneyExpression moneyExpression, Option<MoneyEvaluationContext> context = default)
            => Round(CalculateTotal(moneyExpression, context), context);

        private static Money CalculateTotal(IMoneyExpression moneyExpression, Option<MoneyEvaluationContext> context)
            => moneyExpression
                .Accept(EvaluationVisitor.Instance, CreateState(context))
                .MoneyBags
                .Peek()
                .CalculateTotal(context);

        private static EvaluationVisitor.State CreateState(Option<MoneyEvaluationContext> context)
            => new(CreateDistributionStrategy(context), context, CreateInitialStack());

        private static Stack<MoneyBag> CreateInitialStack()
        {
            var stack = new Stack<MoneyBag>();

            stack.Push(new MoneyBag());

            return stack;
        }

        private static IDistributionStrategy CreateDistributionStrategy(Option<MoneyEvaluationContext> context)
            => new DefaultDistributionStrategy(context);

        private static Money Round(Money money, Option<MoneyEvaluationContext> context)
            => money with { Amount = FindRoundingStrategy(money, context).Round(money.Amount) };

        private static IRoundingStrategy FindRoundingStrategy(Money money, Option<MoneyEvaluationContext> context)
            => context
                .AndThen(c => c.RoundingStrategy)
                .GetOrElse(money.RoundingStrategy);
    }
}
