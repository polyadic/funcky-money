using System.Linq;

namespace Funcky
{
    internal static class Power
    {
        public static decimal OfTen(int exponent)
            => Enumerable.Repeat(10m, exponent).Aggregate(1m, (p, b) => b * p);

        public static decimal OfATenth(int exponent)
            => Enumerable.Repeat(0.1m, exponent).Aggregate(1m, (p, b) => b * p);
    }
}
