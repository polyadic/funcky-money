namespace Funcky;

internal sealed record Iso4217Record(
    string CurrencyName,
    string AlphabeticCurrencyCode,
    int NumericCurrencyCode,
    int MinorUnitDigits);
