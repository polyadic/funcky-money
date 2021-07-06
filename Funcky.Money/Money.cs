using System;
using System.Diagnostics;
using System.Globalization;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    [DebuggerDisplay("{Amount} {Currency.AlphabeticCurrencyCode,nq}")]
    public sealed partial record Money : IMoneyExpression
    {
        public static readonly Money Zero = new(0m);

        public Money(decimal amount, Option<Currency> currency = default)
            => (Amount, Currency, RoundingStrategy)
                = (amount, SelectCurrency(currency), Funcky.RoundingStrategy.Default(Currency));

        public Money(decimal amount, MoneyEvaluationContext context)
            => (Amount, Currency, RoundingStrategy)
                = (amount, context.TargetCurrency, context.RoundingStrategy);

        public Money(int amount, Option<Currency> currency = default)
            : this((decimal)amount, currency)
        {
        }

        public decimal Amount { get; init; }

        public Currency Currency { get; init; }

        public IRoundingStrategy RoundingStrategy { get; }

        public bool IsZero
            => Amount == 0m;

        // These operators supports the operators on IMoneyExpression, because Money + Money or Money * factor does not work otherwise without a cast.
        public static IMoneyExpression operator +(Money augend, IMoneyExpression addend)
            => augend.Add(addend);

        public static IMoneyExpression operator +(Money money)
            => money;

        public static IMoneyExpression operator -(Money minuend, IMoneyExpression subtrahend)
            => minuend.Subtract(subtrahend);

        public static Money operator -(Money money)
            => money with { Amount = -money.Amount };

        public static IMoneyExpression operator *(Money multiplicand, decimal multiplier)
            => multiplicand.Multiply(multiplier);

        public static IMoneyExpression operator *(decimal multiplier, Money multiplicand)
            => multiplicand.Multiply(multiplier);

        public static IMoneyExpression operator /(Money dividend, decimal divisor)
            => dividend.Divide(divisor);

        private static Currency SelectCurrency(Option<Currency> currency)
            => currency.GetOrElse(CurrencyCulture.CurrentCurrency);

        public static Option<Money> ParseOrNone(string money, Option<Currency> currency = default)
            => CurrencyCulture
                .FormatProviderFromCurrency(SelectCurrency(currency))
                .Match(
                    none: ParseManually(money),
                    some: ParseWithFormatProvider(money))
                .AndThen(amount => new Money(amount, SelectCurrency(currency)));

        private static Func<Option<decimal>> ParseManually(string money)
            => ()
               => RemoveIsoCurrency(money).ParseDecimalOrNone();

        private static string RemoveIsoCurrency(string money)
            => RemoveIsoCurrencyPart(money.Split(' '))
                .GetOrElse(money);

        private static Option<string> RemoveIsoCurrencyPart(string[] parts)
            => parts.Length == 2 && parts[1].Length == 3
                ? parts[0]
                : Option<string>.None();

        private static Func<IFormatProvider, Option<decimal>> ParseWithFormatProvider(string money)
            => formatProvider
                => money.ParseDecimalOrNone(NumberStyles.Currency, formatProvider);

        public override string ToString()
            => CurrencyCulture.FormatProviderFromCurrency(Currency).Match(
                none: () => string.Format($"{{0:N{Currency.MinorUnitDigits}}} {{1}}", Amount, Currency.AlphabeticCurrencyCode),
                some: formatProvider => string.Format(formatProvider, $"{{0:C{Currency.MinorUnitDigits}}}", Amount));

        TState IMoneyExpression.Accept<TState>(IMoneyExpressionVisitor<TState> visitor)
            => visitor.Visit(this);
    }
}
