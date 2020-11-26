using System.Globalization;

namespace Funcky
{
    internal static class CurrencyCulture
    {
        public static Currency CurrentCurrency()
            => new Currency(CurrentRegion().ISOCurrencySymbol);

        public static RegionInfo CurrentRegion()
            => new RegionInfo(CultureInfo.CurrentCulture.LCID);
    }
}
