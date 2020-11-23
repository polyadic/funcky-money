namespace Funcky
{
    internal record MoneyProduct : IMoneyExpression
    {
        public MoneyProduct(IMoneyExpression leftMoneyExpression, decimal factor)
        {
            Left = leftMoneyExpression;
            Factor = factor;
        }

        public IMoneyExpression Left { get; }
        public decimal Factor { get; }
    }
}
