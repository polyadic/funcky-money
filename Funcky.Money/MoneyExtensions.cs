using System;

namespace Funcky
{
    public static class MoneyExtensions
    {
        public static Money Evaluate(this IMoneyExpression moneyExpression)
            => moneyExpression switch
            {
                Money money => money,
                MoneySum sum => new Money(sum.Left.Evaluate().Amount + sum.Right.Evaluate().Amount),
                MoneyProduct product => new Money(product.Left.Evaluate().Amount * product.Factor),
                _ => throw new NotImplementedException(),
            };

        public static IMoneyExpression Add(this IMoneyExpression leftMoneyExpression, IMoneyExpression rightMoneyExpression)
        {
            return new MoneySum(leftMoneyExpression, rightMoneyExpression);
        }

        public static IMoneyExpression Multiply(this IMoneyExpression leftMoneyExpression, decimal factor)
        {
            return new MoneyProduct(leftMoneyExpression, factor);
        }
    }
}
