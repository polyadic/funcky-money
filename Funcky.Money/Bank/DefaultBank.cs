using System.Collections.Immutable;
using System.Numerics;
using Funcky.Extensions;

namespace Funcky;

internal sealed class DefaultBank<TUnderlyingType> : IBank<TUnderlyingType>
    where TUnderlyingType : INumberBase<TUnderlyingType>
{
    internal static readonly DefaultBank<TUnderlyingType> Empty = new();

    public DefaultBank(ImmutableDictionary<(Currency Source, Currency Target), TUnderlyingType> exchangeRates)
    {
        ExchangeRates = exchangeRates;
    }

    private DefaultBank()
    {
        ExchangeRates = ImmutableDictionary<(Currency Source, Currency Target), TUnderlyingType>.Empty;
    }

    public ImmutableDictionary<(Currency Source, Currency Target), TUnderlyingType> ExchangeRates { get; }

    public TUnderlyingType ExchangeRate(Currency source, Currency target)
        => ExchangeRates
            .GetValueOrNone(key: (source, target))
            .GetOrElse(() => throw new MissingExchangeRateException($"No exchange rate for {source.AlphabeticCurrencyCode} => {target.AlphabeticCurrencyCode}."));

    internal DefaultBank<TUnderlyingType> AddExchangeRate(Currency source, Currency target, TUnderlyingType sellRate)
        => new(ExchangeRates.Add((source, target), sellRate));
}
