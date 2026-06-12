using System.Collections.Immutable;
using Funcky.Extensions;

namespace Funcky;

internal sealed class DefaultBank(ImmutableDictionary<(Currency Source, Currency Target), decimal> exchangeRates)
    : IBank
{
    internal static readonly DefaultBank Empty = new();

    private DefaultBank()
        : this(ImmutableDictionary<(Currency Source, Currency Target), decimal>.Empty)
    {
    }

    public ImmutableDictionary<(Currency Source, Currency Target), decimal> ExchangeRates { get; } = exchangeRates;

    public decimal ExchangeRate(Currency source, Currency target)
        => ExchangeRates
            .GetValueOrNone(key: (source, target))
            .GetOrElse(() => throw new MissingExchangeRateException($"No exchange rate for {source.AlphabeticCurrencyCode} => {target.AlphabeticCurrencyCode}."));

    internal DefaultBank AddExchangeRate(Currency source, Currency target, decimal sellRate)
        => new(ExchangeRates.Add((source, target), sellRate));
}
