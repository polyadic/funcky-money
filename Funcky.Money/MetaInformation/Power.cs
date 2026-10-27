namespace Funcky;

public static class Power
{
    /// <summary>
    /// Returns 10^-<paramref name="exponent"/> as an exact decimal, e.g. 0.01 for an exponent of 2.
    /// </summary>
    public static decimal OfATenth(int exponent)
        => new(lo: 1, mid: 0, hi: 0, isNegative: false, scale: (byte)exponent);
}
