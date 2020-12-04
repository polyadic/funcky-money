using System;
using System.Diagnostics;

namespace Funcky
{
    [DebuggerDisplay("{ToString()}")]
    public sealed record BankersRounding : AbstractRoundingStrategy
    {
        public BankersRounding(in decimal precision)
            : base(precision)
        {
        }

        public BankersRounding(Currency currency)
            : base(currency)
        {
        }

        public override decimal Round(decimal value)
            => Math.Round(value / Precision, MidpointRounding.ToEven) * Precision;

        public override string ToString()
            => $"BankesRounding {{ Precision: {Precision} }}";
    }
}
