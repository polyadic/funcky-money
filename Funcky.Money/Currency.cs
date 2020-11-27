using System;

namespace Funcky
{
    public record Currency
    {
        private readonly Iso4217Record _currencyInformation;
        private static readonly Lazy<Currency> Chf = new(() => new(nameof(CHF)));
        private static readonly Lazy<Currency> Eur = new(() => new(nameof(EUR)));
        private static readonly Lazy<Currency> Usd = new(() => new(nameof(USD)));

        public Currency(string currency)
        {
            _currencyInformation = CurrencyInformationIso4217.Instance[currency];
        }

        public string CurrencyName
            => _currencyInformation.CurrencyName;

        public string AlphabeticCurrencyCode
            => _currencyInformation.AlphabeticCurrencyCode;

        public int NumericCurrencyCode
            => _currencyInformation.NumericCurrencyCode;

        public int MinorUnitDigits
            => _currencyInformation.MinorUnitDigits;

        public static Currency CHF()
            => Chf.Value;

        public static Currency EUR()
            => Eur.Value;

        public static Currency USD()
            => Usd.Value;
    }
}
