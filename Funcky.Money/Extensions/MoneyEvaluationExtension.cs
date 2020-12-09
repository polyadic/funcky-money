using Funcky.Monads;

namespace Funcky
{
    public static class MoneyEvaluationExtension
    {
        public static Money Evaluate(this IMoneyExpression moneyExpression, Option<MoneyEvaluationContext> context = default)
        {
            var visitor = CreateEvaluationVisitor(context);

            moneyExpression.Accept(visitor);

            return visitor.Result;
        }

        private static EvaluationVisitor CreateEvaluationVisitor(Option<MoneyEvaluationContext> context)
            => new(CreateDistributionStrategy(context), context);

        private static IDistributionStrategy CreateDistributionStrategy(Option<MoneyEvaluationContext> context)
            => new DefaultDistributionStrategy(context);
    }
}
