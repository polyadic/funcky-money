namespace Funcky;

/// <summary>Rounds to the nearest multiple of <paramref name="Precision"/>, midpoints go to the even multiple.</summary>
/// <exception cref="InvalidPrecisionException">The precision is not positive.</exception>
public sealed record BankersRounding(decimal Precision) : IRoundingStrategy
{
    public decimal Precision { get; } = Funcky.Precision.Validate(Precision);

    public decimal Round(decimal value)
        => Math.Round(value / Precision, MidpointRounding.ToEven) * Precision;
}
