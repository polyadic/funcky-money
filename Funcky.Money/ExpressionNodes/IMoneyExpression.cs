namespace Funcky
{
    public interface IMoneyExpression
    {
#if DEFAULT_INTERFACE_IMPLEMENTATION_SUPPORTED
        public static IMoneyExpression operator *(IMoneyExpression moneyExpression, decimal factor)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression operator *(decimal factor, IMoneyExpression moneyExpression)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression operator +(IMoneyExpression leftMoneyExpression, IMoneyExpression rightMoneyExpression)
            => new MoneySum(leftMoneyExpression, rightMoneyExpression);
#endif

        internal void Accept(IMoneyExpressionVisitor visitor);
    }
}
