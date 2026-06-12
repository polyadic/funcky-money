using FsCheck;
using FsCheck.Fluent;

namespace Funcky.Test;

internal class MoneyArbitraries
{
    public static Arbitrary<Currency> ArbitraryCurrency()
         => Arb.From(Gen.Elements<Currency>(Currency.AllCurrencies));

    public static Arbitrary<Money> ArbitraryMoney()
        => GenerateMoney().ToArbitrary();

    public static Arbitrary<SwissMoney> ArbitrarySwissMoney()
        => GenerateSwissFranc().ToArbitrary();

    private static Gen<Money> GenerateMoney()
        => from currency in ArbitraryCurrency().Generator
           from amount in ArbMap.Default.GeneratorFor<int>()
           select new Money(Power.OfATenth(currency.MinorUnitDigits) * amount, currency);

    private static Gen<SwissMoney> GenerateSwissFranc()
        => from amount in ArbMap.Default.GeneratorFor<int>()
           select new SwissMoney(Money.CHF(SwissMoney.SmallestCoin * amount));
}
