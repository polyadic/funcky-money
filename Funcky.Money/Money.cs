using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Funcky.Monads;

namespace Funcky;

[DebuggerDisplay("{Amount} {Currency.AlphabeticCurrencyCode,nq}")]
public sealed partial record Money<TUnderlyingType> : IMoneyExpression<TUnderlyingType>
where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    public static readonly Money<TUnderlyingType> Zero = new(TUnderlyingType.Zero);

    public Money(TUnderlyingType amount, Option<Currency> currency = default)
    {
        Amount = amount;
        Currency = SelectCurrency(currency);
        RoundingStrategy = RoundingStrategy<TUnderlyingType>.Default(Currency);
    }

    public Money(TUnderlyingType amount, MoneyEvaluationContext<TUnderlyingType> context)
    {
        Amount = amount;
        Currency = context.TargetCurrency;
        RoundingStrategy = context.RoundingStrategy;
    }

    public Money(int amount, Option<Currency> currency = default)
        : this(TUnderlyingType.CreateChecked(amount), currency)
    {
    }

    public TUnderlyingType Amount { get; init; }

    public Currency Currency { get; init; }

    public IRoundingStrategy<TUnderlyingType> RoundingStrategy { get; }

    public bool IsZero
        => TUnderlyingType.IsZero(Amount);

    // These operators supports the operators on IMoneyExpression, because Money + Money or Money * factor does not work otherwise without a cast.
    public static IMoneyExpression<TUnderlyingType> operator +(Money<TUnderlyingType> augend, IMoneyExpression<TUnderlyingType> addend)
        => augend.Add(addend);

    public static IMoneyExpression<TUnderlyingType> operator +(Money<TUnderlyingType> money)
        => money;

    public static IMoneyExpression<TUnderlyingType> operator -(Money<TUnderlyingType> minuend, IMoneyExpression<TUnderlyingType> subtrahend)
        => minuend.Subtract(subtrahend);

    public static Money<TUnderlyingType> operator -(Money<TUnderlyingType> money)
        => money with { Amount = -money.Amount };

    public static IMoneyExpression<TUnderlyingType> operator *(Money<TUnderlyingType> multiplicand, TUnderlyingType multiplier)
        => multiplicand.Multiply(multiplier);

    public static IMoneyExpression<TUnderlyingType> operator *(TUnderlyingType multiplier, Money<TUnderlyingType> multiplicand)
        => multiplicand.Multiply(multiplier);

    public static IMoneyExpression<TUnderlyingType> operator /(Money<TUnderlyingType> dividend, TUnderlyingType divisor)
        => dividend.Divide(divisor);

    public static TUnderlyingType operator /(Money<TUnderlyingType> dividend, IMoneyExpression<TUnderlyingType> divisor)
        => dividend.Divide(divisor);

    private static Currency SelectCurrency(Option<Currency> currency)
        => currency.GetOrElse(CurrencyCulture.CurrentCurrency);

    public static Option<Money<TUnderlyingType>> ParseOrNone(string money, Option<Currency> currency = default)
        => CurrencyCulture
            .FormatProviderFromCurrency(SelectCurrency(currency))
            .Match(
                none: ParseManually(money),
                some: ParseWithFormatProvider(money))
            .AndThen(amount => new Money<TUnderlyingType>(amount, SelectCurrency(currency)));

    private static Func<Option<TUnderlyingType>> ParseManually(string money)
        => ()
           => TUnderlyingType.TryParse(RemoveIsoCurrency(money), CultureInfo.CurrentCulture, out var result)
                ? result
                : Option<TUnderlyingType>.None;

    private static string RemoveIsoCurrency(string money)
        => money.Split(' ') is [var first, { Length: 3 }]
            ? first
            : money;

    private static Func<IFormatProvider, Option<TUnderlyingType>> ParseWithFormatProvider(string money)
        => formatProvider
            => TUnderlyingType.TryParse(RemoveIsoCurrency(money), NumberStyles.Currency, formatProvider, out var result)
                ? result
                : Option<TUnderlyingType>.None;

    public override string ToString()
        => CurrencyCulture.FormatProviderFromCurrency(Currency).Match(
            none: () => string.Format($"{{0:N{Currency.MinorUnitDigits}}} {{1}}", Amount, Currency.AlphabeticCurrencyCode),
            some: formatProvider => string.Format(formatProvider, $"{{0:C{Currency.MinorUnitDigits}}}", Amount));

    TState IMoneyExpression<TUnderlyingType>.Accept<TState>(IMoneyExpressionVisitor<TUnderlyingType, TState> visitor)
        => visitor.Visit(this);
}
