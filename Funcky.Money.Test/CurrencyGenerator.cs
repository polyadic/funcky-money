using System;
using FsCheck;

namespace Funcky.Test
{
    internal class CurrencyGenerator
    {
        public static Arbitrary<Currency> MyCurrency()
        {
            return Arb.From(
                from int id in Gen.Choose(0, 11)
                select id switch
                {
                    0 => Currency.CHF,
                    1 => Currency.USD,
                    2 => Currency.EUR,
                    3 => Currency.JPY,
                    4 => Currency.GBP,
                    5 => Currency.OMR,
                    6 => Currency.VUV,
                    7 => Currency.NOK,
                    8 => Currency.CHE,
                    9 => Currency.CHW,
                    10 => Currency.XXX,
                    11 => Currency.XAU,
                    _ => throw new NotImplementedException("invalid id"),
                });
        }
    }
}
