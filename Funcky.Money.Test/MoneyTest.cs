using System.Collections;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Funcky.Test;

public sealed class MoneyTest
{
    public MoneyTest() =>
        Arb
            .Register<MoneyArbitraries>();

    private static MoneyEvaluationContext<decimal> SwissRounding
        => MoneyEvaluationContext<decimal>
            .Builder
            .Default
            .WithTargetCurrency(Currency.CHF)
            .WithSmallestDistributionUnit(SwissMoney.SmallestCoin)
            .Build();

    [Property]
    public Property EvaluatingAMoneyInTheSameCurrencyDoesReturnTheSameAmount(Money<decimal> money)
    {
        return (money.Amount == money.Evaluate().Amount).ToProperty();
    }

    [Property]
    public Property TheSumOfTwoMoneysIsCommutative(Money<decimal> money1, Money<decimal> money2)
    {
        money2 = new Money<decimal>(money2.Amount, money1.Currency);

        return (money1.Add(money2).Evaluate().Amount == money2.Add(money1).Evaluate().Amount).ToProperty();
    }

    [Fact]
    public void WeCanBuildTheSumOfTwoMoneysWithDifferentCurrenciesButOnEvaluationYouNeedAnEvaluationContextWithDefinedExchangeRates()
    {
        var fiveFrancs = new Money<decimal>(5, Currency.CHF);
        var tenDollars = new Money<decimal>(10, Currency.USD);

        var sum = fiveFrancs.Add(tenDollars);

        Assert.Throws<MissingEvaluationContextException>(() => sum.Evaluate());
        Assert.Throws<MissingExchangeRateException>(() => sum.Evaluate(SwissRounding));
    }

    [Property]
    public Property DollarsAreNotFrancs(decimal amount)
    {
        var francs = Money<decimal>.CHF(amount);
        var dollars = Money<decimal>.USD(amount);

        return (francs != dollars).ToProperty();
    }

    [Property]
    public Property MoneyCanBeMultipliedByConstantsFactors(Money<decimal> someMoney, decimal multiplier)
    {
        var result = decimal.Round(someMoney.Amount * multiplier, someMoney.Currency.MinorUnitDigits)
                     == someMoney.Multiply(multiplier).Evaluate().Amount;

        return result.ToProperty();
    }

    [Property]
    public Property DistributeMoneyConservesTheTotal(SwissMoney someMoney, PositiveInt numberOfParts)
        => TheSumOfThePartsIsEqualToTheTotal(someMoney.Get.Amount, Distributed(someMoney, numberOfParts.Get))
            .ToProperty();

    [Property]
    public Property DistributeMoneyHasNumberOfParts(SwissMoney someMoney, PositiveInt numberOfParts)
        => TheNumberOfPartsIsCorrect(numberOfParts.Get, Distributed(someMoney, numberOfParts.Get))
            .ToProperty();

    [Property]
    public Property DistributeMoneyHasMinimalDifference(SwissMoney someMoney, PositiveInt numberOfParts)
        => TheIndividualPartsAreAtMostOneUnitApart(Distributed(someMoney, numberOfParts.Get))
            .ToProperty();

    [Theory]
    [MemberData(nameof(ProportionalDistributionData))]
    public void DistributeMoneyProportionally(int first, int second, decimal expected1, decimal expected2)
    {
        var fiftyCents = Money<decimal>.EUR(0.5m);
        var sum = fiftyCents.Add(fiftyCents);
        var distribution = sum.Distribute(new[] { first, second });

        var distributed = distribution.Select(e => e.Evaluate().Amount).ToList();
        Assert.Equal(sum.Evaluate().Amount, distributed.Sum());

        Assert.Collection(
            distributed,
            item => Assert.Equal(expected1, item),
            item => Assert.Equal(expected2, item));
    }

