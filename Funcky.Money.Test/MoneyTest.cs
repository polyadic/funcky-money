using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FsCheck;
using FsCheck.Xunit;
using Funcky.Xunit;
using Xunit;

namespace Funcky.Test
{
    public sealed class MoneyTest
    {
        private const decimal SmallestCoin = 0.05m;

        public MoneyTest() =>
            Arb
                .Register<MoneyArbitraries>();

        private static MoneyEvaluationContext SwissRounding
            => MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF)
                .WithSmallestDistributionUnit(SmallestCoin)
                .Build();

        [Property]
        public Property EvaluatingAMoneyInTheSameCurrencyDoesReturnTheSameAmount(Money money)
        {
            return (money.Amount == money.Evaluate().Amount).ToProperty();
        }

        [Property]
        public Property TheSumOfTwoMoneysIsCommutative(Money money1, Money money2)
        {
            money2 = new Money(money2.Amount, money1.Currency);

            return (money1.Add(money2).Evaluate().Amount == money2.Add(money1).Evaluate().Amount).ToProperty();
        }

        [Fact]
        public void WeCanBuildTheSumOfTwoMoneysWithDifferentCurrenciesButOnEvaluationYouNeedAnEvaluationContext()
        {
            var fiveFrancs = new Money(5, Currency.CHF);
            var tenDollars = new Money(10, Currency.USD);

            var sum = fiveFrancs.Add(tenDollars);

            Assert.Throws<MissingEvaluationContextException>(() => sum.Evaluate());
        }

        [Property]
        public Property DollarsAreNotFrancs(decimal amount)
        {
            var francs = Money.CHF(amount);
            var dollars = Money.USD(amount);

            return (francs != dollars).ToProperty();
        }

        [Property]
        public Property MoneyCanBeMultipliedByConstantsFactors(Money someMoney, decimal multiplier)
        {
            var result = decimal.Round(someMoney.Amount * multiplier, someMoney.Currency.MinorUnitDigits)
                         == someMoney.Multiply(multiplier).Evaluate().Amount;

            return result.ToProperty();
        }

        [Property]
        public Property DistributeMoneyEqually(SwissMoney someMoney, PositiveInt numberOfParts)
        {
            var distributed = someMoney.Get.Distribute(numberOfParts.Get).Select(e => e.Evaluate(SwissRounding).Amount).ToList();
            var first = distributed.First();

            return (TheSumOfThePartsIsEqualToTheTotal(distributed, someMoney.Get.Amount)
                   && TheNumberOfPartsIsCorrect(numberOfParts, distributed)
                   && TheIndividualPartsAreAtMostOneUnitApart(distributed, first)).ToProperty();
        }

        [Theory]
        [MemberData(nameof(ProportionalDistributionData))]
        public void DistributeMoneyProportionally(int first, int second, decimal expected1, decimal expected2)
        {
            var fiftyCents = Money.EUR(0.5m);
            var sum = fiftyCents.Add(fiftyCents);
            var distribution = sum.Distribute(new[] { first, second });

            var distributed = distribution.Select(e => e.Evaluate().Amount).ToList();
            Assert.Equal(sum.Evaluate().Amount, distributed.Sum());

            Assert.Collection(
                distributed,
                item => Assert.Equal(expected1, item),
                item => Assert.Equal(expected2, item));
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
            var fiveDollarsSeventy = Money.USD(5.7m);
            var midpoint1 = Money.USD(5.715m);
            var midpoint2 = Money.USD(5.725m);
            var pi = Money.USD((decimal)Math.PI);

            Assert.Equal(5.70m, fiveDollarsSeventy.Evaluate().Amount);
            Assert.Equal(5.72m, midpoint1.Evaluate().Amount);
            Assert.Equal(5.72m, midpoint2.Evaluate().Amount);
            Assert.Equal(3.14m, pi.Evaluate().Amount);
        }

        [Fact]
        public void WeCanEvaluateASumOfDifferentCurrenciesWithAContextWhichDefinesExchangeRates()
        {
            var fiveFrancs = new Money(5, Currency.CHF);
            var tenDollars = new Money(10, Currency.USD);
            var fiveEuros = new Money(5, Currency.EUR);

            var sum = fiveFrancs.Add(tenDollars).Add(fiveEuros).Multiply(2);

            var context = MoneyEvaluationContext
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
            var fiveDollars = Money.USD(5);
            var tenDollars = Money.USD(10);

            var sum = fiveDollars + tenDollars + fiveDollars;
            var product = 3.00m * tenDollars;

            Assert.Equal(Money.USD(20), sum.Evaluate());
            Assert.Equal(Money.USD(30), product.Evaluate());
        }

        [Property]
        public Property TheMoneyNeutralElementWorksWithAnyCurrency(Money money)
        {
            return (money == (money + Money.Zero).Evaluate()
                    && (money == (Money.Zero + money).Evaluate())).ToProperty().When(!money.IsZero);
        }

