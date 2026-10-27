using System.Globalization;

namespace Funcky;

public sealed partial record Money(decimal Amount, Currency Currency) : IMoneyExpression
{
    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);

    /// <summary>Formats the money the way the culture of its currency does, e.g. "CHF-1’000.00" or "-$1,000.00".</summary>
    public override string ToString()
        => CurrencyCulture.FormatProvider(Currency).Match(
            none: () => $"{Amount.ToString($"N{Currency.MinorUnitDigits}", CultureInfo.CurrentCulture)} {Currency.AlphabeticCode}",
            some: culture => Amount.ToString("C", CurrencyCulture.NumberFormat(Currency, culture)));
}
