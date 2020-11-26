namespace Funcky
{
    public static class MoneyMultiplicationExtension
    {
        public static IMoneyExpression Multiply(this IMoneyExpression moneyExpression, decimal factor)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression Multiply(this IMoneyExpression moneyExpression, int factor)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression Multiply(this IMoneyExpression moneyExpression, double factor)
            => new MoneyProduct(moneyExpression, (decimal)factor);
    }
}
