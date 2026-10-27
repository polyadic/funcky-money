using System.Globalization;
using Funcky.Extensions;

namespace Funcky;

public sealed partial record Money(decimal Amount, Currency Currency) : IMoneyExpression
{
    /// <summary>Parses a money in the format produced by <see cref="ToString"/> for the given currency.</summary>
    public static Option<Money> ParseOrNone(string money, Currency currency)
        => CurrencyCulture.FormatProvider(currency)
            .Match(
                none: () => money.ParseDecimalOrNone(NumberStyles.Currency, CultureInfo.CurrentCulture),
                some: culture => money.ParseDecimalOrNone(NumberStyles.Currency, CurrencyCulture.NumberFormat(currency, culture)))
            .Select(amount => new Money(amount, currency));

    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);

    /// <summary>Formats the money the way the culture of its currency does, e.g. "CHF-1’000.00" or "-$1,000.00".</summary>
    public override string ToString()
        => CurrencyCulture.FormatProvider(Currency).Match(
            none: () => $"{Amount.ToString($"N{Currency.MinorUnitDigits}", CultureInfo.CurrentCulture)} {Currency.AlphabeticCode}",
            some: culture => Amount.ToString("C", CurrencyCulture.NumberFormat(Currency, culture)));
}
