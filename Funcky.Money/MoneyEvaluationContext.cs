using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Funcky.Monads;

namespace Funcky
{
    public class MoneyEvaluationContext
    {
        private MoneyEvaluationContext(Currency targetCurrency, IDictionary<Currency, decimal> exchangeRates)
        {
            TargetCurrency = targetCurrency;
            ExchangeRates = exchangeRates;
        }

        public Currency TargetCurrency { get; }

        public IDictionary<Currency, decimal> ExchangeRates { get; }

        public class Builder
        {
            public static readonly Builder Default = new Builder();

            private readonly Option<Currency> _targetCurrency;
            private readonly ImmutableDictionary<Currency, decimal> _exchangeRates;

            private Builder()
            {
                _targetCurrency = default;
                _exchangeRates = ImmutableDictionary<Currency, decimal>.Empty;
            }

            private Builder(Option<Currency> currency, ImmutableDictionary<Currency, decimal> exchangeRates)
            {
                _targetCurrency = currency;
                _exchangeRates = exchangeRates;
            }

            public MoneyEvaluationContext Build()
            {
                return new MoneyEvaluationContext(
                    _targetCurrency.GetOrElse(() => throw new NotImplementedException()),
                    _exchangeRates);
            }

            public Builder WithTargetCurrency(Currency currency)
            {
                return new Builder(Option.Some(currency), _exchangeRates);
            }

            public Builder WithExchangeRate(Currency currency, decimal sellRate)
            {
                return new Builder(_targetCurrency, _exchangeRates.Add(currency, sellRate));
            }
        }
    }
}
