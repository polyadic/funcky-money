namespace Funcky
{
    internal static class DecimalRounding
    {
        public static decimal Truncate(decimal amount, decimal precision)
            => decimal.Truncate(amount / precision) * precision;
    }
}
