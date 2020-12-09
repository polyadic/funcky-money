namespace Funcky
{
    internal interface IMoneyExpressionVisitor
    {
        void Visit(Money money);

        void Visit(MoneySum sum);

        void Visit(MoneyProduct product);

        void Visit(MoneyDistributionPart part);
    }
}
