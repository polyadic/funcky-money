using System;
using System.Linq;
using Funcky.Xunit;
using Xunit;

namespace Funcky.Test
{
    public class MoneyTest
    {
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
        public void WeCanBuildTheSumOfTwoMoneysWithDifferentCurrenciesButOnEvaluationYouNeedAEvaluationContext()
        {
            var fiveFrancs = new Money(5, Currency.CHF());
            var tenDollars = new Money(10, Currency.USD());
            var sum = fiveFrancs.Add(tenDollars);

            Assert.Throws<MissingEvaluationContextException>(() => sum.Evaluate());
        }

        [Fact]
        public void FiveDollarsAreNotFiveFrancs()
        {
            var fiveFrancs = new Money(5, Currency.CHF());
            var fiveDollars = new Money(5, Currency.USD());

            Assert.NotEqual(fiveFrancs, fiveDollars);
        }

        [Fact]
        public void MoneyCanBeMultipliedByConstantsFactors()
        {
            var fiveFrancs = new Money(5);

            Assert.Equal(15.00m, fiveFrancs.Multiply(3).Evaluate().Amount);
            Assert.Equal(16.00m, fiveFrancs.Multiply(3.2).Evaluate().Amount);
            Assert.Equal(17.50m, fiveFrancs.Multiply(3.5f).Evaluate().Amount);
            Assert.Equal(19.00m, fiveFrancs.Multiply(3.8m).Evaluate().Amount);
        }

        [Fact]
        public void DistributeMoneyEqually()
        {
            var fiveFrancs = new Money(5);
            var sum = fiveFrancs.Add(fiveFrancs);
            var distribution = sum.Distribute(3);

            Assert.Equal(sum.Evaluate().Amount, distribution.Select(e => e.Evaluate().Amount).Sum());

            Assert.Collection(
                distribution.Select(e => e.Evaluate().Amount),
                item => Assert.Equal(3.34m, item),
                item => Assert.Equal(3.33m, item),
                item => Assert.Equal(3.33m, item));
        }

        [Fact]
        public void DistributeMoneyProportionally()
        {
            var fiftyCents = new Money(0.5m);
            var sum = fiftyCents.Add(fiftyCents);
            var distribution = sum.Distribute(new[] { 5, 1 });

            Assert.Equal(sum.Evaluate().Amount, distribution.Select(e => e.Evaluate().Amount).Sum());

            Assert.Collection(
                distribution.Select(e => e.Evaluate().Amount),
                item => Assert.Equal(0.84m, item),
                item => Assert.Equal(0.16m, item));
        }

        [Fact]
        public void InputValuesGetRoundedUntilEvaluation()
        {
            var fiveDollarsSeventy = new Money(5.7m);
            var midpoint1 = new Money(5.715m);
            var midpoint2 = new Money(5.725m);
            var pi = new Money((decimal)Math.PI);

            Assert.Equal(5.70m, fiveDollarsSeventy.Amount);
            Assert.Equal(5.715m, midpoint1.Amount);
            Assert.Equal(5.725m, midpoint2.Amount);
            Assert.Equal(3.14159265358979m, pi.Amount);
        }

        [Fact]
        public void WeCanEvaluateASumOfDifferentCurrenciesWithAContextWhichDefinesExchangeRates()
        {
            var fiveFrancs = new Money(5, Currency.CHF());
            var tenDollars = new Money(10, Currency.USD());
            var fiveEuros = new Money(5, Currency.EUR());

            var sum = fiveFrancs.Add(tenDollars).Add(fiveEuros).Multiply(2);

            var context = MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF())
                .WithExchangeRate(Currency.USD(), 0.9004m)
                .WithExchangeRate(Currency.EUR(), 1.0715m)
                .Build();

            Assert.Equal(38.7230m, sum.Evaluate(context).Amount);
        }

