namespace Funcky
{
    internal record MoneyProduct : IMoneyExpression
    {
        public MoneyProduct(IMoneyExpression moneyExpression, decimal factor)
        {
            Expression = moneyExpression;
            Factor = factor;
        }

        public IMoneyExpression Expression { get; }

        public decimal Factor { get; }
    }
}