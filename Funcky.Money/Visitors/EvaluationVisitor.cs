namespace Funcky;

internal sealed class EvaluationVisitor(Option<MoneyEvaluationContext> context) : IMoneyExpressionVisitor<Money>
{
    public Money Visit(Money money)
        => money;

    public Money Visit(MoneySum sum)
        => Add(sum.Left.Accept(this), sum.Right.Accept(this));

    public Money Visit(MoneyProduct product)
        => Multiply(product.Expression.Accept(this), product.Factor);

    private static Money Multiply(Money money, decimal factor)
        => money with { Amount = money.Amount * factor };

    private Money Add(Money left, Money right)
        => left.Currency == right.Currency
            ? left with { Amount = left.Amount + right.Amount }
            : throw MissingExchangeRate(left.Currency, right.Currency);

    private Exception MissingExchangeRate(Currency source, Currency target)
        => context.Match<Exception>(
            none: () => new MissingEvaluationContextException(),
            some: _ => new MissingExchangeRateException(source, target));
}
