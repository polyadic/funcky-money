namespace Funcky;

public interface IBank
{
    decimal ExchangeRate(Currency source, Currency target);
}
