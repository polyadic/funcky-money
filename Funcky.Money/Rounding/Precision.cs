namespace Funcky;

internal static class Precision
{
    /// <exception cref="InvalidPrecisionException">The precision is not positive.</exception>
    public static decimal Validate(decimal precision)
        => precision > 0
            ? precision
            : throw new InvalidPrecisionException(precision);
}
