namespace Funcky
{
    internal interface IMoneyExpressionVisitor
    {
        void Visit(Money money);

        void Visit(MoneySum money);

        void Visit(MoneyProduct money);

        void Visit(MoneyDistributionPart money);
    }
}
