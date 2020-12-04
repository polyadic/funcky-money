namespace Funcky
{
    public static class RoundingStrategy
    {
        public static AbstractRoundingStrategy NoRounding(decimal precision)
            => new NoRounding(precision);

        public static AbstractRoundingStrategy NoRounding(Currency currency)
            => new NoRounding(currency);

        public static AbstractRoundingStrategy BankersRounding(decimal precision)
            => new BankersRounding(precision);

        public static AbstractRoundingStrategy BankersRounding(Currency currency)
            => new BankersRounding(currency);

        public static AbstractRoundingStrategy RoundWithAwayFromZero(decimal precision)
            => new RoundWithAwayFromZero(precision);

        public static AbstractRoundingStrategy RoundWithAwayFromZero(Currency currency)
            => new RoundWithAwayFromZero(currency);

        internal static AbstractRoundingStrategy Default(decimal precision)
            => BankersRounding(precision);

        internal static AbstractRoundingStrategy Default(Currency currency)
            => BankersRounding(currency);
    }
}
