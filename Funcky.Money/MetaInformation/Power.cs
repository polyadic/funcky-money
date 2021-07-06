using System.Linq;

namespace Funcky
{
    internal static class Power
    {
        private const decimal Base = 10m;
        private const decimal InvertedBase = 0.1m;
        private const decimal NeutralElement = 1m;

        public static decimal OfTen(int exponent)
            => Enumerable
                .Repeat(Base, exponent)
                .Aggregate(NeutralElement, (p, b) => b * p);

        public static decimal OfATenth(int exponent)
            => Enumerable
                .Repeat(InvertedBase, exponent)
                .Aggregate(NeutralElement, (p, b) => b * p);
    }
}
