namespace Funcky;

public sealed class MissingExchangeRateException(Currency source, Currency target)
    : Exception($"No exchange rate from {source.AlphabeticCode} to {target.AlphabeticCode} is known.")
{
    public Currency SourceCurrency { get; } = source;

    public Currency TargetCurrency { get; } = target;
}
