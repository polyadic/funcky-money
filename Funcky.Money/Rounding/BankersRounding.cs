using System.Diagnostics;
using System.Numerics;

namespace Funcky;

[DebuggerDisplay("{ToString()}")]
internal sealed record BankersRounding<TUnderlyingType> : IRoundingStrategy<TUnderlyingType>
    where TUnderlyingType : INumberBase<TUnderlyingType>, IFloatingPoint<TUnderlyingType>
{
    private readonly TUnderlyingType _precision;

    public BankersRounding(TUnderlyingType precision)
    {
        if (precision <= TUnderlyingType.Zero)
        {
            throw new InvalidPrecisionException();
        }

        _precision = precision;
    }

    public BankersRounding(Currency currency)
    {
        _precision = Power<TUnderlyingType>.OfATenth(currency.MinorUnitDigits);
    }

    public bool Equals(IRoundingStrategy<TUnderlyingType>? roundingStrategy)
        => roundingStrategy is BankersRounding<TUnderlyingType> bankersRounding
           && Equals(bankersRounding);

    public TUnderlyingType Round(TUnderlyingType value)
        => TUnderlyingType.Round(value / _precision, MidpointRounding.ToEven) * _precision;

    public override string ToString()
        => $"BankersRounding {{ Precision: {_precision} }}";
}