        [Property]
        public Property InASumOfMultipleZerosWithDifferentCurrenciesTheEvaluationHasTheSameCurrencyAsTheFirstMoneyInTheExpression(Currency c1, Currency c2, Currency c3)
        {
            var sum = new Money(0m, c1) + new Money(0m, c2) + new Money(0m, c3) + new Money(0m, c2);

            return (sum.Evaluate().Currency == c1).ToProperty();
        }

        [Fact]
        public void MoneyFormatsCorrectlyAccordingToTheCurrency()
        {
            var thousandFrancs = Money.CHF(-1000);
            var thousandDollars = Money.USD(-1000);

            Assert.Equal("CHF-1’000.00", thousandFrancs.ToString());
            Assert.Equal("-$1,000.00", thousandDollars.ToString());
        }

        [Fact]
        public void MoneyParsesCorrectlyFromString()
        {
            var r1 = FunctionalAssert.IsSome(Money.ParseOrNone("CHF-1’000.00", Currency.CHF));
            Assert.Equal(new Money(-1000, Currency.CHF), r1);

            var r2 = FunctionalAssert.IsSome(Money.ParseOrNone("-$1,000.00", Currency.USD));
            Assert.Equal(new Money(-1000, Currency.USD), r2);

            var r3 = FunctionalAssert.IsSome(Money.ParseOrNone("1000", Currency.CHF));
            Assert.Equal(new Money(1000, Currency.CHF), r3);
        }

        [Fact]
        public void CurrenciesWithoutFormatProviders()
        {
            // XAU is gold and therefore has no format provider, any decimal or thousand separator is therefore arbitrary.
            // Funcky therefore chooses the current culture to format such currencies, that is why we set a specific one here.
            using var cultureSwitch = new TemporaryCultureSwitch("CHF");

            var currencyWithoutFormatProvider = Money.XAU(9585);
            Assert.Equal("9’585 XAU", currencyWithoutFormatProvider.ToString());

            var money = FunctionalAssert.IsSome(Money.ParseOrNone("9’585.00 XAU", Currency.XAU));
            Assert.Equal(new Money(9585, Currency.XAU), money);
        }

        [Property]
        public Property WeCanParseTheStringsWeGenerate(Money money)
        {
            return Money.ParseOrNone(money.ToString(), money.Currency).Match(false, m => m == money).ToProperty();
        }

        [Fact]
        public void ThePrecisionCanBeSetToSomethingOtherThanAPowerOfTen()
        {
            var precision05 = new Money(1m, SwissRounding);
            var precision002 = new Money(1m, MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.CHF).WithRounding(RoundingStrategy.BankersRounding(0.002m)).Build());

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

