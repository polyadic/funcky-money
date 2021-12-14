namespace Funcky.Money.SourceGenerator;

internal sealed record Iso4217Record
{
    public Iso4217Record(string currencyName, string alphabeticCurrencyCode, int numericCurrencyCode, int? minorUnitDigits)
    {
        CurrencyName = currencyName;
        AlphabeticCurrencyCode = alphabeticCurrencyCode;
        NumericCurrencyCode = numericCurrencyCode;
        MinorUnitDigits = minorUnitDigits;
    }

    public string CurrencyName { get; }

    public string AlphabeticCurrencyCode { get; }

    public int NumericCurrencyCode { get; }

    public int? MinorUnitDigits { get; }
}