    [Theory]
    [MemberData(nameof(ProportionalDistributionData))]
    public void DistributeNegativeMoneyProportionally(int first, int second, decimal expected1, decimal expected2)
    {
        var fiftyCents = Money<decimal>.EUR(-0.5m);
        var sum = fiftyCents.Add(fiftyCents);
        var distribution = sum.Distribute(new[] { first, second });

        var distributed = distribution.Select(e => e.Evaluate().Amount).ToList();
        Assert.Equal(sum.Evaluate().Amount, distributed.Sum());

        Assert.Collection(
            distributed,
            item => Assert.Equal(expected1 * -1, item),
            item => Assert.Equal(expected2 * -1, item));
    }

    public static TheoryData<int, int, decimal, decimal> ProportionalDistributionData()
        => new()
        {
            { 1, 1, 0.5m, 0.5m },
            { 200, 200, 0.5m, 0.5m },
            { 5, 1, 0.84m, 0.16m },
            { 1, 5, 0.17m, 0.83m },
            { 7, 2, 0.78m, 0.22m },
            { 2, 7, 0.23m, 0.77m },
            { 98, 1, 0.99m, 0.01m },
            { 1, 98, 0.02m, 0.98m },
        };

    [Fact]
    public void InputValuesGetRoundedDuringEvaluation()
    {
        var fiveDollarsSeventy = Money<decimal>.USD(5.7m);
        var midpoint1 = Money<decimal>.USD(5.715m);
        var midpoint2 = Money<decimal>.USD(5.725m);
        var pi = Money<decimal>.USD((decimal)Math.PI);

        Assert.Equal(5.70m, fiveDollarsSeventy.Evaluate().Amount);
        Assert.Equal(5.72m, midpoint1.Evaluate().Amount);
        Assert.Equal(5.72m, midpoint2.Evaluate().Amount);
        Assert.Equal(3.14m, pi.Evaluate().Amount);
    }

    [Fact]
    public void WeCanEvaluateASumOfDifferentCurrenciesWithAContextWhichDefinesExchangeRates()
    {
        var fiveFrancs = new Money<decimal>(5, Currency.CHF);
        var tenDollars = new Money<decimal>(10, Currency.USD);
        var fiveEuros = new Money<decimal>(5, Currency.EUR);

        var sum = fiveFrancs.Add(tenDollars).Add(fiveEuros).Multiply(2);

        var context = MoneyEvaluationContext<decimal>
            .Builder
            .Default
            .WithTargetCurrency(Currency.CHF)
            .WithExchangeRate(Currency.USD, 0.9004m)
            .WithExchangeRate(Currency.EUR, 1.0715m)
            .Build();

        Assert.Equal(38.72m, sum.Evaluate(context).Amount);
    }

    [Fact]
    public void WeCanDefineMoneyExpressionsWithOperators()
    {
        var fiveDollars = Money<decimal>.USD(5);
        var tenDollars = Money<decimal>.USD(10);

        var sum = fiveDollars + tenDollars + fiveDollars;
        var product = 3.00m * tenDollars;

        Assert.Equal(Money<decimal>.USD(20), sum.Evaluate());
        Assert.Equal(Money<decimal>.USD(30), product.Evaluate());
    }

    [Property]
    public Property TheMoneyNeutralElementWorksWithAnyCurrency(Money<decimal> money)
    {
        return (money == (money + Money<decimal>.Zero).Evaluate()
                && (money == (Money<decimal>.Zero + money).Evaluate())).ToProperty().When(!money.IsZero);
    }

    [Property]
    public Property InASumOfMultipleZerosWithDifferentCurrenciesTheEvaluationHasTheSameCurrencyAsTheFirstMoneyInTheExpression(Currency c1, Currency c2, Currency c3)
    {
        var sum = new Money<decimal>(0m, c1) + new Money<decimal>(0m, c2) + new Money<decimal>(0m, c3) + new Money<decimal>(0m, c2);

        return (sum.Evaluate().Currency == c1).ToProperty();
    }

