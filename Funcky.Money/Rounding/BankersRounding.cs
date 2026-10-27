namespace Funcky;

/// <summary>Rounds to the nearest multiple of <paramref name="Precision"/>, midpoints go to the even multiple.</summary>
public sealed record BankersRounding(decimal Precision) : IRoundingStrategy
{
    public decimal Round(decimal value)
        => Math.Round(value / Precision, MidpointRounding.ToEven) * Precision;
}
