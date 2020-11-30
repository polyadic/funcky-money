namespace Funcky
{
    public interface IBank
    {
        public decimal ExchangeRate(Currency source, Currency target);
    }
}
