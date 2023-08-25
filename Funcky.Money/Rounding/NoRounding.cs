using System.Diagnostics;

namespace Funcky;

[DebuggerDisplay("NoRounding")]
internal sealed record NoRounding<TUnderlyingType> : IRoundingStrategy<TUnderlyingType>
{
    public TUnderlyingType Round(TUnderlyingType value)
        => value;

    public bool Equals(IRoundingStrategy<TUnderlyingType>? roundingStrategy)
        => roundingStrategy is NoRounding<TUnderlyingType> noRounding
               && Equals(noRounding);
}
