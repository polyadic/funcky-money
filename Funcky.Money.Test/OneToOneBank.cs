namespace Funcky.Test
{
    // Bank which returns a 1:1 exchange rate for every pair of currencies.
    internal class OneToOneBank : IBank
    {
        public static readonly IBank Instance = new OneToOneBank();

        public decimal ExchangeRate(Currency source, Currency target)
            => 1m;
    }
}
