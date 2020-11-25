namespace Funcky
{
    internal interface IMoneyExpressionVisitor
    {
        public void Visit(Money money);

        public void Visit(MoneySum money);

        public void Visit(MoneyProduct money);

        public void Visit(MoneyDistributionPart money);

        public void Visit(MoneyDistribution money);
    }
}
