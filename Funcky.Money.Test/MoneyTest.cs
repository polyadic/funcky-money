using Xunit;

namespace Funcky.Test
{
    public class MoneyTest
    {
        [Fact]
        public void YouCanCreateAMoneyObject()
        {
            var fiveDollars = new Money(5);

            Assert.Equal(5.00m, fiveDollars.Amount);
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
    }
}