    [Fact]
    public void MoneyFormatsCorrectlyAccordingToTheCurrency()
    {
        var thousandFrancs = Money<decimal>.CHF(-1000);
        var thousandDollars = Money<decimal>.USD(-1000);

        Assert.Equal("CHF-1’000.00", thousandFrancs.ToString());
        Assert.Equal("-$1,000.00", thousandDollars.ToString());
    }

    [Fact]
    public void MoneyParsesCorrectlyFromString()
    {
        var r1 = FunctionalAssert.Some(Money<decimal>.ParseOrNone("CHF-1’000.00", Currency.CHF));
        Assert.Equal(new Money<decimal>(-1000, Currency.CHF), r1);

        var r2 = FunctionalAssert.Some(Money<decimal>.ParseOrNone("-$1,000.00", Currency.USD));
        Assert.Equal(new Money<decimal>(-1000, Currency.USD), r2);

        var r3 = FunctionalAssert.Some(Money<decimal>.ParseOrNone("1000", Currency.CHF));
        Assert.Equal(new Money<decimal>(1000, Currency.CHF), r3);
    }

    [Fact]
    public void CurrenciesWithoutFormatProviders()
    {
        // XAU is gold and therefore has no format provider, any decimal or thousand separator is therefore arbitrary.
        // Funcky therefore chooses the current culture to format such currencies, that is why we set a specific one here.
        using var cultureSwitch = new TemporaryCultureSwitch("de-CH");

        var currencyWithoutFormatProvider = Money<decimal>.XAU(9585);
        Assert.Equal("9’585 XAU", currencyWithoutFormatProvider.ToString());

        var money = FunctionalAssert.Some(Money<decimal>.ParseOrNone("9’585.00 XAU", Currency.XAU));
        Assert.Equal(new Money<decimal>(9585, Currency.XAU), money);
    }

    [Property]
    public Property WeCanParseTheStringsWeGenerate(Money<decimal> money)
    {
        return Money<decimal>.ParseOrNone(money.ToString(), money.Currency).Match(false, m => m == money).ToProperty();
    }

    [Fact]
    public void ThePrecisionCanBeSetToSomethingOtherThanAPowerOfTen()
    {
        var precision05 = new Money<decimal>(1m, SwissRounding);
        var precision002 = new Money<decimal>(1m, MoneyEvaluationContext<decimal>.Builder.Default.WithTargetCurrency(Currency.CHF).WithRounding(RoundingStrategy<decimal>.BankersRounding(0.002m)).Build());

        Assert.Collection(
            precision05.Distribute(3, 0.05m).Select(e => e.Evaluate().Amount),
            item => Assert.Equal(0.35m, item),
            item => Assert.Equal(0.35m, item),
            item => Assert.Equal(0.30m, item));

        Assert.Collection(
            precision002.Distribute(3, 0.002m).Select(e => e.Evaluate().Amount),
            item => Assert.Equal(0.334m, item),
            item => Assert.Equal(0.334m, item),
            item => Assert.Equal(0.332m, item));

        var francs = new Money<decimal>(0.10m, SwissRounding);
        Assert.Collection(
            francs.Distribute(3, 0.05m).Select(e => e.Evaluate().Amount),
            item => Assert.Equal(0.05m, item),
            item => Assert.Equal(0.05m, item),
            item => Assert.Equal(0m, item));
    }

    [Fact]
    public void TheRoundingStrategyIsCorrectlyPassedThrough()
    {
        var commonContext = MoneyEvaluationContext<decimal>
            .Builder
            .Default
            .WithTargetCurrency(Currency.CHF);

        var precision05 = new Money<decimal>(1m, commonContext.WithRounding(RoundingStrategy<decimal>.BankersRounding(SwissMoney.SmallestCoin)).Build());
        var precision002 = new Money<decimal>(1m, commonContext.WithRounding(RoundingStrategy<decimal>.BankersRounding(0.002m)).Build());

        Assert.Equal(precision05.RoundingStrategy, precision05.Distribute(3, SwissMoney.SmallestCoin).First().Evaluate().RoundingStrategy);
        Assert.Equal(precision002.RoundingStrategy, precision002.Distribute(3, 0.002m).First().Evaluate().RoundingStrategy);
    }