        [Fact]
        public void WeCanDefineMoneyExpressionsWithOperators()
        {
            var fiveDollars = new Money(5);
            var tenDollars = new Money(10);

            var sum = fiveDollars + tenDollars + fiveDollars;
            var product = 3.00m * tenDollars;

            Assert.Equal(new Money(20), sum.Evaluate());
            Assert.Equal(new Money(30), product.Evaluate());
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
            var r1 = FunctionalAssert.IsSome(Money.ParseOrNone("CHF-1’000.00", Currency.CHF()));
            Assert.Equal(new Money(-1000, Currency.CHF()), r1);

            var r2 = FunctionalAssert.IsSome(Money.ParseOrNone("-$1,000.00", Currency.USD()));
            Assert.Equal(new Money(-1000, Currency.USD()), r2);

            var r3 = FunctionalAssert.IsSome(Money.ParseOrNone("1000", Currency.CHF()));
            Assert.Equal(new Money(1000, Currency.CHF()), r3);
        }

        [Fact]
        public void ThePrecisionCanBeSetToSomethingOtherThanAPowerOfTen()
        {
            var precision05 = new Money(1m, MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.CHF()).WithPrecision(0.05m).Build());
            var precision002 = new Money(1m, MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.CHF()).WithPrecision(0.002m).Build());

            Assert.Collection(
                precision05.Distribute(3).Select(e => e.Evaluate().Amount),
                item => Assert.Equal(0.35m, item),
                item => Assert.Equal(0.35m, item),
                item => Assert.Equal(0.30m, item));

            Assert.Collection(
                precision002.Distribute(3).Select(e => e.Evaluate().Amount),
                item => Assert.Equal(0.334m, item),
                item => Assert.Equal(0.334m, item),
                item => Assert.Equal(0.332m, item));

            var francs = Money.CHF(0.10m);
            Assert.Collection(
                francs.Distribute(3).Select(e => e.Evaluate().Amount),
                item => Assert.Equal(0.05m, item),
                item => Assert.Equal(0.05m, item),
                item => Assert.Equal(0m, item));
        }

        [Fact]
        public void ThePrecisionIsCorrectlyPassedThrough()
        {
            var precision05 = new Money(1m, MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.CHF()).WithPrecision(0.05m).Build());
            var precision002 = new Money(1m, MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.CHF()).WithPrecision(0.002m).Build());

            var x = precision05 with { Amount = 0m };

            Assert.Equal(precision05.Precision, precision05.Distribute(3).First().Evaluate().Precision);
            Assert.Equal(precision002.Precision, precision002.Distribute(3).First().Evaluate().Precision);
        }

        [Fact]
        public void DistributionMustDistributeExactlyTheGivenAmount()
        {
            var francs = Money.CHF(0.08m);

            Assert.Collection(
                francs.Distribute(3).Select(e => e.Evaluate().Amount),
                item => Assert.Equal(0.05m, item),
                item => Assert.Equal(0.03m, item),
                item => Assert.Equal(0m, item));
        }

        [Fact]
        public void DefaultRoundingStrategyIsBankersRounding()
        {
            var francs = Money.CHF(1);
            var evaluationContext = MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.CHF());

            Assert.Equal(MidpointRounding.ToEven, francs.MidpointRounding);
            Assert.Equal(MidpointRounding.ToEven, francs.MidpointRounding);
            Assert.Equal(MidpointRounding.ToEven, RoundingStrategy.BankersRounding);
        }

        [Fact]
        public void WeCanDelegateTheExchangeRatesToABank()
        {
            var fiveFrancs = new Money(5, Currency.CHF());
            var tenDollars = new Money(10, Currency.USD());
            var fiveEuros = new Money(5, Currency.EUR());

            var sum = (fiveFrancs + tenDollars + fiveEuros) * 1.5m;

            var context = MoneyEvaluationContext
                .Builder
                .Default
                .WithTargetCurrency(Currency.CHF())
                .WithBank(OneToOneBank.Instance)
                .Build();

            Assert.Equal(30m, sum.Evaluate(context).Amount);
        }
    }
}
