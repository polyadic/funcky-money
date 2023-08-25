using System.Diagnostics;
using System.Numerics;

namespace Funcky;

[DebuggerDisplay("{ToString()}")]
internal sealed record RoundWithAwayFromZero<TUnderlyingType> : IRoundingStrategy<TUnderlyingType>
    where TUnderlyingType : INumberBase<TUnderlyingType>, IFloatingPoint<TUnderlyingType>
{
    public RoundWithAwayFromZero(TUnderlyingType precision)
    {
        if (precision <= TUnderlyingType.Zero)
        {
            throw new InvalidPrecisionException();
        }

        Precision = precision;
    }

    public RoundWithAwayFromZero(Currency currency)
    {
        Precision = Power<TUnderlyingType>.OfATenth(currency.MinorUnitDigits);
    }

    public TUnderlyingType Precision { get; }

    public TUnderlyingType Round(TUnderlyingType value)
        => TUnderlyingType.Round(value / Precision, MidpointRounding.AwayFromZero) * Precision;

    public override string ToString()
        => $"Round {{ MidpointRounding: AwayFromZero, Precision: {Precision} }}";

    public bool Equals(IRoundingStrategy<TUnderlyingType>? roundingStrategy)
        => roundingStrategy is RoundWithAwayFromZero<TUnderlyingType> roundWithAwayFromZero
           && Equals(roundWithAwayFromZero);
}
