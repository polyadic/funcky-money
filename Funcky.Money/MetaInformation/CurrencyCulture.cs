using System.Globalization;

namespace Funcky;

/// <summary>
/// Maps currencies to cultures: a currency is formatted by the first specific culture (by name) whose region uses it.
/// Currencies without such a culture (e.g. XAU, gold) have no format provider.
/// </summary>
internal static class CurrencyCulture
{
    private static readonly Lazy<IReadOnlyDictionary<string, CultureInfo>> CultureByCurrencyCode = new(CollectCulturesByCurrencyCode);

    public static Option<CultureInfo> FormatProvider(Currency currency)
        => CultureByCurrencyCode.Value.TryGetValue(currency.AlphabeticCode, out var culture)
            ? culture
            : Option<CultureInfo>.None;

    public static NumberFormatInfo NumberFormat(Currency currency, CultureInfo culture)
    {
        var numberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
        numberFormat.CurrencyDecimalDigits = currency.MinorUnitDigits;

        return numberFormat;
    }

    private static IReadOnlyDictionary<string, CultureInfo> CollectCulturesByCurrencyCode()
        => CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Where(culture => culture.Name.Length > 0)
            .OrderBy(culture => culture.Name, StringComparer.Ordinal)
            .GroupBy(culture => new RegionInfo(culture.Name).ISOCurrencySymbol)
            .ToDictionary(group => group.Key, group => group.First());
}
