namespace Funcky;

public static class MoneyEvaluationExtension
{
    extension(IMoneyExpression expression)
    {
        public Money Evaluate(Option<MoneyEvaluationContext> context = default)
            => expression.Accept(new EvaluationVisitor(context));
    }
}
