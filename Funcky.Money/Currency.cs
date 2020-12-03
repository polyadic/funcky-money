using System;
using System.Diagnostics;

namespace Funcky
{
    [DebuggerDisplay("{CurrencyName,nq} ({AlphabeticCurrencyCode,nq})")]
    public record Currency
    {
        private readonly Iso4217Record _currencyInformation;
        private static readonly Lazy<Currency> Chf = new(() => new(nameof(CHF)));
        private static readonly Lazy<Currency> Eur = new(() => new(nameof(EUR)));
        private static readonly Lazy<Currency> Usd = new(() => new(nameof(USD)));

        public Currency(string currency)
        {
            _currencyInformation = Iso4217Information.Currencies[currency];
        }

        public string CurrencyName
            => _currencyInformation.CurrencyName;

        public string AlphabeticCurrencyCode
            => _currencyInformation.AlphabeticCurrencyCode;

        public int NumericCurrencyCode
            => _currencyInformation.NumericCurrencyCode;

        public int MinorUnitDigits
            => _currencyInformation.MinorUnitDigits;

        // ReSharper disable InconsistentNaming - Reason: we want the currencies in capital letters
        public static Currency CHF()
            => Chf.Value;

        public static Currency EUR()
            => Eur.Value;

        public static Currency USD()
            => Usd.Value;
    }
}
