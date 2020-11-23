using Xunit;

namespace Funcky.Test
{
    public class MoneyTest
    {
        [Fact]
        public void YouCanCreateAMoneyObject()
        {
            var fiveDollars = new Money(5);
        }
    }
}
