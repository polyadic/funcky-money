namespace Funcky;

/// <summary>Leaves every value untouched.</summary>
public sealed record NoRounding : IRoundingStrategy
{
    public decimal Round(decimal value)
        => value;
}
