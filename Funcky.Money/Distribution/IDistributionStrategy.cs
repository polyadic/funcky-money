namespace Funcky
{
    internal interface IDistributionStrategy
    {
        Money Distribute(MoneyDistributionPart part, Money total);
    }
}
