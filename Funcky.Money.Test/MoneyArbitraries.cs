using FsCheck;

namespace Funcky.Test
{
    internal class MoneyArbitraries
    {
        public static Arbitrary<Currency> ArbitraryCurrency()
            => Arb.From(Gen.Elements<Currency>(Currency.AllCurrencies));

        public static Arbitrary<Money> ArbitraryMoney()
            => GenerateMoney().ToArbitrary();

        public static Arbitrary<SwissMoney> ArbitrarySwissFrancs()
            => GenerateSwissFranc().ToArbitrary();

        private static Gen<Money> GenerateMoney()
            => from currency in Arb.Generate<Currency>()
               from amount in Arb.Generate<PositiveInt>()
               select new Money(Power.OfATenth(currency.MinorUnitDigits) * amount.Get, currency);

        private static Gen<SwissMoney> GenerateSwissFranc()
            => from amount in Arb.Generate<PositiveInt>()
               select new SwissMoney(Money.CHF(0.05m * amount.Get));
    }
}
