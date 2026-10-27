namespace Funcky;

internal sealed class EvaluationVisitor(Option<MoneyEvaluationContext> context) : IMoneyExpressionVisitor<MoneyBag>
{
    public MoneyBag Visit(Money money)
        => MoneyBag.Of(money);

    public MoneyBag Visit(MoneySum sum)
        => sum.Left.Accept(this).Merge(sum.Right.Accept(this));

    public MoneyBag Visit(MoneyDifference difference)
        => difference.Minuend.Accept(this).Merge(difference.Subtrahend.Accept(this).Multiply(-1));

    public MoneyBag Visit(MoneyProduct product)
        => product.Expression.Accept(this).Multiply(product.Factor);

    public MoneyBag Visit(MoneyQuotient quotient)
        => quotient.Dividend.Accept(this).Divide(quotient.Divisor);

    public MoneyBag Visit(MoneyDistributionPart part)
        => MoneyBag.Of(Distribute(part.Distribution.Expression.Accept(this).ToMoney(context), part));

    private Money Distribute(Money total, MoneyDistributionPart part)
        => total with { Amount = DefaultDistributionStrategy.Part(total.Amount, part.Distribution.Factors, part.Index, DistributionPrecision(part.Distribution, total.Currency)) };

    private decimal DistributionPrecision(MoneyDistribution distribution, Currency currency)
        => distribution.Precision
            .OrElse(context.SelectMany(c => c.SmallestDistributionUnit))
            .GetOrElse(Power.OfATenth(currency.MinorUnitDigits));
}
