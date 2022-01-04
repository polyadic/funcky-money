using Funcky.Extensions;

namespace Funcky;

public static class ToHumanReadableExtension
{
    public static string ToHumanReadable(this MoneyExpression moneyExpression)
        => moneyExpression.Match(
            money: FormatMoney,
            moneySum: FormatSum,
            moneyProduct: FormatProduct,
            moneyDistributionPart: FormatDistributionPart);

    private static string FormatDistributionPart(MoneyExpression.MoneyDistributionPart d)
        => $"{ToHumanReadable(d.Distribution.Expression)}.Distribute({d.Distribution.Factors.JoinToString(", ")})[{d.Index}]";

    private static string FormatProduct(MoneyExpression.MoneyProduct p)
        => $"({p.Factor} * {ToHumanReadable(p.Expression)})";

    private static string FormatSum(MoneyExpression.MoneySum s)
        => $"({ToHumanReadable(s.Left)} + {ToHumanReadable(s.Right)})";

    private static string FormatMoney(MoneyExpression.Money m)
        => string.Format($"{{0:N{m.Currency.MinorUnitDigits}}}{{1}}", m.Amount, m.Currency.AlphabeticCurrencyCode);
}
