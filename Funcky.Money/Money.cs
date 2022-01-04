using System.Globalization;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky;

public sealed partial record Money
{
    public static MoneyExpression.Money Zero
        => MoneyExpression.Money.Zero;

    public static MoneyExpression.Money Create(decimal amount, Option<Currency> currency = default)
        => new(amount, currency);

    public static MoneyExpression.Money Create(decimal amount, MoneyEvaluationContext context)
        => new(amount, context);

    public static MoneyExpression.Money Create(int amount, Option<Currency> currency = default)
        => new(amount, currency);

    public static Option<MoneyExpression.Money> ParseOrNone(string money, Option<Currency> currency = default)
        => CurrencyCulture
            .FormatProviderFromCurrency(SelectCurrency(currency))
            .Match(
                none: ParseManually(money),
                some: ParseWithFormatProvider(money))
            .AndThen(amount => new MoneyExpression.Money(amount, SelectCurrency(currency)));

    private static Func<Option<decimal>> ParseManually(string money)
        => ()
            => RemoveIsoCurrency(money).ParseDecimalOrNone();

    private static string RemoveIsoCurrency(string money)
    {
        var parts = money.Split(' ');
        return parts.Length == 2 && parts[1].Length == 3
            ? parts[0]
            : money;
    }

    private static Func<IFormatProvider, Option<decimal>> ParseWithFormatProvider(string money)
        => formatProvider
            => money.ParseDecimalOrNone(NumberStyles.Currency, formatProvider);

    private static Currency SelectCurrency(Option<Currency> currency)
        => currency.GetOrElse(CurrencyCulture.CurrentCurrency);
}
