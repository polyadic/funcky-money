namespace Funcky.Money.SourceGenerator;

internal sealed record Iso4217Record(
    string CurrencyName,
    string AlphabeticCurrencyCode,
    int NumericCurrencyCode,
    int? MinorUnitDigits)
{
    public string CurrencyName { get; } = CurrencyName;

    public string AlphabeticCurrencyCode { get; } = AlphabeticCurrencyCode;

    public int NumericCurrencyCode { get; } = NumericCurrencyCode;

    public int? MinorUnitDigits { get; } = MinorUnitDigits;
}
