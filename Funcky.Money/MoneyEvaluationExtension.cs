using Funcky.Monads;

namespace Funcky
{
    public static class MoneyEvaluationExtension
    {
        public static Money Evaluate(this IMoneyExpression moneyExpression, Option<MoneyEvaluationContext> context = default)
        {
            var visitor = new EvaluationVisitor(context);

            moneyExpression.Accept(visitor);

            return visitor.Result;
        }
    }
}
