namespace Funcky;

internal class ToHumanReadableVisitor : IMoneyExpressionVisitor<string>
{
    private static readonly Lazy<ToHumanReadableVisitor> LazyInstance = new(() => new());

    public static ToHumanReadableVisitor Instance
        => LazyInstance.Value;

    public string Visit(Money money)
        => string.Format($"{{0:N{money.Currency.MinorUnitDigits}}}{{1}}", money.Amount, money.Currency.AlphabeticCurrencyCode);

    public string Visit(MoneySum sum)
        => $"({Accept(sum.Left)} + {Accept(sum.Right)})";

    public string Visit(MoneyProduct product)
        => $"({product.Factor} * {Accept(product.Expression)})";

    public string Visit(MoneyDistributionPart part)
        => $"{Accept(part.Distribution.Expression)}.Distribute({FormatFactors(part.Distribution.Factors)})[{part.Index}]";

    private string Accept(IMoneyExpression expression)
        => expression.Accept(this);

    private string FormatFactors(IEnumerable<int> factors)
        => string.Join(", ", factors);
}
