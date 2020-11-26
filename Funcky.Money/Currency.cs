using System;

namespace Funcky
{
    public record Currency
    {
        private Iso4217Record _currencyInformation;
        private static readonly Lazy<Currency> _chf = new Lazy<Currency>(() => new(nameof(CHF)));
        private static readonly Lazy<Currency> _eur = new Lazy<Currency>(() => new(nameof(EUR)));
        private static readonly Lazy<Currency> _usd = new Lazy<Currency>(() => new(nameof(USD)));

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
            => _chf.Value;

        public static Currency EUR()
            => _eur.Value;

        public static Currency USD()
            => _usd.Value;
    }
}
