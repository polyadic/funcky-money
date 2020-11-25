namespace Funcky
{
    public static class MoneyMultiplicationExtension
    {
        public static IMoneyExpression Multiply(this IMoneyExpression leftMoneyExpression, decimal factor)
            => new MoneyProduct(leftMoneyExpression, factor);

        public static IMoneyExpression Multiply(this IMoneyExpression leftMoneyExpression, int factor)
            => new MoneyProduct(leftMoneyExpression, factor);

        public static IMoneyExpression Multiply(this IMoneyExpression leftMoneyExpression, double factor)
            => new MoneyProduct(leftMoneyExpression, (decimal)factor);
    }
}
