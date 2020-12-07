namespace Funcky
{
    public static class MoneyMultiplicationExtension
    {
        public static IMoneyExpression Multiply(this IMoneyExpression multiplicand, decimal multiplier)
            => new MoneyProduct(multiplicand, multiplier);
    }
}
