namespace Funcky;

public interface IRoundingStrategy<TUnderlyingType> : IEquatable<IRoundingStrategy<TUnderlyingType>>
{
    TUnderlyingType Round(TUnderlyingType value);
}
