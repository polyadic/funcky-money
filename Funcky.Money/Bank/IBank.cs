namespace Funcky;

public interface IBank<TUnderlyingType>
{
    TUnderlyingType ExchangeRate(Currency source, Currency target);
}
