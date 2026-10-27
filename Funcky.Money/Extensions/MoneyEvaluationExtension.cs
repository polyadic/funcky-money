namespace Funcky;

public static class MoneyEvaluationExtension
{
    extension(IMoneyExpression expression)
    {
        /// <summary>
        /// Evaluates the expression into a single money. Rounding happens once, at the very end, with the rounding
        /// strategy of the context or, without a context, the one shared by all moneys of the expression.
        /// </summary>
        public Money Evaluate(Option<MoneyEvaluationContext> context = default)
            => Round(expression.Accept(new EvaluationVisitor(context)).ToMoney(context));
    }

    private static Money Round(Money money)
        => money with { Amount = money.RoundingStrategy.Round(money.Amount) };
}
