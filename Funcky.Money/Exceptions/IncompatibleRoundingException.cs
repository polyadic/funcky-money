namespace Funcky;

public sealed class IncompatibleRoundingException(IRoundingStrategy roundingStrategy, decimal smallestDistributionUnit)
    : Exception($"The rounding strategy {roundingStrategy} changes amounts distributed in units of {smallestDistributionUnit}.")
{
    public IRoundingStrategy RoundingStrategy { get; } = roundingStrategy;

    public decimal SmallestDistributionUnit { get; } = smallestDistributionUnit;
}
