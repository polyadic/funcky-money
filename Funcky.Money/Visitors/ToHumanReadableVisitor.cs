using System.Numerics;
using Funcky.Extensions;

namespace Funcky;

internal class ToHumanReadableVisitor<TUnderlyingType> : IMoneyExpressionVisitor<TUnderlyingType, string>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    private const string DistributionSeparator = ", ";
    private static readonly Lazy<ToHumanReadableVisitor<TUnderlyingType>> LazyInstance = new(() => new());

    public static ToHumanReadableVisitor<TUnderlyingType> Instance
        => LazyInstance.Value;

    public string Visit(Money<TUnderlyingType> money)
        => string.Format($"{{0:N{money.Currency.MinorUnitDigits}}}{{1}}", money.Amount, money.Currency.AlphabeticCurrencyCode);

    public string Visit(MoneySum<TUnderlyingType> sum)
        => $"({Accept(sum.Left)} + {Accept(sum.Right)})";

    public string Visit(MoneyProduct<TUnderlyingType> product)
        => $"({product.Factor} * {Accept(product.Expression)})";

    public string Visit(MoneyDistributionPart<TUnderlyingType> part)
        => $"{Accept(part.Distribution.Expression)}.Distribute({part.Distribution.Factors.JoinToString(DistributionSeparator)})[{part.Index}]";

    private string Accept(IMoneyExpression<TUnderlyingType> expression)
        => expression.Accept(this);
}
