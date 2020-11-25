namespace Funcky
{
    public static class MoneySummationExtension
    {
        public static IMoneyExpression Add(this IMoneyExpression leftMoneyExpression, IMoneyExpression rightMoneyExpression)
            => new MoneySum(leftMoneyExpression, rightMoneyExpression);
    }
}
