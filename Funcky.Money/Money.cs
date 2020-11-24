using System.Globalization;
using Funcky.Monads;

namespace Funcky
{
    public record Money : IMoneyExpression
    {
        public Money(decimal amount, Option<Currency> currency = default)
        {
            Amount = amount;
            Currency = currency.GetOrElse(() => FromCurrentCulture());
        }

        private static Currency FromCurrentCulture()
            => new Currency(RegionFromCurrentCulture().ISOCurrencySymbol);

        private static RegionInfo RegionFromCurrentCulture()
            => new RegionInfo(CultureInfo.CurrentCulture.LCID);

        public decimal Amount { get; }
        public Currency Currency { get; }
    }
}
