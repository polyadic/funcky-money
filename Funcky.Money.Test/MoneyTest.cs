using System;
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

        private static MoneyEvaluationContext SwissRounding
            => MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF)
                .WithSmallestDistributionUnit(SmallestCoin)
                .Build();

        [Fact]
        public void WeCanCreateAMoneyFromDifferentTypesAndTheAmountIsADecimal()
        {
            var fiveDollars = Money.USD(5);
            var fiveDollarsFifty = Money.USD(5.5m);
            var fiveDollarsSeventy = Money.USD(5.7m);
            var fiveDollarsNinety = Money.USD(5.90m);

            Assert.Equal(5.00m, fiveDollars.Amount);
            Assert.Equal(5.50m, fiveDollarsFifty.Amount);
            Assert.Equal(5.70m, fiveDollarsSeventy.Amount);
            Assert.Equal(5.90m, fiveDollarsNinety.Amount);
        }

        [Fact]
        public void EvaluatingAMoneyInTheSameCurrencyDoesReturnTheSameAmount()
        {
            IMoneyExpression fiveDollars = Money.USD(5);

            Assert.Equal(fiveDollars, fiveDollars.Evaluate());
        }

        [Fact]
        public void TheSumOfTwoMoneysEvaluatesCorrectly()
        {
            var fiveDollars = Money.USD(5);
            var tenDollars = Money.USD(10);

            Assert.Equal(15.00m, fiveDollars.Add(tenDollars).Evaluate().Amount);
        }

        [Fact]
        public void WeCanBuildTheSumOfTwoMoneysWithDifferentCurrenciesButOnEvaluationYouNeedAnEvaluationContext()
        {
            var fiveFrancs = new Money(5, Currency.CHF);
            var tenDollars = new Money(10, Currency.USD);

            var sum = fiveFrancs.Add(tenDollars);

            Assert.Throws<MissingEvaluationContextException>(() => sum.Evaluate());
        }

        [Fact]
        public void FiveDollarsAreNotFiveFrancs()
        {
            var fiveFrancs = Money.CHF(5);
            var fiveDollars = Money.USD(5);

            Assert.NotEqual(fiveFrancs, fiveDollars);
        }

        [Property]
        public Property MoneyCanBeMultipliedByConstantsFactors(decimal amount, decimal multiplier)
        {
            var someMoney = Money.CHF(amount);
            var result = decimal.Round(amount * multiplier, someMoney.Currency.MinorUnitDigits)
                         == someMoney.Multiply(multiplier).Evaluate().Amount;

            return result.ToProperty();
        }

        [Property]
        public Property DistributeMoneyEqually()
            => Prop.ForAll(
                Arb.Default.Decimal(),
                Arb.Default.PositiveInt(),
                (amount, numberOfParts) =>
                        {
                            var validAmount = SwissRounding.RoundingStrategy.Round(amount);
                            var someMoney = new Money(validAmount, SwissRounding);
                            var distributed = someMoney.Distribute(numberOfParts.Get).Select(e => e.Evaluate(SwissRounding).Amount).ToList();
                            var first = distributed.First();

                            return distributed.Sum() == validAmount
                                   && distributed.Count == numberOfParts.Get
                                   && distributed.All(AtMostOneUnitLess(first, SmallestCoin));
                        });

        [Fact]
        public void DistributeMoneyProportionally()
        {
            var fiftyCents = Money.EUR(0.5m);
            var sum = fiftyCents.Add(fiftyCents);
            var distribution = sum.Distribute(new[] { 5, 1 });

            var distributed = distribution.Select(e => e.Evaluate().Amount).ToList();
            Assert.Equal(sum.Evaluate().Amount, distributed.Sum());

            Assert.Collection(
                distributed,
                item => Assert.Equal(0.84m, item),
                item => Assert.Equal(0.16m, item));
        }

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

        [Fact]
        public void TheMoneyNeutralElementIsWorkingWithAnyCurrency()
        {
            var fiveFrancs = Money.CHF(5);
            var fiveDollars = Money.USD(5);

            Assert.Equal(fiveFrancs, (fiveFrancs + Money.Zero).Evaluate());
            Assert.Equal(fiveDollars, (fiveDollars + Money.Zero).Evaluate());
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

            Assert.Throws<ImpossibleDistributionException>(() =>
                francs.Distribute(3, 0.05m).Select(e => e.Evaluate()).First());
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
            var v1 = new Money(0.50m, SwissRounding);
            var v2 = new Money(7m, SwissRounding);
            var v3 = new Money(2.50m, SwissRounding);

            var tree = v3.Add(v2.Multiply(1.5m).Add(v1)).Add(v2.Multiply(2).Add(v1))
                .Add(v3.Add(v2.Divide(2).Add(v1).Subtract(v2)).Add(v2.Add(v1)));

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

        private Func<decimal, bool> AtMostOneUnitLess(decimal reference, decimal unit)
            => amount => amount == reference || amount == reference - unit;
    }
}