    [Fact]
    public void DistributionMustDistributeExactlyTheGivenAmount()
    {
        var francs = new Money<decimal>(0.08m, SwissRounding);

        Assert.Throws<ImpossibleDistributionException>(()
            => francs.Distribute(3, 0.05m).Select(e => e.Evaluate()).First());
    }

    [Fact]
    public void DefaultRoundingStrategyIsBankersRounding()
    {
        var francs = new Money<decimal>(1m, SwissRounding);
        var evaluationContext = MoneyEvaluationContext<decimal>.Builder.Default.WithTargetCurrency(Currency.CHF);

        Assert.Equal(new BankersRounding<decimal>(0.05m), francs.RoundingStrategy);
        Assert.Equal(new BankersRounding<decimal>(0.01m), francs.Evaluate(evaluationContext.Build()).RoundingStrategy);
    }

    [Fact]
    public void WeCanDelegateTheExchangeRatesToABank()
    {
        var fiveFrancs = new Money<decimal>(5, Currency.CHF);
        var tenDollars = new Money<decimal>(10, Currency.USD);
        var fiveEuros = new Money<decimal>(5, Currency.EUR);

        var sum = (fiveFrancs + tenDollars + fiveEuros) * 1.5m;

        Assert.Equal(30m, sum.Evaluate(OneToOneContext(Currency.CHF)).Amount);
    }

    [Fact]
    public void EvaluationOnZeroMoneysWorks()
    {
        var sum = (Money<decimal>.Zero + Money<decimal>.Zero) * 1.5m;

        var context = OneToOneContext(Currency.JPY);

        Assert.True(Money<decimal>.Zero.Evaluate(context).IsZero);
        Assert.True(sum.Evaluate(context).IsZero);
        Assert.True(Money<decimal>.Zero.Evaluate().IsZero);
        Assert.True(sum.Evaluate().IsZero);
    }

    [Fact]
    public void DifferentPrecisionsCannotBeEvaluatedWithoutAnEvaluationContext()
    {
        var francs = MoneyEvaluationContext<decimal>
            .Builder
            .Default
            .WithTargetCurrency(Currency.CHF);

        var normalFrancs = francs.WithRounding(RoundingStrategy<decimal>.BankersRounding(0.05m));
        var preciseFrancs = francs.WithRounding(RoundingStrategy<decimal>.BankersRounding(0.001m));

        var two = new Money<decimal>(2, normalFrancs.Build());
        var oneHalf = new Money<decimal>(0.5m, preciseFrancs.Build());
        var sum = (two + oneHalf) * 0.01m;

        Assert.Throws<MissingEvaluationContextException>(() => sum.Evaluate());
        Assert.Equal(0.02m, sum.Evaluate(francs.Build()).Amount);
    }

    [Fact]
    public void DifferentRoundingStrategiesCannotBeEvaluatedWithoutAnEvaluationContext()
    {
        var francs = MoneyEvaluationContext<decimal>
            .Builder
            .Default
            .WithTargetCurrency(Currency.CHF);

        var normalFrancs = francs.WithRounding(RoundingStrategy<decimal>.RoundWithAwayFromZero(0.05m));
        var preciseFrancs = francs.WithRounding(RoundingStrategy<decimal>.BankersRounding(0.001m));

        var two = new Money<decimal>(2, normalFrancs.Build());
        var oneHalf = new Money<decimal>(0.5m, preciseFrancs.Build());
        var sum = (two + oneHalf) * 0.05m;

        Assert.Throws<MissingEvaluationContextException>(() => sum.Evaluate());
        Assert.Equal(0.15m, sum.Evaluate(normalFrancs.Build()).Amount);
    }

