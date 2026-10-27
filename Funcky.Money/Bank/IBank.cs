namespace Funcky;

public interface IBank
{
    /// <summary>Returns the factor to convert an amount of <paramref name="source"/> into <paramref name="target"/>.</summary>
    /// <exception cref="MissingExchangeRateException">The bank knows no exchange rate for this pair of currencies.</exception>
    decimal ExchangeRate(Currency source, Currency target);
}
