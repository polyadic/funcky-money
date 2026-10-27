namespace Funcky;

public sealed class InvalidPrecisionException(decimal precision)
    : Exception($"The precision must be positive, but was {precision}.")
{
    public decimal Precision { get; } = precision;
}