    [Fact]
    public void RoundingIsOnlyDoneAtTheEndOfTheEvaluation()
    {
        var francs = new Money<decimal>(0.01m, SwissRounding);
        var noRounding = MoneyEvaluationContext<decimal>
            .Builder
            .Default
            .WithTargetCurrency(Currency.CHF)
            .WithRounding(RoundingStrategy<decimal>.NoRounding())
            .Build();

        Assert.Equal(0.01m, francs.Amount);
        Assert.Equal(0m, francs.Evaluate().Amount);
        Assert.Equal(0.01m, francs.Evaluate(noRounding).Amount);
    }

    [Fact]
    public void AllNecessaryOperatorsAreDefined()
    {
        var francs = new Money<decimal>(0.50m, SwissRounding);

        var allOperators = -((((francs * 2) + francs) / 2) - +(francs * 2));

        Assert.Equal(0.25m, allOperators.Evaluate().Amount);
    }

    [Fact]
    public void WeCanEvaluateComplexExpressions()
    {
        var tree = ComplexExpression();

        Assert.Equal(35m, tree.Evaluate().Amount);
    }

    [Fact]
    public void BuildContextFailsWhenCreatingItWithARoundingStrategyIncompatibleWithSmallestDistributionUnit()
    {
        var incompatibleRoundingContext = MoneyEvaluationContext<decimal>
            .Builder
            .Default
            .WithTargetCurrency(Currency.CHF)
            .WithSmallestDistributionUnit(0.01m)
            .WithRounding(RoundingStrategy<decimal>.Default(0.05m));

        Assert.Throws<IncompatibleRoundingException>(() => incompatibleRoundingContext.Build());
    }

    [Theory]
    [MemberData(nameof(CulturesWithCurrencies))]
    public void IfCurrencyIsOmittedOnConstructionOfAMoneyItGetsDeducedByTheCurrentCulture(string culture, Currency currency)
    {
        using var cultureSwitch = new TemporaryCultureSwitch(culture);

        var money = new Money<decimal>(5);

        Assert.Equal(currency, money.Currency);
    }

    public static TheoryData<string, Currency> CulturesWithCurrencies()
        => new()
        {
            { "jp-JP", Currency.JPY },
            { "de-CH", Currency.CHF },
            { "de-DE", Currency.EUR },
            { "en-GB", Currency.GBP },
            { "en-US", Currency.USD },
        };

    [Fact]
    public void YouCannotConstructAMoneyWithIllegalCulturesWithoutACorrectCurrencyIsoCode()
    {
        using var cultureSwitch = new TemporaryCultureSwitch("en-UK");

        Assert.Throws<NotSupportedException>(() => new Money<decimal>(5));
    }

    [Fact]
    public void ToHumanReadableExtensionTransformsExpressionsCorrectly()
    {
        var distribution = ((Money<decimal>.CHF(1.5m) + Money<decimal>.EUR(2.5m)) * 3).Distribute(new[] { 3, 1, 3, 2 });
        var expression = (distribution.Skip(2).First() + (Money<decimal>.USD(2.99m) * 2)) / 2;
        var sum = Money<decimal>.CHF(30) + Money<decimal>.JPY(500);
        var product = Money<decimal>.CHF(100) * 2.5m;
        var difference = Money<decimal>.CHF(200) - Money<decimal>.JPY(500);
        var quotient = Money<decimal>.CHF(500) / 2;

        // this also shows a few quirks of the Expression-Tree (subtraction and division are only convenience)
        Assert.Equal("(((2.50CHF + ((1.5 * 7.00CHF) + 0.50CHF)) + ((2 * 7.00CHF) + 0.50CHF)) + ((2.50CHF + (((0.5 * 7.00CHF) + 0.50CHF) + (-1 * 7.00CHF))) + (7.00CHF + 0.50CHF)))", ComplexExpression().ToHumanReadable());
        Assert.Equal("(0.5 * ((3 * (1.50CHF + 2.50EUR)).Distribute(3, 1, 3, 2)[2] + (2 * 2.99USD)))", expression.ToHumanReadable());

        Assert.Equal("(30.00CHF + 500JPY)", sum.ToHumanReadable());
        Assert.Equal("(2.5 * 100.00CHF)", product.ToHumanReadable());
        Assert.Equal("(200.00CHF + (-1 * 500JPY))", difference.ToHumanReadable());
        Assert.Equal("(0.5 * 500.00CHF)", quotient.ToHumanReadable());
    }

