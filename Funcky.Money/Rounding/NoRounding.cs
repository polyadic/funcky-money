using System.Diagnostics;

namespace Funcky
{
    [DebuggerDisplay("NoRounding")]
    internal sealed record NoRounding : AbstractRoundingStrategy
    {
        public NoRounding(in decimal precision)
            : base(precision)
        {
        }

        public NoRounding(Currency currency)
            : base(currency)
        {
        }

        public override decimal Round(decimal value)
            => value;
    }
}
