using System;
using System.Linq;
using Funcky.Monads;
using Xunit;

namespace Funcky.Test
{
    public class MoneyTest
    {
        [Fact]
        public void WeCanCreateAMoneyFromDifferentTypesAndTheAmountIsADecimal()
        {
            var fiveDollars = new Money(5);
            var fiveDollarsFifty = new Money(5.5f);
            var fiveDollarsSeventy = new Money(5.7);
            var fiveDollarsNinety = new Money(5.90m);

            Assert.Equal(5.00m, fiveDollars.Amount);
            Assert.Equal(5.50m, fiveDollarsFifty.Amount);
            Assert.Equal(5.70m, fiveDollarsSeventy.Amount);
            Assert.Equal(5.90m, fiveDollarsNinety.Amount);
        }

        [Fact]
        public void EvaluatingAMoneyInTheSameCurrencyDoesReturnTheSameAmount()
        {
            IMoneyExpression fiveDollars = new Money(5);

            Assert.Equal(fiveDollars, fiveDollars.Evaluate());
        }

        [Fact]
        public void TheSumOfTwoMoneysEvaluatesCorrectly()
        {
            var fiveDollars = new Money(5);
            var tenDollars = new Money(10);

            Assert.Equal(15.00m, fiveDollars.Add(tenDollars).Evaluate().Amount);
        }

        [Fact]
        public void WeCanBuildTheSumOfTwoMoneysWithDifferentCurrenciesButOnEvaluationYouNeedAEvaluationContext()
        {
            var fiveFrancs = new Money(5, Option.Some(Currency.CHF()));
            var tenDollars = new Money(10, Option.Some(Currency.USD()));
            var sum = fiveFrancs.Add(tenDollars);

            Assert.Throws<MissingEvaluationContextException>(() => sum.Evaluate());
        }

        [Fact]
        public void FiveDollarsAreNotFiveFrancs()
        {
            var fiveFrancs = new Money(5, Option.Some(Currency.CHF()));
            var fiveDollars = new Money(5, Option.Some(Currency.USD()));

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
            var fiftyCents = new Money(0.5);
            var sum = fiftyCents.Add(fiftyCents);
            var distribution = sum.Distribute(new[] { 5, 1 });

            Assert.Equal(sum.Evaluate().Amount, distribution.Select(e => e.Evaluate().Amount).Sum());

            Assert.Collection(
                distribution.Select(e => e.Evaluate().Amount),
                item => Assert.Equal(0.84m, item),
                item => Assert.Equal(0.16m, item));
        }

        [Fact]
        public void InputValuesGetRoundedToTheGivenPrecision()
        {
            var fiveDollarsSeventy = new Money(5.7f);
            var midpoint1 = new Money(5.715m);
            var midpoint2 = new Money(5.725m);
            var pi = new Money(Math.PI);

            Assert.Equal(5.70m, fiveDollarsSeventy.Amount);
            Assert.Equal(5.72m, midpoint1.Amount);
            Assert.Equal(5.72m, midpoint2.Amount);
            Assert.Equal(3.14m, pi.Amount);
        }
    }
}
