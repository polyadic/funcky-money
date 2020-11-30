using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Funcky.Monads;

namespace Funcky
{
    public class MoneyEvaluationContext
    {
        private MoneyEvaluationContext(Currency targetCurrency, Option<decimal> precision, Option<MidpointRounding> midpointRounding, IDictionary<Currency, decimal> exchangeRates)
        {
            TargetCurrency = targetCurrency;
            Precision = precision.GetOrElse(() => Power.OfATenth(targetCurrency.MinorUnitDigits));
            MidpointRounding = midpointRounding.GetOrElse(RoundingStrategy.DefaultRoundingStrategy);
            ExchangeRates = exchangeRates;
        }

        public Currency TargetCurrency { get; }

        public decimal Precision { get; }

        public MidpointRounding MidpointRounding { get; }

        public IDictionary<Currency, decimal> ExchangeRates { get; }

        public class Builder
        {
            public static readonly Builder Default = new();

            private readonly Option<Currency> _targetCurrency;
            private readonly Option<decimal> _precision;
            private readonly Option<MidpointRounding> _midpointRounding;
            private readonly ImmutableDictionary<Currency, decimal> _exchangeRates;

            private Builder()
            {
                _targetCurrency = default;
                _precision = default;
                _midpointRounding = default;
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
                    _targetCurrency.GetOrElse(() => throw new Exception("Money evaluation context has no target currency set.")),
                    _precision,
                    _midpointRounding,
                    _exchangeRates);

            public Builder WithTargetCurrency(Currency currency)
                => new(currency, _precision, _exchangeRates);

            public Builder WithPrecision(decimal precision)
                => new(_targetCurrency, precision, _exchangeRates);

            public Builder WithMidpointRounding(decimal precision)
                => new(_targetCurrency, precision, _exchangeRates);

            public Builder WithExchangeRate(Currency currency, decimal sellRate)
                => new(_targetCurrency, _precision, _exchangeRates.Add(currency, sellRate));
        }
    }
}
