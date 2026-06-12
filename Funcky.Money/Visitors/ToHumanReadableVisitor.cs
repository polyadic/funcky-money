using Funcky.Extensions;

namespace Funcky;

internal sealed class ToHumanReadableVisitor : IMoneyExpressionVisitor<string>
{
    private const string DistributionSeparator = ", ";
    private static readonly Lazy<ToHumanReadableVisitor> LazyInstance = new(() => new());

    public static ToHumanReadableVisitor Instance
        => LazyInstance.Value;

    public string Visit(Money money)
        => string.Format($"{{0:N{money.Currency.MinorUnitDigits}}}{{1}}", money.Amount, money.Currency.AlphabeticCurrencyCode);

    public string Visit(MoneySum sum)
        => $"({Accept(sum.Augend)} + {Accept(sum.Addend)})";

    public string Visit(MoneyDifference difference)
        => $"({Accept(difference.Minuend)} - {Accept(difference.Subtrahend)})";

    public string Visit(MoneyProduct product)
        => $"({product.Factor} * {Accept(product.Expression)})";

    public string Visit(MoneyQuotient quotient)
        => $"({Accept(quotient.Expression)} / {quotient.Divisor})";

    public string Visit(MoneyDistributionPart part)
        => $"{Accept(part.Distribution.Expression)}.Distribute({part.Distribution.Factors.JoinToString(DistributionSeparator)})[{part.Index}]";

    private string Accept(IMoneyExpression expression)
        => expression.Accept(this);
}
