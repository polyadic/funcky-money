namespace Funcky
{
    public static class MoneyExtension
    {
        public static IMoneyExpression Add(this IMoneyExpression leftMoneyExpression, IMoneyExpression rightMoneyExpression)
        {
            return new MoneySum(leftMoneyExpression, rightMoneyExpression);
        }

        public static IMoneyExpression Multiply(this IMoneyExpression leftMoneyExpression, decimal factor)
        {
            return new MoneyProduct(leftMoneyExpression, factor);
        }

        public static IMoneyExpression Multiply(this IMoneyExpression leftMoneyExpression, int factor)
        {
            return new MoneyProduct(leftMoneyExpression, factor);
        }

        public static IMoneyExpression Multiply(this IMoneyExpression leftMoneyExpression, double factor)
        {
            return new MoneyProduct(leftMoneyExpression, (decimal)factor);
        }
    }
}
