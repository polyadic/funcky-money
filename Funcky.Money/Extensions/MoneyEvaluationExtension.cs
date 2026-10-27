namespace Funcky;

public static class MoneyEvaluationExtension
{
    extension(IMoneyExpression expression)
    {
        public Money Evaluate(Option<MoneyEvaluationContext> context = default)
            => RoundToMinorUnit(expression.Accept(new EvaluationVisitor(context)).ToMoney(context));
    }

    private static Money RoundToMinorUnit(Money money)
        => money with { Amount = decimal.Round(money.Amount, money.Currency.MinorUnitDigits, MidpointRounding.ToEven) };
}