    [Fact]
    public void RoundingStrategiesMustBeInitializedWithAValidPrecision()
    {
        Assert.Throws<InvalidPrecisionException>(() => _ = RoundingStrategy<decimal>.Default(0.0m));
        Assert.Throws<InvalidPrecisionException>(() => _ = RoundingStrategy<decimal>.BankersRounding(0.0m));
        Assert.Throws<InvalidPrecisionException>(() => _ = RoundingStrategy<decimal>.RoundWithAwayFromZero(0.0m));
    }

    [Fact]
    public void WeCanCalculateADimensionlessFactorByDividingAMoneyByAnother()
    {
        Assert.Equal(2.5m, Money<decimal>.CHF(5) / Money<decimal>.CHF(2));
        Assert.Equal(0.75m, Money<decimal>.USD(3).Divide(Money<decimal>.USD(4)));
    }

    [Fact]
    public void DividingTwoMoneysOnlyWorksIfTheyAreOfTheSameCurrency()
    {
        Assert.ThrowsAny<MissingExchangeRateException>(() => Money<decimal>.CHF(5) / Money<decimal>.USD(2));
    }

    [Fact]
    public void DividingTwoMoneysWithDifferentCurrenciesNeedAnEvaluationContext()
    {
        Assert.Equal(0.8m, Money<decimal>.CHF(4).Divide(Money<decimal>.USD(5), OneToOneContext(Currency.USD)));
    }

    [Fact]
    public void DividingTwoMoneysOnlyWorksIfTheDivisorIsNonZero()
    {
        Assert.Throws<DivideByZeroException>(() => Money<decimal>.CHF(5) / Money<decimal>.Zero);
        Assert.Throws<DivideByZeroException>(() => Money<decimal>.USD(3).Divide(Money<decimal>.USD(0)));
    }

    private static List<decimal> Distributed(SwissMoney someMoney, int numberOfParts)
        => someMoney
            .Get
            .Distribute(numberOfParts)
            .Select(e => e.Evaluate(SwissRounding).Amount)
            .ToList();

    private static bool TheIndividualPartsAreAtMostOneUnitApart(IEnumerable<decimal> distributed)
        => distributed.All(AtMostOneDistributionUnitLess(distributed.First()));

    private static Func<decimal, bool> AtMostOneDistributionUnitLess(decimal reference)
        => amount => Math.Abs(amount - reference) <= SwissMoney.SmallestCoin;

    private static bool TheNumberOfPartsIsCorrect(int numberOfParts, ICollection distributed)
        => distributed.Count == numberOfParts;

    private static bool TheSumOfThePartsIsEqualToTheTotal(decimal validAmount, IEnumerable<decimal> distributed)
        => distributed.Sum() == validAmount;

    private static IMoneyExpression<decimal> ComplexExpression()
    {
        var v1 = new Money<decimal>(0.50m, SwissRounding);
        var v2 = new Money<decimal>(7m, SwissRounding);
        var v3 = new Money<decimal>(2.50m, SwissRounding);

        return v3.Add(v2.Multiply(1.5m).Add(v1)).Add(v2.Multiply(2).Add(v1))
            .Add(v3.Add(v2.Divide(2).Add(v1).Subtract(v2)).Add(v2.Add(v1)));
    }

    private static MoneyEvaluationContext<decimal> OneToOneContext(Currency targetCurrency)
        => MoneyEvaluationContext<decimal>
            .Builder
            .Default
            .WithTargetCurrency(targetCurrency)
            .WithBank(OneToOneBank<decimal>.Instance)
            .Build();
}
