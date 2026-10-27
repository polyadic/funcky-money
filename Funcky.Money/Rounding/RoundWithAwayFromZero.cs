namespace Funcky;

/// <summary>Rounds to the nearest multiple of <paramref name="Precision"/>, midpoints go away from zero.</summary>
public sealed record RoundWithAwayFromZero(decimal Precision) : IRoundingStrategy
{
    public decimal Round(decimal value)
        => Math.Round(value / Precision, MidpointRounding.AwayFromZero) * Precision;
}
