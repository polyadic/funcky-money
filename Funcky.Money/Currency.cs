using System.Diagnostics;
using Funcky.Monads;

namespace Funcky;

[DebuggerDisplay("{CurrencyName,nq} ({AlphabeticCurrencyCode,nq})")]
public partial record Currency
{
    private readonly Iso4217Record _currencyInformation;

    internal Currency(Iso4217Record currencyInformation)
    {
        _currencyInformation = currencyInformation;
    }

    public string CurrencyName
        => _currencyInformation.CurrencyName;

    public string AlphabeticCurrencyCode
        => _currencyInformation.AlphabeticCurrencyCode;

    public int NumericCurrencyCode
        => _currencyInformation.NumericCurrencyCode;

    public int MinorUnitDigits
        => _currencyInformation.MinorUnitDigits;

    public static partial Option<Currency> ParseOrNone(string input);
}
