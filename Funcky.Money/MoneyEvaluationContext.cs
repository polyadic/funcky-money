using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Funcky.Monads;

namespace Funcky
{
    public class MoneyEvaluationContext
    {
        private MoneyEvaluationContext(Currency targetCurrency, Option<decimal> precision, IDictionary<Currency, decimal> exchangeRates)
        {
            TargetCurrency = targetCurrency;
            ExchangeRates = exchangeRates;
            Precision = precision.GetOrElse(() => Power.OfATenth(targetCurrency.MinorUnitDigits));
        }

        public Currency TargetCurrency { get; }

        public IDictionary<Currency, decimal> ExchangeRates { get; }

        public decimal Precision { get; }

        public class Builder
        {
            public static readonly Builder Default = new();

            private readonly Option<Currency> _targetCurrency;
            private readonly ImmutableDictionary<Currency, decimal> _exchangeRates;
            private readonly Option<decimal> _precision;

            private Builder()
            {
                _targetCurrency = default;
                _exchangeRates = ImmutableDictionary<Currency, decimal>.Empty;
            }

            private Builder(Option<Currency> currency, Option<decimal> precision, ImmutableDictionary<Currency, decimal> exchangeRates)
            {
                _targetCurrency = currency;
                _precision = precision;
                _exchangeRates = exchangeRates;
            }

            public MoneyEvaluationContext Build()
                => new(
                    _targetCurrency.GetOrElse(() => throw new NotImplementedException()),
                    _precision,
                    _exchangeRates);

            public Builder WithTargetCurrency(Currency currency)
                => new(currency, _precision, _exchangeRates);

            public Builder WithExchangeRate(Currency currency, decimal sellRate)
                => new(_targetCurrency, _precision, _exchangeRates.Add(currency, sellRate));

            public Builder WithPrecisionRate(decimal precision)
                => new(_targetCurrency, precision, _exchangeRates);
        }
    }
}
