using FsCheck;

namespace Funcky.Test
{
    internal class CurrencyGenerator
    {
        public static Arbitrary<Currency> GenerateCurrency()
            => Arb.From(Gen.Elements<Currency>(Currency.AllCurrencies));
    }
}
