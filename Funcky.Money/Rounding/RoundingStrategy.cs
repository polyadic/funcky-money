namespace Funcky;

public static class RoundingStrategy
{
    public static IRoundingStrategy Default(decimal precision)
        => BankersRounding(precision);

    public static IRoundingStrategy Default(Currency currency)
        => Default(Power.OfATenth(currency.MinorUnitDigits));

    public static IRoundingStrategy BankersRounding(decimal precision)
        => new BankersRounding(precision);

    public static IRoundingStrategy RoundWithAwayFromZero(decimal precision)
        => new RoundWithAwayFromZero(precision);

    public static IRoundingStrategy NoRounding()
        => new NoRounding();
}
