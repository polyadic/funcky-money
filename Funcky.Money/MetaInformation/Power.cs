using System;
using System.Linq;

namespace Funcky
{
    internal class Power
    {
        public static decimal OfTen(int exponent)
            => Enumerable.Repeat(exponent > 0 ? 10m : 0.1m, Math.Abs(exponent)).Aggregate(1m, (p, b) => b * p);
    }
}
