using System.Diagnostics;

namespace Funcky
{
    public abstract record AbstractRoundingStrategy
    {
        private readonly decimal _precision;

        public AbstractRoundingStrategy(in decimal precision)
        {
            Debug.Assert(precision > 0m, "precision must be positive and cannot be zero");

            _precision = precision;
        }

        public AbstractRoundingStrategy(Currency currency)
        {
            _precision = Power.OfATenth(currency.MinorUnitDigits);
        }

        public decimal Precision
            => _precision;

        public abstract decimal Round(decimal value);
    }
}
