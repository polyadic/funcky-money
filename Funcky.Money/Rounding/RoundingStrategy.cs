namespace Funcky;

public static class RoundingStrategy
{
    public static IRoundingStrategy<decimal> NoRounding()
        => new NoRounding<decimal>();

    public static IRoundingStrategy<decimal> BankersRounding(decimal precision)
        => new BankersRounding<decimal>(precision);

    public static IRoundingStrategy<decimal> BankersRounding(Currency currency)
        => new BankersRounding<decimal>(currency);

    public static IRoundingStrategy<decimal> RoundWithAwayFromZero(decimal precision)
        => new RoundWithAwayFromZero<decimal>(precision);

    public static IRoundingStrategy<decimal> RoundWithAwayFromZero(Currency currency)
        => new RoundWithAwayFromZero<decimal>(currency);

    internal static IRoundingStrategy<decimal> Default(decimal precision)
        => BankersRounding(precision);

    internal static IRoundingStrategy<decimal> Default(Currency currency)
        => BankersRounding(currency);
}
