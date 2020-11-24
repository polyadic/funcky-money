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
    }
}
