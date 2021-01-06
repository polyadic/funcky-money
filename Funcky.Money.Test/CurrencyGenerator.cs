using FsCheck;

namespace Funcky.Test
{
    internal class CurrencyGenerator
    {
        public static Arbitrary<Currency> MyCurrency()
        {
            return Arb.From(
                from int id in Gen.Choose(0, Currency.AllCurrencies.Count - 1)
                select Currency.AllCurrencies[id]);
        }
    }
}
