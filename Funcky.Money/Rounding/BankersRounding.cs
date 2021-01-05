using System;
using System.Diagnostics;

namespace Funcky
{
    [DebuggerDisplay("{ToString()}")]
    internal sealed record BankersRounding : IRoundingStrategy
    {
        private readonly decimal _precision;

        public BankersRounding(decimal precision)
        {
            if (precision <= 0m)
            {
                throw new InvalidPrecisionException();
            }

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
