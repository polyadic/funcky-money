using System;
using System.Diagnostics;

namespace Funcky
{
    [DebuggerDisplay("{ToString()}")]
    internal sealed record RoundWithAwayFromZero : IRoundingStrategy
    {
        public RoundWithAwayFromZero(decimal precision)
        {
            if (precision <= 0m)
            {
                throw new InvalidPrecisionException();
            }

            Precision = precision;
        }

        public RoundWithAwayFromZero(Currency currency)
        {
            Precision = Power.OfATenth(currency.MinorUnitDigits);
        }

        public decimal Precision { get; }

        public decimal Round(decimal value)
            => Math.Round(value / Precision, MidpointRounding.AwayFromZero) * Precision;

        public override string ToString()
            => $"Round {{ MidpointRounding: AwayFromZero, Precision: {Precision} }}";

        public bool Equals(IRoundingStrategy? roundingStrategy)
            => roundingStrategy is RoundWithAwayFromZero roundWithAwayFromZero
               && Equals(roundWithAwayFromZero);
    }
}
