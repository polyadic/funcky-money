using System;
using System.Diagnostics;

namespace Funcky
{
    [DebuggerDisplay("{ToString()}")]
    internal record RoundWithAwayFromZero : AbstractRoundingStrategy
    {
        public RoundWithAwayFromZero(in decimal precision)
            : base(precision)
        {
        }

        public RoundWithAwayFromZero(Currency currency)
            : base(currency)
        {
        }

        public override decimal Round(decimal value)
            => Math.Round(value / Precision, MidpointRounding.AwayFromZero) * Precision;

        public override string ToString()
            => $"Round {{ MidpointRounding: AwayFromZero, Precision: {Precision} }}";
    }
}