            var francs = new Money(0.10m, SwissRounding);
            Assert.Collection(
                francs.Distribute(3, 0.05m).Select(e => e.Evaluate().Amount),
                item => Assert.Equal(0.05m, item),
                item => Assert.Equal(0.05m, item),
                item => Assert.Equal(0m, item));
        }

        [Fact]
        public void TheRoundingStrategyIsCorrectlyPassedThrough()
        {
            var commonContext = MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF);

            var precision05 = new Money(1m, commonContext.WithRounding(RoundingStrategy.BankersRounding(SmallestCoin)).Build());
            var precision002 = new Money(1m, commonContext.WithRounding(RoundingStrategy.BankersRounding(0.002m)).Build());

            Assert.Equal(precision05.RoundingStrategy, precision05.Distribute(3, SmallestCoin).First().Evaluate().RoundingStrategy);
            Assert.Equal(precision002.RoundingStrategy, precision002.Distribute(3, 0.002m).First().Evaluate().RoundingStrategy);
        }

        [Fact]
        public void DistributionMustDistributeExactlyTheGivenAmount()
        {
            var francs = new Money(0.08m, SwissRounding);

            Assert.Throws<ImpossibleDistributionException>(()
                => francs.Distribute(3, 0.05m).Select(e => e.Evaluate()).First());
        }

        [Fact]
        public void DefaultRoundingStrategyIsBankersRounding()
        {
            var francs = new Money(1m, SwissRounding);
            var evaluationContext = MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.CHF);

            Assert.Equal(new BankersRounding(0.05m), francs.RoundingStrategy);
            Assert.Equal(new BankersRounding(0.01m), francs.Evaluate(evaluationContext.Build()).RoundingStrategy);
        }

        [Fact]
        public void WeCanDelegateTheExchangeRatesToABank()
        {
            var fiveFrancs = new Money(5, Currency.CHF);
            var tenDollars = new Money(10, Currency.USD);
            var fiveEuros = new Money(5, Currency.EUR);

            var sum = (fiveFrancs + tenDollars + fiveEuros) * 1.5m;

            var context = MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF)
                .WithBank(OneToOneBank.Instance)
                .Build();

            Assert.Equal(30m, sum.Evaluate(context).Amount);
        }

        [Fact]
        public void EvaluationOnZeroMoniesWorks()
        {
            var sum = (Money.Zero + Money.Zero) * 1.5m;

            var context = MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.JPY)
                .WithBank(OneToOneBank.Instance)
                .Build();

            Assert.True(Money.Zero.Evaluate(context).IsZero);
            Assert.True(sum.Evaluate(context).IsZero);
            Assert.True(Money.Zero.Evaluate().IsZero);
            Assert.True(sum.Evaluate().IsZero);
        }

        [Fact]
        public void DifferentPrecisionsCannotBeEvaluatedWithoutAnEvaluationContext()
        {
            var francs = MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF);

            var normalFrancs = francs.WithRounding(RoundingStrategy.BankersRounding(0.05m));
            var preciseFrancs = francs.WithRounding(RoundingStrategy.BankersRounding(0.001m));

            var two = new Money(2, normalFrancs.Build());
            var oneHalf = new Money(0.5m, preciseFrancs.Build());
            var sum = (two + oneHalf) * 0.01m;

            Assert.Throws<MissingEvaluationContextException>(() => sum.Evaluate());
            Assert.Equal(0.02m, sum.Evaluate(francs.Build()).Amount);
        }

        [Fact]
        public void DifferentRoundingStrategiesCannotBeEvaluatedWithoutAnEvaluationContext()
        {
            var francs = MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF);

            var normalFrancs = francs.WithRounding(RoundingStrategy.RoundWithAwayFromZero(0.05m));
            var preciseFrancs = francs.WithRounding(RoundingStrategy.BankersRounding(0.001m));

            var two = new Money(2, normalFrancs.Build());
            var oneHalf = new Money(0.5m, preciseFrancs.Build());
            var sum = (two + oneHalf) * 0.05m;

            Assert.Throws<MissingEvaluationContextException>(() => sum.Evaluate());
            Assert.Equal(0.15m, sum.Evaluate(normalFrancs.Build()).Amount);
        }

        [Fact]
        public void RoundingIsOnlyDoneAtTheEndOfTheEvaluation()
        {
            var francs = new Money(0.01m, SwissRounding);
            var noRounding = MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF)
                .WithRounding(RoundingStrategy.NoRounding())
                .Build();

            Assert.Equal(0.01m, francs.Amount);
            Assert.Equal(0m, francs.Evaluate().Amount);
            Assert.Equal(0.01m, francs.Evaluate(noRounding).Amount);
        }

        [Fact]
        public void AllNecessaryOperatorsAreDefined()
        {
            var francs = new Money(0.50m, SwissRounding);

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
            var incompatibleRoundingContext = MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF)
                .WithSmallestDistributionUnit(0.01m)
                .WithRounding(RoundingStrategy.Default(0.05m));

            Assert.Throws<IncompatibleRoundingException>(() => incompatibleRoundingContext.Build());
        }

        [Theory]
        [MemberData(nameof(CulturesWithCurrencies))]
        public void IfCurrencyIsOmittedOnConstructionOfAMoneyItGetsDeducedByTheCurrentCulture(string culture, Currency currency)
        {
            using var cultureSwitch = new TemporaryCultureSwitch(culture);

            var money = new Money(5);

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

            Assert.Throws<NotSupportedException>(() => new Money(5));
        }

        [Fact]
        public void ToHumanReadableExtensionTransformsExpressionsCorrectly()
        {
            var distribution = ((Money.CHF(1.5m) + Money.EUR(2.5m)) * 3).Distribute(new[] { 3, 1, 3, 2 });
            var expression = (distribution.Skip(2).First() + (Money.USD(2.99m) * 2)) / 2;
            var sum = Money.CHF(30) + Money.JPY(500);
            var product = Money.CHF(100) * 2.5m;
            var difference = Money.CHF(200) - Money.JPY(500);
            var quotient = Money.CHF(500) / 2;

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
            Assert.Throws<InvalidPrecisionException>(() => _ = RoundingStrategy.Default(0.0m));
            Assert.Throws<InvalidPrecisionException>(() => _ = RoundingStrategy.BankersRounding(0.0m));
            Assert.Throws<InvalidPrecisionException>(() => _ = RoundingStrategy.RoundWithAwayFromZero(0.0m));
        }

        private static bool TheIndividualPartsAreAtMostOneUnitApart(IEnumerable<decimal> distributed, decimal first)
            => distributed.All(AtMostOneUnitLess(first, SmallestCoin));

        private static Func<decimal, bool> AtMostOneUnitLess(decimal reference, decimal unit)
            => amount => amount == reference || amount == reference - unit;

        private static bool TheNumberOfPartsIsCorrect(PositiveInt numberOfParts, ICollection distributed)
            => distributed.Count == numberOfParts.Get;

        private static bool TheSumOfThePartsIsEqualToTheTotal(IEnumerable<decimal> distributed, decimal validAmount)
            => distributed.Sum() == validAmount;

        private static IMoneyExpression ComplexExpression()
        {
            var v1 = new Money(0.50m, SwissRounding);
            var v2 = new Money(7m, SwissRounding);
            var v3 = new Money(2.50m, SwissRounding);

            return v3.Add(v2.Multiply(1.5m).Add(v1)).Add(v2.Multiply(2).Add(v1))
                .Add(v3.Add(v2.Divide(2).Add(v1).Subtract(v2)).Add(v2.Add(v1)));
        }
    }
}
