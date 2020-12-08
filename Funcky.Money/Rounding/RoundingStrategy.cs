namespace Funcky
{
    public static class RoundingStrategy
    {
        public static IRoundingStrategy NoRounding()
            => new NoRounding();

        public static IRoundingStrategy BankersRounding(decimal precision)
            => new BankersRounding(precision);

        public static IRoundingStrategy BankersRounding(Currency currency)
            => new BankersRounding(currency);

        public static IRoundingStrategy RoundWithAwayFromZero(decimal precision)
            => new RoundWithAwayFromZero(precision);

        public static IRoundingStrategy RoundWithAwayFromZero(Currency currency)
            => new RoundWithAwayFromZero(currency);

        internal static IRoundingStrategy Default(decimal precision)
            => BankersRounding(precision);

        internal static IRoundingStrategy Default(Currency currency)
            => BankersRounding(currency);
    }
}
