using System;
using System.Globalization;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    public record Money : IMoneyExpression
    {
        public static readonly Money Zero = new(0m);

        public Money(decimal amount, Option<Currency> currency = default)
        {
            Currency = SelectCurrency(currency);
            Amount = amount;
            Precision = Power.OfATenth(Currency.MinorUnitDigits);
        }

        public Money(int amount, Option<Currency> currency = default)
            : this((decimal)amount, currency)
        {
        }

        public Money(double amount, Option<Currency> currency = default)
            : this(Math.Round((decimal)amount, SelectCurrency(currency).MinorUnitDigits, MidpointRounding.ToEven), currency)
        {
        }

        public decimal Amount { get; init; }

        public Currency Currency { get; init; }

        public decimal Precision { get; set; }

        public MidpointRounding MidpointRounding { get; set; } = MidpointRounding.ToEven;

        public static Option<Money> ParseOrNone(string money, Option<Currency> currency = default)
        {
            var selectedCurrency = SelectCurrency(currency);

            return money
                .TryParseDecimal(NumberStyles.Currency, CurrencyCulture.CultureInfoFromCurrency(selectedCurrency))
                .AndThen(m => new Money(m, selectedCurrency));
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
            => currency.GetOrElse(CurrencyCulture.CurrentCurrency);

        public static Money CHF(decimal amount)
            => new(amount, Currency.CHF()) { Precision = 0.05m, MidpointRounding = MidpointRounding.ToEven };
    }
}
