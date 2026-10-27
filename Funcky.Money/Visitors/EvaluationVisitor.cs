namespace Funcky;

internal sealed class EvaluationVisitor(Option<MoneyEvaluationContext> context) : IMoneyExpressionVisitor<Money>
{
    public Money Visit(Money money)
        => money;

    public Money Visit(MoneySum sum)
        => Add(sum.Left.Accept(this), sum.Right.Accept(this));

    public Money Visit(MoneyProduct product)
        => Multiply(product.Expression.Accept(this), product.Factor);

    public Money Visit(MoneyDistributionPart part)
        => Distribute(part.Distribution.Expression.Accept(this), part);

    private static Money Multiply(Money money, decimal factor)
        => money with { Amount = money.Amount * factor };

    private Money Distribute(Money total, MoneyDistributionPart part)
        => total with { Amount = DefaultDistributionStrategy.Part(total.Amount, part.Distribution.Factors, part.Index, DistributionPrecision(part.Distribution, total.Currency)) };

    private decimal DistributionPrecision(MoneyDistribution distribution, Currency currency)
        => distribution.Precision
            .OrElse(context.SelectMany(c => c.SmallestDistributionUnit))
            .GetOrElse(Power.OfATenth(currency.MinorUnitDigits));

    private Money Add(Money left, Money right)
        => left.Currency == right.Currency
            ? left with { Amount = left.Amount + right.Amount }
            : throw MissingExchangeRate(left.Currency, right.Currency);

    private Exception MissingExchangeRate(Currency source, Currency target)
        => context.Match<Exception>(
            none: () => new MissingEvaluationContextException(),
            some: _ => new MissingExchangeRateException(source, target));
}
