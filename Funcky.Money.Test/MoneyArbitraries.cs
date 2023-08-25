using FsCheck;

namespace Funcky.Test;

internal class MoneyArbitraries
{
    public static Arbitrary<Currency> ArbitraryCurrency()
         => Arb.From(Gen.Elements<Currency>(Currency.AllCurrencies));

    public static Arbitrary<Money<decimal>> ArbitraryMoney()
        => GenerateMoney().ToArbitrary();

    public static Arbitrary<SwissMoney> ArbitrarySwissMoney()
        => GenerateSwissFranc().ToArbitrary();

    private static Gen<Money<decimal>> GenerateMoney()
        => from currency in Arb.Generate<Currency>()
           from amount in Arb.Generate<int>()
           select new Money<decimal>(Power<decimal>.OfATenth(currency.MinorUnitDigits) * amount, currency);

    private static Gen<SwissMoney> GenerateSwissFranc()
        => from amount in Arb.Generate<int>()
           select new SwissMoney(Money<decimal>.CHF(SwissMoney.SmallestCoin * amount));
}
