using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Funcky
{
    internal static class CurrencyCulture
    {
        public static Currency CurrentCurrency()
        {
            var currencySymbol = CurrentRegion().ISOCurrencySymbol;

            return Currency
                .ParseOrNone(currencySymbol)
                .GetOrElse(() => throw new NotSupportedException($"The currency '{currencySymbol}' is not supported, is the current Region '{CurrentRegion().Name}' valid?"));
        }

        public static RegionInfo CurrentRegion()
            => new(CultureInfo.CurrentCulture.Name);

        internal static IFormatProvider CultureInfoFromCurrency(Currency currency)
        {
            return AllCultures()
                .Select(c => (CultureInfo: c, RegionInfo: new RegionInfo(c.Name)))
                .First(cr => cr.RegionInfo.ISOCurrencySymbol == currency.AlphabeticCurrencyCode)
                .CultureInfo;
        }

        private static IEnumerable<CultureInfo> AllCultures()
            => CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(x => !x.Equals(CultureInfo.InvariantCulture))
                .Where(x => !x.IsNeutralCulture);
    }
}
