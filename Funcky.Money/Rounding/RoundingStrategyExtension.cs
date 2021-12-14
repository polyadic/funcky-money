namespace Funcky;

internal static class RoundingStrategyExtension
{
    public static bool IsSameAfterRounding(this IRoundingStrategy roundingStrategy, decimal amount)
        => roundingStrategy.Round(amount) == amount;
}
