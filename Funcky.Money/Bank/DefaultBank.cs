using System;
using System.Collections.Immutable;
using Funcky.Extensions;

namespace Funcky
{
    internal class DefaultBank : IBank
    {
        internal static readonly DefaultBank Empty = new();

        public DefaultBank(ImmutableDictionary<(Currency Source, Currency Target), decimal> exchangeRates)
        {
            ExchangeRates = exchangeRates;
        }

        private DefaultBank()
        {
            ExchangeRates = ImmutableDictionary<(Currency Source, Currency Target), decimal>.Empty;
        }

        public ImmutableDictionary<(Currency Source, Currency Target), decimal> ExchangeRates { get; }

        public decimal ExchangeRate(Currency source, Currency target)
            => ExchangeRates
                .TryGetValue(key: (source, target))
                .GetOrElse(() => throw new Exception($"No exchange rate for {source} => {target}"));

        internal DefaultBank AddExchangeRate(Currency source, Currency target, decimal sellRate)
            => new DefaultBank(ExchangeRates.Add((source, target), sellRate));
    }
}
