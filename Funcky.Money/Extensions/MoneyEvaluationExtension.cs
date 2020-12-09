using Funcky.Monads;

namespace Funcky
{
    public static class MoneyEvaluationExtension
    {
        public static Money Evaluate(this IMoneyExpression moneyExpression, Option<MoneyEvaluationContext> context = default)
        {
            var distributionStrategy = new DefaultDistributionStrategy(context);
            var visitor = new EvaluationVisitor(distributionStrategy, context);

            moneyExpression.Accept(visitor);

            return visitor.Result;
        }
    }
}
