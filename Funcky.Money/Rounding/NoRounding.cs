using System.Diagnostics;

namespace Funcky;

[DebuggerDisplay("NoRounding")]
internal sealed record NoRounding : IRoundingStrategy
{
    public decimal Round(decimal value)
        => value;

    public bool Equals(IRoundingStrategy? roundingStrategy)
        => roundingStrategy is NoRounding noRounding
               && Equals(noRounding);
}
