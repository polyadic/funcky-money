using System;
using System.Diagnostics;

namespace Funcky
{
    [DebuggerDisplay("{ToString()}")]
    public sealed record BankersRounding : IRoundingStrategy
    {
        private readonly decimal _precision;

        public BankersRounding(decimal precision)
        {
            Debug.Assert(precision > 0m, "precision must be positive and cannot be zero");

            _precision = precision;
        }

        public BankersRounding(Currency currency)
        {
            _precision = Power.OfATenth(currency.MinorUnitDigits);
        }

        public bool Equals(IRoundingStrategy? roundingStrategy)
            => roundingStrategy is BankersRounding bankersRounding
               && Equals(bankersRounding);

        public decimal Round(decimal value)
            => Math.Round(value / _precision, MidpointRounding.ToEven) * _precision;

        public override string ToString()
            => $"BankersRounding {{ Precision: {_precision} }}";
    }
}
