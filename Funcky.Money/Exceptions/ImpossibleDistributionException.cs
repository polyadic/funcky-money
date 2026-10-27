namespace Funcky;

public sealed class ImpossibleDistributionException(decimal amount, decimal precision)
    : Exception($"The amount {amount} cannot be distributed exactly in units of {precision}.")
{
    public decimal Amount { get; } = amount;

    public decimal Precision { get; } = precision;
}
