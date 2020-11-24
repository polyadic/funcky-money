using System;

namespace Funcky
{
    public static class MoneyEvaluation
    {
        public static Money Evaluate(this IMoneyExpression moneyExpression)
            => moneyExpression switch
            {
                Money money => money,
                MoneySum sum => Evaluate(sum),
                MoneyProduct product => Evaluate(product),
                _ => throw new NotImplementedException(),
            };

        private static Money Evaluate(MoneySum sum)
        {
            var left = sum.Left.Evaluate();
            var right = sum.Right.Evaluate();

            return SameEvaluationTarget(left, right)
                ? new Money(left.Amount + right.Amount)
                : throw new MissingEvaluationContextException();
        }

        private static Money Evaluate(MoneyProduct product)
            => new Money(product.Expression.Evaluate().Amount * product.Factor);

        private static bool SameEvaluationTarget(Money left, Money right)
            => left.Currency == right.Currency;
    }
}
