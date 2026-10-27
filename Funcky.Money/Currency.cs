namespace Funcky;

public sealed record Currency(string AlphabeticCode)
{
    public static Currency CHF { get; } = new("CHF");

    public static Currency USD { get; } = new("USD");
}
