using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

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

        internal static Option<IFormatProvider> FormatProviderFromCurrency(Currency currency)
        {
            return AllCultures()
                .Select(c => (CultureInfo: c, RegionInfo: new RegionInfo(c.Name)))
                .FirstOrNone(cr => cr.RegionInfo.ISOCurrencySymbol == currency.AlphabeticCurrencyCode)
                .AndThen(ci => (IFormatProvider)ci.CultureInfo);
        }

        private static IEnumerable<CultureInfo> AllCultures()
            => CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(x => !x.Equals(CultureInfo.InvariantCulture))
                .Where(x => !x.IsNeutralCulture);
    }
}
