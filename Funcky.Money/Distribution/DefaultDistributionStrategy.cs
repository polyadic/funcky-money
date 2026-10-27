namespace Funcky;

/// <summary>
/// Distributes an amount in whole units of a precision: every part gets its proportional share truncated towards
/// zero, the remaining units are handed out one by one to the first parts.
/// </summary>
internal static class DefaultDistributionStrategy
{
    public static decimal Part(decimal total, IReadOnlyList<int> factors, int index, decimal precision)
        => Parts(total, factors, precision).ElementAt(index);

    public static IEnumerable<decimal> Parts(decimal total, IReadOnlyList<int> factors, decimal precision)
    {
        var totalUnits = total / precision;
        var totalFactor = factors.Sum();
        var truncatedUnits = factors.Select(factor => decimal.Truncate(totalUnits * factor / totalFactor)).ToList();
        var remainingUnits = totalUnits - truncatedUnits.Sum();

        return truncatedUnits
            .Select((units, index) => index < Math.Abs(remainingUnits) ? units + Math.Sign(remainingUnits) : units)
            .Select(units => units * precision);
    }
}
