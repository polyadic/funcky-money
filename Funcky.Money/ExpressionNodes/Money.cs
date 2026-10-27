using System.Globalization;
using Funcky.Extensions;

namespace Funcky;

/// <summary>
/// An amount of a currency. Two moneys are equal if they have the same amount and currency; the rounding strategy
/// only influences evaluation.
/// </summary>
public sealed partial record Money : IMoneyExpression
{
    /// <summary>Creates a money in the currency of the current culture.</summary>
    /// <exception cref="NotSupportedException">The current culture has no known currency.</exception>
    public Money(decimal amount)
        : this(amount, CurrencyCulture.CurrentCurrency().GetOrElse(() => throw new NotSupportedException("The current culture has no known currency, specify it explicitly.")))
    {
    }

    public Money(decimal amount, Currency currency)
        : this(amount, currency, Funcky.RoundingStrategy.Default(currency))
    {
    }

    /// <summary>Creates a money in the target currency of the context, which also provides the rounding strategy.</summary>
    public Money(decimal amount, MoneyEvaluationContext context)
        : this(amount, context.TargetCurrency, context.RoundingStrategy)
    {
    }

    internal Money(decimal amount, Currency currency, IRoundingStrategy roundingStrategy)
        => (Amount, Currency, RoundingStrategy) = (amount, currency, roundingStrategy);

    public decimal Amount { get; init; }

    public Currency Currency { get; init; }

    public IRoundingStrategy RoundingStrategy { get; init; }

    /// <summary>Parses a money in the format produced by <see cref="ToString"/> for the given currency.</summary>
    public static Option<Money> ParseOrNone(string money, Currency currency)
        => CurrencyCulture.FormatProvider(currency)
            .Match(
                none: () => ParseWithoutFormatProvider(money, currency),
                some: culture => money.ParseDecimalOrNone(NumberStyles.Currency, CurrencyCulture.NumberFormat(currency, culture)))
            .Select(amount => new Money(amount, currency));

    public TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);

    public bool Equals(Money? other)
        => other is not null && Amount == other.Amount && Currency == other.Currency;

    public override int GetHashCode()
        => HashCode.Combine(Amount, Currency);

    /// <summary>Formats the money the way the culture of its currency does, e.g. "CHF-1’000.00" or "-$1,000.00".</summary>
    public override string ToString()
        => CurrencyCulture.FormatProvider(Currency).Match(
            none: () => FormatWithoutFormatProvider(Amount, Currency),
            some: culture => Amount.ToString("C", CurrencyCulture.NumberFormat(Currency, culture)));

    private static string FormatWithoutFormatProvider(decimal amount, Currency currency)
        => $"{amount.ToString($"N{currency.MinorUnitDigits}", CultureInfo.CurrentCulture)} {currency.AlphabeticCode}";

    private static Option<decimal> ParseWithoutFormatProvider(string money, Currency currency)
        => RemoveCurrencyCode(money, currency).ParseDecimalOrNone(NumberStyles.Currency, CultureInfo.CurrentCulture);

    private static string RemoveCurrencyCode(string money, Currency currency)
        => money.Replace(currency.AlphabeticCode, string.Empty).Trim();
}
