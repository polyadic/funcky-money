using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Funcky
{
    internal static class CurrencyCulture
    {
        public static Currency CurrentCurrency()
            => new Currency(CurrentRegion().ISOCurrencySymbol);

        public static RegionInfo CurrentRegion()
            => new RegionInfo(CultureInfo.CurrentCulture.LCID);

        internal static IFormatProvider CultureInfoFromCurrency(Currency currency)
        {
            return AllCultures()
                .Select(c => (CultureInfo: c, RegionInfo: new RegionInfo(c.LCID)))
                .First(cr => cr.RegionInfo.ISOCurrencySymbol == currency.AlphabeticCurrencyCode)
                .CultureInfo;
        }

        private static IEnumerable<CultureInfo> AllCultures()
            => CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(x => !x.Equals(CultureInfo.InvariantCulture))
                .Where(x => !x.IsNeutralCulture);
    }
}
