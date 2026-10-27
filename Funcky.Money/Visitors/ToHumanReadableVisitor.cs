using System.Globalization;

namespace Funcky;

/// <summary>Renders an expression tree as text, e.g. "((2 * 7.00CHF) + 0.50CHF)".</summary>
internal sealed class ToHumanReadableVisitor : IMoneyExpressionVisitor<string>
{
    public static readonly ToHumanReadableVisitor Instance = new();

    private ToHumanReadableVisitor()
    {
    }

    public string Visit(Money money)
        => $"{money.Amount.ToString($"F{money.Currency.MinorUnitDigits}", CultureInfo.InvariantCulture)}{money.Currency.AlphabeticCode}";

    public string Visit(MoneySum sum)
        => Binary(sum.Left, "+", sum.Right);

    public string Visit(MoneyDifference difference)
        => Binary(difference.Minuend, "-", difference.Subtrahend);

    public string Visit(MoneyProduct product)
        => $"({Number(product.Factor)} * {product.Expression.Accept(this)})";

    public string Visit(MoneyQuotient quotient)
        => $"({quotient.Dividend.Accept(this)} / {Number(quotient.Divisor)})";

    public string Visit(MoneyDistributionPart part)
        => $"{part.Distribution.Expression.Accept(this)}.Distribute({string.Join(", ", part.Distribution.Factors)})[{part.Index}]";

    private static string Number(decimal number)
        => number.ToString(CultureInfo.InvariantCulture);

    private string Binary(IMoneyExpression left, string operation, IMoneyExpression right)
        => $"({left.Accept(this)} {operation} {right.Accept(this)})";
}
