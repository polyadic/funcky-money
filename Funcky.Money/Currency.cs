namespace Funcky;

public sealed record Currency(string AlphabeticCode, int MinorUnitDigits)
{
    public static Currency CHF { get; } = new("CHF", 2);

    public static Currency USD { get; } = new("USD", 2);
}
