using System;
using System.Globalization;
using Funcky.Monads;

namespace Funcky
{
    public record Money : IMoneyExpression
    {
        public Money(decimal amount, Option<Currency> currency = default)
        {
            Currency = currency.GetOrElse(() => FromCurrentCulture());
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

        private static Currency FromCurrentCulture()
            => new Currency(RegionFromCurrentCulture().ISOCurrencySymbol);

        private static RegionInfo RegionFromCurrentCulture()
            => new RegionInfo(CultureInfo.CurrentCulture.LCID);

        public decimal Amount { get; }

        public Currency Currency { get; }
    }
}
