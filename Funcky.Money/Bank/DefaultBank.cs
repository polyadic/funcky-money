namespace Funcky;

/// <summary>
/// A bank which only knows the explicitly configured exchange rates towards a single target currency.
/// </summary>
internal sealed class DefaultBank(Currency targetCurrency, IReadOnlyDictionary<Currency, decimal> exchangeRates) : IBank
{
    public decimal ExchangeRate(Currency source, Currency target)
        => source == target
            ? 1m
            : target == targetCurrency && exchangeRates.TryGetValue(source, out var rate)
                ? rate
                : throw new MissingExchangeRateException(source, target);
}
