using System;
using System.Globalization;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    public record Money : IMoneyExpression
    {
        public static readonly Money Zero = new Money(0m);

        public Money(decimal amount, Option<Currency> currency = default)
        {
            Currency = SelectCurrency(currency);
            Amount = Math.Round(amount, Currency.MinorUnitDigits, MidpointRounding.ToEven);
        }

        public Money(int amount, Option<Currency> currency = default)
            : this((decimal)amount, currency)
        {
        }

        public Money(double amount, Option<Currency> currency = default)
            : this((decimal)amount, currency)
        {
        }

        public decimal Amount { get; }

        public Currency Currency { get; }

        public decimal Precision { get; set; }

        public static Option<Money> ParseOrNone(string money, Option<Currency> currency = default)
        {
            var selectedCurrency = SelectCurrency(currency);

            return money
                .TryParseDecimal(NumberStyles.Currency, CurrencyCulture.CultureInfoFromCurrency(selectedCurrency))
                .AndThen(m => new Money(m, Option.Some(selectedCurrency)));
        }

        public override string ToString()
            => string.Format(CurrencyCulture.CultureInfoFromCurrency(Currency), "{0:C}", Amount);

        void IMoneyExpression.Accept(IMoneyExpressionVisitor visitor)
            => visitor.Visit(this);

        // These operators supports the operators on IMoneyExpression, because Money + Money or Money * factor does not work otherwise without a cast.
        public static IMoneyExpression operator *(Money moneyExpression, decimal factor)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression operator *(decimal factor, Money moneyExpression)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression operator +(Money leftMoneyExpression, IMoneyExpression rightMoneyExpression)
            => new MoneySum(leftMoneyExpression, rightMoneyExpression);

        private static Currency SelectCurrency(Option<Currency> currency)
            => currency.GetOrElse(() => CurrencyCulture.CurrentCurrency());
    }
}
